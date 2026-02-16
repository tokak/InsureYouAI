using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Text;

namespace InsureYouAINew.Models
{
    public class ChatHub : Hub
    {
        private readonly IConfiguration _configuration;
        private static readonly ConcurrentDictionary<string, List<Content>> _history = new();
        private const int MaxHistoryCount = 100;

        public ChatHub(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public override Task OnConnectedAsync()
        {
            // Yeni bağlantıda temiz bir liste oluştur
            _history[Context.ConnectionId] = new List<Content>();
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            // Bellek sızıntısını önlemek için veriyi temizle
            _history.TryRemove(Context.ConnectionId, out _);
            return base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string userMessage)
        {
            if (string.IsNullOrWhiteSpace(userMessage)) return;

            // 1. Kullanıcıya kendi mesajını UI'da göstermesi için geri yolla
            await Clients.Caller.SendAsync("ReceiveUserEcho", userMessage);

            // 2. Geçmişi al (yoksa oluştur)
            var history = _history.GetOrAdd(Context.ConnectionId, _ => new List<Content>());

            // 3. Kullanıcı mesajını geçmişe ekle
            history.Add(new Content
            {
                Role = "user",
                Parts = new List<Part> { new Part { Text = userMessage } }
            });

            try
            {
                await StreamGemini(history);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("ReceiveError", "AI Hatası: " + ex.Message);
            }
        }

        private async Task StreamGemini(List<Content> history)
        {
            var apiKey = _configuration["Gemini:ApiKey"];
            var client = new Client(apiKey: apiKey);
            var sb = new StringBuilder();
            var cancellationToken = Context.ConnectionAborted;

            // Geçmişi sınırla (Token tasarrufu ve performans için)
            var recentHistory = history.TakeLast(MaxHistoryCount).ToList();

            try
            {
                // Gemini 2.0/3.0 Modellerinde SystemInstruction kullanımı önerilir
                var generateConfig = new GenerateContentConfig
                {
                    SystemInstruction = new Content
                    {
                        Parts = new List<Part> { new Part { Text = "Sen profesyonel bir sigorta asistanısın. Kısa, öz ve yardımcı cevaplar ver. Karmaşık terimleri basitleştir." } }
                    },
                    MaxOutputTokens = 1000,
                    Temperature = 0.3f
                };

                // Akışı başlat (Model ismini ortamınıza göre "gemini-2.0-flash" veya "gemini-1.5-flash" olarak güncelleyebilirsiniz)
                var responseStream = client.Models.GenerateContentStreamAsync(
                    model: "gemini-2.5-flash-lite",
                    contents: recentHistory,
                    config: generateConfig,
                    cancellationToken: cancellationToken);

                await foreach (var response in responseStream)
                {
                    var text = response.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

                    if (!string.IsNullOrEmpty(text))
                    {
                        sb.Append(text);
                        // SignalR üzerinden her bir kelimeyi (token) anlık gönder
                        await Clients.Caller.SendAsync("ReceiveToken", text, cancellationToken);
                    }
                }

                // 4. Modelin tam cevabını geçmişe ekle (Bir sonraki soru için bağlam oluşturur)
                var fullResponse = sb.ToString();
                history.Add(new Content
                {
                    Role = "model",
                    Parts = new List<Part> { new Part { Text = fullResponse } }
                });

                // İşlemin bittiğini bildir
                await Clients.Caller.SendAsync("CompleteMessage", fullResponse);
            }
            catch (OperationCanceledException)
            {
                // Kullanıcı tarayıcıyı kapattığında veya bağlantıyı kestiğinde sessizce çık
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("ReceiveError", "Akış hatası: " + ex.Message);
            }
        }
    }
}
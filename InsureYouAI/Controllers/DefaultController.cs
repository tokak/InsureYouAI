using InsureYouAI.Context;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Nodes;
using System.Text;
using InsureYouAI.Entities;
using System.Text.Json;
using MimeKit;
using System.Net.Http.Headers;
using MailKit.Net.Smtp;
using Google.GenAI.Types;
using Google.GenAI;

namespace InsureYouAI.Controllers
{
    public class DefaultController : Controller
    {
        private readonly InsureContext _context;
        private readonly IConfiguration _configuration;
        public DefaultController(InsureContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        public IActionResult Index()
        {
            return View();
        }

        public PartialViewResult SendMessage()
        {
            return PartialView();
        }
        [HttpPost]
        public async Task<IActionResult> SendMessage(Message message)
        {
            // 1. Gelen mesajı veritabanına kaydet
            message.SendDate = DateTime.Now;
            message.IsRead = false;
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            string textContent = "AI yanıtı oluşturulamadı.";

            #region Gemini_AI_Analiz
            try
            {
                var apiKey = _configuration["Gemini:ApiKey"]; // Config'den güvenli şekilde alıyoruz
                var client = new Client(apiKey: apiKey);

                // Gemini için prompt ve sistem talimatı hazırlığı
                var generateConfig = new GenerateContentConfig
                {
                    SystemInstruction = new Content
                    {
                        Parts = new List<Part> { new Part { Text = "Sen bir sigorta firmasının müşteri iletişim asistanısın. Kurumsal ama samimi bir dille yaz. Yanıtları 2-3 paragrafla sınırla." } }
                    },
                    MaxOutputTokens = 1024,
                    Temperature = 0.5f // Biraz daha yaratıcı ama kontrollü bir ton
                };

                var userContent = new Content
                {
                    Role = "user",
                    Parts = new List<Part> { new Part { Text = message.MessagetDetail } }
                };

                // Tekil yanıt alıyoruz (API akışı yerine doğrudan sonuç)
                var response = await client.Models.GenerateContentAsync(
                    model: "gemini-2.0-flash", // En güncel ve hızlı model
                    contents: new List<Content> { userContent },
                    config: generateConfig
                );

                textContent = response.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text ?? "Yanıt boş döndü.";
            }
            catch (Exception ex)
            {
                // Hata durumunda loglama yapıp varsayılan mesajı bırakıyoruz
                Console.WriteLine("Gemini API Hatası: " + ex.Message);
                return Content($"Gemini API Hatası: {ex.Message}");
            }
            #endregion

            #region Email_Gönderme
            try
            {
                MimeMessage mimeMessage = new MimeMessage();
                mimeMessage.From.Add(new MailboxAddress("InsureYouAI Admin", "murattokak827@gmail.com"));
                mimeMessage.To.Add(new MailboxAddress(message.NameSurname, message.Email));
                mimeMessage.Subject = "InsureYouAI Email Yanıtı";

                var bodyBuilder = new BodyBuilder { TextBody = textContent };
                mimeMessage.Body = bodyBuilder.ToMessageBody();

                using (var client2 = new MailKit.Net.Smtp.SmtpClient())
                {
                    await client2.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                    await client2.AuthenticateAsync("murattokak827@gmail.com", "");
                    await client2.SendAsync(mimeMessage);
                    await client2.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Email Hatası: " + ex.Message);
            }
            #endregion

            #region GeminiAIMessage_DbKayıt
            ClaudeAIMessage geminiAIMessage = new ClaudeAIMessage() // Tablo isminiz Claude olarak kalmış olabilir, sorun değil
            {
                MessageDetail = textContent,
                ReceiveEmail = message.Email,
                ReceiveNameSurname = message.NameSurname,
                SendDate = DateTime.Now
            };

            _context.ClaudeAIMessages.Add(geminiAIMessage);
            await _context.SaveChangesAsync();
            #endregion

            return RedirectToAction("Index");
        }



        public PartialViewResult SubscribeEmail()
        {
            return PartialView();
        }

        [HttpPost]
        public IActionResult SubscribeEmail(string email)
        {
            return View();
        }

    }

}

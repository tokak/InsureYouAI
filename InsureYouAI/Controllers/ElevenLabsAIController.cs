using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace InsureYouAINew.Controllers
{
    public class ElevenLabsAIController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        private readonly string _apiKey;
        private readonly string _voiceId;
        private readonly string _baseUrl;

        public ElevenLabsAIController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;

            _apiKey = _configuration["ElevenLabs:ApiKey"];
            _voiceId = _configuration["ElevenLabs:VoiceId"];
            _baseUrl = _configuration["ElevenLabs:BaseUrl"];
        }

        /// <summary>
        /// Sigorta asistanı metnini ElevenLabs API kullanarak sese çevirir
        /// ve oluşturulan ses dosyasını UI'da oynatılmak üzere kaydeder.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SpeakInsuranceAnswer(string text)
        {
            ViewBag.ControllerName = "Yapay Zeka Sigorta Asistanı";
            ViewBag.PageName = "Sesli Yanıt Alanı";

            if (string.IsNullOrWhiteSpace(text))
            {
                ViewBag.Error = "Lütfen bir metin girin.";
                return View();
            }

            string aiResponse = $"InsureYOU AI yanıtı: {text}";
            ViewBag.Answer = aiResponse;

            var audioUrl = await GenerateSpeechAsync(aiResponse);

            if (audioUrl == null)
            {
                ViewBag.Error = "Ses oluşturulamadı.";
                return View();
            }

            ViewBag.AudioUrl = audioUrl;
            return View();
        }

        /// <summary>
        /// ElevenLabs API ile verilen metni sese dönüştürür ve mp3 dosyası olarak kaydeder.
        /// Başarılı olursa dosya yolunu döner.
        /// </summary>
        private async Task<string?> GenerateSpeechAsync(string text)
        {
            var client = _httpClientFactory.CreateClient();

            var url = $"{_baseUrl}/{_voiceId}/stream";

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.TryAddWithoutValidation("xi-api-key", _apiKey);

            var payload = new
            {
                text = text,
                model_id = "eleven_multilingual_v2"
            };

            var json = System.Text.Json.JsonSerializer.Serialize(payload);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return null;

            var audioBytes = await response.Content.ReadAsByteArrayAsync();

            var fileName = $"voice_{Guid.NewGuid()}.mp3";
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/voices", fileName);

            Directory.CreateDirectory("wwwroot/voices");

            await System.IO.File.WriteAllBytesAsync(path, audioBytes);

            return "/voices/" + fileName;
        }
    }
}

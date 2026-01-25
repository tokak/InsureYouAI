using InsureYouAI.Context;
using InsureYouAI.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;

namespace InsureYouAI.Controllers
{
    public class AboutController : Controller
    {
        private readonly InsureContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        public AboutController(InsureContext context, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }
        public IActionResult AboutList()
        {
            ViewBag.ControllerName = "Hakkımızda";
            ViewBag.PageName = "Mevcut Hakkımızda Yazısı";
            var values = _context.Abouts.ToList();
            return View(values);
        }

        [HttpGet]
        public IActionResult CreateAbout()
        {
            ViewBag.ControllerName = "Hakkımızda";
            ViewBag.PageName = "Yeni Hakkımızda Yazı Girişi (Tema Bütünlüğünü Korumak İçin 1 Adet Hakkımızda Yazısı Giriniz)";
            return View();
        }

        [HttpPost]
        public IActionResult CreateAbout(About about)
        {
            _context.Abouts.Add(about);
            _context.SaveChanges();
            return RedirectToAction("AboutList");
        }

        public IActionResult DeleteAbout(int id)
        {
            var value = _context.Abouts.Find(id);
            _context.Abouts.Remove(value);
            _context.SaveChanges();
            return RedirectToAction("AboutList");
        }

        [HttpGet]
        public IActionResult UpdateAbout(int id)
        {
            ViewBag.ControllerName = "Hakkımızda";
            ViewBag.PageName = "Hakkımızda Yazı Güncelleme Sayfası";
            var value = _context.Abouts.Find(id);
            return View(value);
        }

        [HttpPost]
        public IActionResult UpdateAbout(About about)
        {
            _context.Abouts.Update(about);
            _context.SaveChanges();
            return RedirectToAction("AboutList");
        }


        [HttpGet]
        public async Task<IActionResult> CreateAboutWithGoogleGemini()
        {
            var apiKey = _configuration["Gemini:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                ViewBag.value = "Gemini API Key bulunamadı.";
                return View();
            }

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-lite:generateContent?key={apiKey}";

            var requestBody = new
            {
                contents = new[]
                {
            new
            {
                parts = new[]
                {
                    new
                    {
                        text = "Kurumsal bir sigorta firması için etkileyici, güven verici ve profesyonel bir 'Hakkımızda' yazısı oluştur."
                    }
                }
            }
        }
            };

            using var httpClient = new HttpClient();
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.value = "Gemini API yanıt vermedi.";
                return View();
            }

            var responseJson = await response.Content.ReadAsStringAsync();

            using var jsonDoc = JsonDocument.Parse(responseJson);
            var aboutText = jsonDoc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            ViewBag.value = aboutText;

            return View();
        }

    }

}


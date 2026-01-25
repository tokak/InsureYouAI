using InsureYouAI.Context;
using InsureYouAI.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace InsureYouAI.Controllers
{
    public class ServiceController : Controller
    {
        private readonly InsureContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        public ServiceController(InsureContext context, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }
        public IActionResult ServiceList()
        {
            ViewBag.ControllerName = "Hizmelet";
            ViewBag.PageName = "Mevcut Sigorta Hizmetleri Listesi";
            var values = _context.Services.ToList();
            return View(values);
        }

        [HttpGet]
        public IActionResult CreateService()
        {
            ViewBag.ControllerName = "Hizmetler";
            ViewBag.PageName = "Yeni Hizmet Yazısı Girişi";
            return View();
        }

        [HttpPost]
        public IActionResult CreateService(Service service)
        {
            _context.Services.Add(service);
            _context.SaveChanges();
            return RedirectToAction("ServiceList");
        }

        [HttpGet]
        public IActionResult UpdateService(int id)
        {
            ViewBag.ControllerName = "Hizmetler";
            ViewBag.PageName = "Hizmet Yazısı Güncelleme Sayfası";
            var value = _context.Services.Find(id);
            return View(value);
        }

        [HttpPost]
        public IActionResult UpdateService(Service service)
        {
            _context.Services.Update(service);
            _context.SaveChanges();
            return RedirectToAction("ServiceList");
        }

        public IActionResult DeleteService(int id)
        {
            var value = _context.Services.Find(id);
            _context.Services.Remove(value);
            _context.SaveChanges();
            return RedirectToAction("ServiceList");
        }

        public async Task<IActionResult> CreateServiceWithGemini()
        {
            var apiKey = _configuration["Gemini:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                ViewBag.services = new List<string> { "Gemini API Key bulunamadı." };
                return View();
            }

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-lite:generateContent?key={apiKey}";

            string prompt =
                "Bir sigorta şirketi için hizmetler bölümü hazırla. " +
                "Maksimum 100 karakterden oluşan, numaralı olacak şekilde 5 farklı hizmet cümlesi yaz.";

            var requestBody = new
            {
                contents = new[]
                {
            new
            {
                parts = new[]
                {
                    new { text = prompt }
                }
            }
        }
            };

            using var client = new HttpClient();
            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            var response = await client.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.services = new List<string>
        {
            $"Gemini API'den cevap alınamadı. Hata: {response.StatusCode}"
        };
                return View();
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);

            var fullText = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            // Satır satır ayır, numaraları temizle
            var services = fullText!
                .Split('\n')
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.TrimStart('1', '2', '3', '4', '5', '.', ' ', '-', ')'))
                .ToList();

            ViewBag.services = services;

            return View();
        }

    }
}

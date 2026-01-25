using InsureYouAI.Context;
using InsureYouAI.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;

namespace InsureYouAI.Controllers
{
    public class TestimonialController : Controller
    {
        private readonly InsureContext _context;
        private readonly IConfiguration _configuration;
        public TestimonialController(InsureContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        public IActionResult TestimonialList()
        {
            ViewBag.ControllerName = "Referanslar";
            ViewBag.PageName = "Referanslar Tarafından Oluşuturulan Yazılar";
            var values = _context.Testimonials.ToList();
            return View(values);
        }

        [HttpGet]
        public IActionResult CreateTestimonial()
        {
            ViewBag.ControllerName = "Referanslar";
            ViewBag.PageName = "Yeni Referans Yazısı";
            return View();
        }

        [HttpPost]
        public IActionResult CreateTestimonial(Testimonial testimonial)
        {
            _context.Testimonials.Add(testimonial);
            _context.SaveChanges();
            return RedirectToAction("TestimonialList");
        }

        [HttpGet]
        public IActionResult UpdateTestimonial(int id)
        {
            ViewBag.ControllerName = "Referanslar";
            ViewBag.PageName = "Referans Yazısı Güncelleme Sayfası";
            var value = _context.Testimonials.Find(id);
            return View(value);
        }

        [HttpPost]
        public IActionResult UpdateTestimonial(Testimonial testimonial)
        {
            _context.Testimonials.Update(testimonial);
            _context.SaveChanges();
            return RedirectToAction("TestimonialList");
        }

        public IActionResult DeleteTestimonial(int id)
        {
            var value = _context.Testimonials.Find(id);
            _context.Testimonials.Remove(value);
            _context.SaveChanges();
            return RedirectToAction("TestimonialList");
        }

        public async Task<IActionResult> CreateTestimonialWithGemini()
        {
            var apiKey = _configuration["Gemini:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                ViewBag.testimonials = new List<string>
        {
            "Gemini API Key bulunamadı."
        };
                return View();
            }

            var url =
                $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-lite:generateContent?key={apiKey}";

            string prompt =
                "Bir sigorta şirketi için müşteri deneyimlerine dair (testimonial) içerik oluştur. " +
                "Türkçe olacak şekilde; her biri müşteri yorumu, müşteri adı-soyadı ve unvan içeren " +
                "numaralı 6 farklı testimonial hazırla.";

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
                ViewBag.testimonials = new List<string>
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

            // Satırlara ayır ve numaraları temizle
            var testimonials = fullText!
                .Split('\n')
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.TrimStart('1', '2', '3', '4', '5', '6', '.', ' ', '-', ')'))
                .ToList();

            ViewBag.testimonials = testimonials;

            return View();
        }

    }
}

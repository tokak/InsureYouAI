using InsureYouAI.Context;
using InsureYouAI.Dtos;
using InsureYouAI.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace InsureYouAI.Controllers
{
    public class ArticleController : Controller
    {
        private readonly InsureContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        public ArticleController(InsureContext context, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public IActionResult ArticleList()
        {
            ViewBag.ControllerName = "Makaleler";
            ViewBag.PageName = "Makale Listesi";
            var values = _context.Articles.Include(x => x.AppUser).Include(y => y.Category).ToList();
            return View(values);
        }

        [HttpGet]
        public IActionResult CreateArticle()
        {
            ViewBag.ControllerName = "Makaleler";
            ViewBag.PageName = "Yeni Makale Oluştur";

            var categories = _context.Categories
                           .Select(x => new SelectListItem
                           {
                               Text = x.CategoryName,
                               Value = x.CategoryId.ToString()
                           })
                           .ToList();

            ViewBag.Categories = categories;

            var authors = _context.Users
                         .Select(x => new SelectListItem
                         {
                             Text = x.Name + " " + x.Surname,
                             Value = x.Id
                         })
                         .ToList();

            ViewBag.Authors = authors;

            return View();
        }

        [HttpPost]
        public IActionResult CreateArticle(Article article)
        {
            article.CreatedDate = DateTime.Now;
            _context.Articles.Add(article);
            _context.SaveChanges();
            return RedirectToAction("ArticleList");
        }

        [HttpGet]
        public IActionResult UpdateArticle(int id)
        {

            ViewBag.ControllerName = "Makaleler";
            ViewBag.PageName = "Makale Güncelleme Sayfası";

            var categories = _context.Categories
                           .Select(x => new SelectListItem
                           {
                               Text = x.CategoryName,
                               Value = x.CategoryId.ToString()
                           })
                           .ToList();

            ViewBag.Categories = categories;

            var authors = _context.Users
                         .Select(x => new SelectListItem
                         {
                             Text = x.Name + " " + x.Surname,
                             Value = x.Id
                         })
                         .ToList();

            ViewBag.Authors = authors;

            var value = _context.Articles.Find(id);
            return View(value);
        }

        [HttpPost]
        public IActionResult UpdateArticle(Article article)
        {
            _context.Articles.Update(article);
            _context.SaveChanges();
            return RedirectToAction("ArticleList");
        }

        public IActionResult DeleteArticle(int id)
        {
            var value = _context.Articles.Find(id);
            _context.Articles.Remove(value);
            _context.SaveChanges();
            return RedirectToAction("ArticleList");
        }

        [HttpGet]
        public IActionResult CreateArticleWithGemini()
        {
            ViewBag.ControllerName = "Makaleler";
            ViewBag.PageName = "Yapay Zeka Makale Oluşturucu";
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateArticleWithGemini(string prompt)
        {
            var apiKey = _configuration["Gemini:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                ViewBag.article = "Gemini API Key bulunamadı. appsettings.json -> Gemini:ApiKey kontrol et.";
                return View();
            }

            if (string.IsNullOrWhiteSpace(prompt))
            {
                ViewBag.article = "Lütfen bir prompt girin.";
                return View();
            }

            // ✅ Key URL’de değil, header’da
            var url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-lite:generateContent";


            var systemInstruction =
               "Sen bir sigorta şirketi için içerik üreten bir yapay zekasın. " +
                "Kullanıcının verdiği özet ve anahtar kelimelere göre SADECE makale metnini üret. " +
                "Başlık yazma. Giriş cümlesi ekleme. Açıklama yapma. " +
                "Markdown (##, **, ---) kullanma. " +
                "Selamlama, yorum veya taslak ifadesi ekleme. " +
                "Doğrudan makale içeriğini düz metin olarak yaz. ";

            var requestBody = new GeminiRequestDto
            {
                contents = new List<Content>
        {
            new Content
            {
                parts = new List<Part>
                {
                    new Part { text = $"{systemInstruction}\n\nKullanıcı isteği:\n{prompt}" }
                }
            }
        },
                generationConfig = new GenerationConfig
                {
                    temperature = 0.7
                    // maxOutputTokens = 1200
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            using var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Remove("X-goog-api-key");
            client.DefaultRequestHeaders.Add("X-goog-api-key", apiKey);

            HttpResponseMessage response;
            try
            {
                response = await client.PostAsync(url, httpContent);
            }
            catch (Exception ex)
            {
                ViewBag.article = $"İstek gönderilemedi: {ex.Message}";
                return View();
            }

            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                // 429 için özel mesaj (kota/limit)
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    ViewBag.article = "Çok fazla istek atıldı (429). Kotanı/limitlerini kontrol et. " + responseBody;
                    return View();
                }

                ViewBag.article = $"Bir hata oluştu: {(int)response.StatusCode} {response.StatusCode} - {responseBody}";
                return View();
            }

            var result = JsonSerializer.Deserialize<GeminiResponseDto>(responseBody);

            var aiText = result?
                .candidates?
                .FirstOrDefault()?
                .content?
                .parts?
                .FirstOrDefault()?
                .text;

            ViewBag.article = string.IsNullOrWhiteSpace(aiText)
                ? "Gemini yanıtı boş döndü."
                : aiText;

            return View();
        }


    }
}

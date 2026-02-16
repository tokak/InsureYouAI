using InsureYouAI.Context;
using InsureYouAI.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Google.GenAI.Types;
using Google.GenAI;
using System;

namespace InsureYouAI.Controllers
{
    public class AppUserController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly InsureContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        public AppUserController(InsureContext context, IConfiguration configuration, IHttpClientFactory httpClientFactory, UserManager<AppUser> userManager)
        {
            _context = context;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _userManager = userManager;
        }

        public IActionResult UserList()
        {
            var values = _userManager.Users.ToList();
            return View(values);
        }

        public async Task<IActionResult> UserProfileWithAI(string id)
        {
            // 1️⃣ Kullanıcı bilgilerini çek
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            ViewBag.name = user.Name;
            ViewBag.surname = user.Surname;
            ViewBag.imageUrl = user.ImageUrl;
            ViewBag.description = user.Description;
            ViewBag.titlevalue = user.Title;
            ViewBag.city = user.City;
            ViewBag.education = user.Education;

            // 2️⃣ Kullanıcıya ait makaleleri çek
            var articles = await _context.Articles
                                         .Where(x => x.AppUserId == id)
                                         .Select(y => y.Content)
                                         .ToListAsync();

            if (articles == null || articles.Count == 0)
            {
                ViewBag.AIResult = "Bu kullanıcıya ait analiz yapılacak makale bulunamadı!";
                return View(user);
            }

            var allArticles = string.Join("\n\n", articles);

            // 3️⃣ Gemini API Key kontrolü
            var apiKey = _configuration["Gemini:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                ViewBag.AIResult = "Gemini API Key bulunamadı.";
                return View(user);
            }

            try
            {
                // 4️⃣ Gemini Client ve Prompt Hazırlığı
                var client = new Client(apiKey: apiKey);

                string prompt = $@"
            Siz bir sigorta sektöründe uzman bir içerik analistisin.
            Elinizde, bir sigorta şirketinin çalışanının yazdığı tüm makaleler var.
            Bu makaleler üzerinden çalışanın içerik üretim tarzını analiz et.

            Analiz Başlıkları:
            1) Konu çeşitliliği ve odak alanları (sağlık, hayat, kasko, tamamlayıcı, BES vb.)
            2) Hedef kitle tahmini (bireysel/kurumsal, segment, persona)
            3) Dil ve Anlatım Tarzı (tekniklik seviyesi, okunabilirlik, ikna gücü)
            4) Sigorta terimlerini kullanma ve doğruluk düzeyi
            5) Müşteri ihtiyaçlarına ve risk yönetimine odaklanma
            6) Pazarlama/satış vurgusu, CTA netliği
            7) Geliştirilmesi gereken alanlar ve net aksiyon maddeleri

            Makaleler:
            {allArticles}

            Lütfen çıktıyı profesyonel rapor formatında, madde madde ve en sonda 5 maddelik aksiyon listesi ile ver.";

                // 5️⃣ API Çağrısı (Yeni SDK Formatı)
                var response = await client.Models.GenerateContentAsync(
                    model: "gemini-3-flash-preview", 
                    contents: prompt,
                    config: new GenerateContentConfig
                    {
                        MaxOutputTokens = 500, // Analiz raporu uzun olabileceği için artırdık
                        Temperature = 0.2f
                    }
                );

                // 6️⃣ Yanıtı Al
                var resultText = response.Candidates?[0].Content?.Parts?[0].Text;
                ViewBag.AIResult = resultText ?? "Gemini boş yanıt döndü.";
            }
            catch (Exception ex)
            {
                // Hata durumunda kullanıcıya bilgi ver
                ViewBag.AIResult = $"Analiz sırasında bir hata oluştu: {ex.Message}";
            }

            return View(user);
        }

        public async Task<IActionResult> UserCommentsProfileWithAI(string id)
        {
            // 1️⃣ Kullanıcı bilgilerini çek
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // ViewBag atamaları (mevcut yapını korudum)
            ViewBag.name = user.Name;
            ViewBag.surname = user.Surname;
            ViewBag.imageUrl = user.ImageUrl;
            ViewBag.description = user.Description;
            ViewBag.titlevalue = user.Title;
            ViewBag.city = user.City;
            ViewBag.education = user.Education;

            // 2️⃣ Kullanıcının yorumlarını çek
            var comments = await _context.Comments
                                         .Where(x => x.AppUserId == id)
                                         .Select(y => y.CommentDetail)
                                         .ToListAsync();

            if (comments.Count == 0)
            {
                ViewBag.AIResult = "Bu kullanıcıya ait analiz yapılacak yorum bulunamadı!";
                return View(user);
            }

            var allComments = string.Join("\n\n", comments);

            // 3️⃣ Gemini Client Hazırlığı
            var apiKey = _configuration["Gemini:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                ViewBag.AIResult = "Gemini API Key bulunamadı.";
                return View(user);
            }

            try
            {
                var client = new Client(apiKey: apiKey);

                // 4️⃣ Prompt ve Config Hazırlığı
                string prompt = $@"
            Sen kullanıcı davranış analizi yapan bir yapay zeka uzmanısın.
            Aşağıdaki yorumlara göre kullanıcıyı analiz et.

            Analiz Başlıkları:
            1) Genel Duygu Durumu (pozitif/negatif/nötr)
            2) Toksik içerik analizi
            3) İlgi alanları
            4) İletişim tarzı
            5) Geliştirilmesi gereken alanlar
            6) 5 Maddelik kısa özet

            Yorumlar:
            {allComments}";

                var config = new GenerateContentConfig
                {
                    MaxOutputTokens = 500,
                    Temperature = 0.2f
                };

                // 5️⃣ API Çağrısı (SDK üzerinden)
                // Daha garantici yaklaşım:
                var response = await client.Models.GenerateContentAsync(
                    model: "gemini-3-flash-preview",
                    contents: new List<Content> { new Content { Parts = new List<Part> { new Part { Text = prompt } } } },
                    config: config
                );

                // 6️⃣ Yanıtı Al
                // SDK sayesinde JSON parse etmekle uğraşmıyoruz
                var resultText = response.Candidates?[0].Content?.Parts?[0].Text;

                ViewBag.AIResult = resultText ?? "Gemini boş yanıt döndü.";
            }
            catch (Exception ex)
            {
                ViewBag.AIResult = "AI Analizi sırasında bir hata oluştu: " + ex.Message;
            }

            return View(user);
        }
    }
}
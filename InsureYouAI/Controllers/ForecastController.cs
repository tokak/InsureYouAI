using InsureYouAI.Context;
using InsureYouAI.Services;
using Microsoft.AspNetCore.Mvc;

namespace InsureYouAINew.Controllers
{
    public class ForecastController : Controller
    {
        private readonly InsureContext _context;

        // ML tahmin servisi
        private readonly ForecastService _forecastService;

        public ForecastController(InsureContext context)
        {
            _context = context;

            // Forecast servisi oluşturuluyor (ML işlemleri burada yapılacak)
            _forecastService = new ForecastService();
        }

        public IActionResult Index()
        {
            //  Veritabanındaki poliçeleri al
            // Yıl + Ay bazında grupla (aylık satış sayısı çıkarıyoruz)
            var salesData = _context.Policies
                .GroupBy(p => new { p.StartDate.Year, p.StartDate.Month }) // aynı ayları grupla
                .Select(g => new
                {
                    Year = g.Key.Year,   // yıl bilgisi
                    Month = g.Key.Month, // ay bilgisi
                    Count = g.Count()    // o ay satılan poliçe sayısı
                })
                .AsEnumerable()
                // ML modelinin anlayacağı formata çeviriyoruz
                .Select(g => new PolicySalesData
                {
                    // Her ay için temsil tarihi oluşturuyoruz (ayın 1'i)
                    Date = new DateTime(g.Year, g.Month, 1),
                    // O ayın satış adedi
                    SaleCount = g.Count
                })

                // ML zaman serisi çalışabilmesi için sıralama şart
                .OrderBy(x => x.Date)

                // Listeye çeviriyoruz
                .ToList();


            // ML modeline geçmiş satış verisini veriyoruz
            // horizon: 3 → gelecek 3 ay tahmin yapılacak
            var forecast = _forecastService.GetForecast(salesData, horizon: 3);

            // Tahmin sonuçlarını View'a gönderiyoruz
            // (ForecastedValues, Min, Max burada var)
            ViewBag.Forecast = forecast;


            // Geçmiş satış verisini de View'a gönderiyoruz
            return View(salesData);
        }
    }
}

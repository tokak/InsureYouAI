using InsureYouAI.Context;
using InsureYouAI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ML;
using Microsoft.ML.Transforms.TimeSeries;

namespace InsureYouAINew.ViewComponents.DashboardViewComponents
{
    public class _DashboardForecastingPoliciesSalesComponentPartial : ViewComponent
    {
        private readonly InsureContext _context;
        public _DashboardForecastingPoliciesSalesComponentPartial(InsureContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            // 1️⃣ 2025 verisini çek
            var policies = _context.Policies
                .Where(x => x.StartDate >= new DateTime(2025, 1, 1) &&
                            x.StartDate < new DateTime(2026, 1, 1))
                .ToList();


            // 2️⃣ PolicyType bazlı aylık seri oluştur (Eksik ayları 0 doldur)
            var rawData = policies
                .GroupBy(x => x.PolicyType)
                .Select(g => new
                {
                    PolicyType = g.Key,
                    MonthlyCounts = Enumerable.Range(1, 12)   // 12 ay zorunlu
                        .Select(m => new
                        {
                            Month = m,
                            Count = g.Count(x => x.StartDate.Month == m)
                        })
                        .ToList()
                })
                .ToList();


            // 3️⃣ ML Setup
            var ml = new MLContext();
            List<PolicyForecastViewModel> result = new();


            foreach (var item in rawData)
            {
                // ML için sıralı index oluştur (0-11)
                var mlData = item.MonthlyCounts
                    .OrderBy(m => m.Month)
                    .Select((m, i) => new PolicyMonthlyData
                    {
                        MonthIndex = i,
                        Value = m.Count
                    });

                var dataView = ml.Data.LoadFromEnumerable(mlData);


                // 4️⃣ SSA Tahmin Pipeline
                var pipeline = ml.Forecasting.ForecastBySsa(
                    outputColumnName: "Forecast",
                    inputColumnName: "Value",
                    windowSize: 3,
                    seriesLength: 12,
                    trainSize: 12,
                    horizon: 1);

                var model = pipeline.Fit(dataView);

                var forecastEngine =
                    model.CreateTimeSeriesEngine<PolicyMonthlyData, PolicyForecastOutput>(ml);

                var prediction = forecastEngine.Predict();

                int predicted = (int)Math.Max(0, prediction.Forecast[0]); // negatifleri engelle


                result.Add(new PolicyForecastViewModel
                {
                    PolicyType = item.PolicyType,
                    ForecastCount = predicted
                });
            }


            // 5️⃣ Yüzde Hesabı
            int total = result.Sum(x => x.ForecastCount);

            foreach (var item in result)
                item.Percentage = total > 0 ? (item.ForecastCount * 100 / total) : 0;


            return View(result);
        }

    }

    public class PolicyMonthlyData
    {
        public float MonthIndex { get; set; }
        public float Value { get; set; }
    }

    public class PolicyForecastOutput
    {
        public float[] Forecast { get; set; }
    }
}

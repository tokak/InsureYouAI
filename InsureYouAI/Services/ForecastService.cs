using Microsoft.ML;
using Microsoft.ML.Transforms.TimeSeries;

namespace InsureYouAI.Services
{
    // Zaman serisi için kullanılacak veri modeli
    // Her kayıt: bir tarih ve o tarihteki satış adedi
    public class PolicySalesData
    {
        public DateTime Date { get; set; }   // Satışın gerçekleştiği tarih
        public float SaleCount { get; set; } // O tarihteki satış sayısı
    }

    // Tahmin sonucunu tutacak model
    public class PolicySalesForecast
    {
        public float[] ForecastedValues { get; set; }   // Gelecek dönem tahmini satışlar
        public float[] LowerBoundValues { get; set; }   // Tahmin alt güven aralığı
        public float[] UpperBoundValues { get; set; }   // Tahmin üst güven aralığı
    }

    // Tahmin işlemlerini yapan servis sınıfı
    public class ForecastService
    {
        //ML.NET içindeki tüm işlemleri yöneten ana sınıftır.
        private readonly MLContext _mlContext;

        // ML.NET çalışma ortamını başlatır
        public ForecastService()
        {
            _mlContext = new MLContext();
        }

        // Ana tahmin metodu
        // salesData = geçmiş satış verisi
        // horizon = kaç dönem ileri tahmin yapılacak (default 3)
        public PolicySalesForecast GetForecast(List<PolicySalesData> salesData, int horizon = 3)
        {
            // Toplam veri sayısı
            int count = salesData.Count;

            // List veriyi ML.NET'in anlayacağı IDataView formatına çevirir
            var dataView = _mlContext.Data.LoadFromEnumerable(salesData);

            // SSA (Singular Spectrum Analysis) tahmin pipeline'ı oluşturulur
            var forecastingPipeline = _mlContext.Forecasting.ForecastBySsa(

                outputColumnName: "ForecastedValues",        // Tahmin sonuçlarının yazılacağı kolon
                inputColumnName: "SaleCount",                // Tahmin yapılacak veri kolonu

                // Modelin kısa dönem trendi öğrenmesi için pencere boyutu
                windowSize: Math.Max(2, count / 4),          // Verinin 1/4'ü kadar

                // Modelin genel trendi öğrenmesi için seri uzunluğu
                seriesLength: Math.Max(4, count / 2),        // Verinin yarısı

                // Eğitim için kullanılacak veri miktarı
                trainSize: count - horizon,                  // Son horizon kadar veri test için ayrılır

                // Kaç dönem ileri tahmin yapılacak
                horizon: horizon,

                // %95 güven aralığı ile tahmin yapılır
                confidenceLevel: 0.95f,

                // Alt tahmin sınırı (minimum beklenen satış)
                confidenceLowerBoundColumn: "LowerBoundValues",

                // Üst tahmin sınırı (maksimum beklenen satış)
                confidenceUpperBoundColumn: "UpperBoundValues"
            );

            // Model eğitilir
            var model = forecastingPipeline.Fit(dataView);

            // Zaman serisi tahmin motoru oluşturulur
            var forecastingEngine = model.CreateTimeSeriesEngine<PolicySalesData, PolicySalesForecast>(_mlContext);

            // Gelecek dönem tahminleri yapılır ve döndürülür
            return forecastingEngine.Predict();
        }
    }

}

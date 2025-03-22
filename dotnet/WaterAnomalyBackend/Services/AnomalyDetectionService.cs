using WaterAnomalyBackend.Models;
using WaterAnomalyDetection.Integration;

namespace WaterAnomalyBackend.Services;

public class AnomalyDetectionService
{
    private readonly WaterAnomalyApiClient _apiClient;
    private readonly IMeterReadingRepository _readingRepo;
    private readonly IPredictionRepository _predictionRepo;

    public AnomalyDetectionService(
        WaterAnomalyApiClient apiClient,
        IMeterReadingRepository readingRepo,
        IPredictionRepository predictionRepo)
    {
        _apiClient = apiClient;
        _readingRepo = readingRepo;
        _predictionRepo = predictionRepo;
    }

    public async Task<AnomalyPrediction> ScoreReadingAsync(ScoreRequest request, CancellationToken ct = default)
    {
        var recentReadings = (await _readingRepo.GetRecentAsync(request.MeterId)).ToList();

        double prevConsumption = recentReadings.Count > 0
            ? (double)recentReadings[0].ConsumptionGallons
            : (double)request.ConsumptionGallons;

        double rolling24hAvg = recentReadings
            .Where(r => r.ReadingTimestamp >= DateTime.UtcNow.AddHours(-24))
            .Select(r => (double)r.ConsumptionGallons)
            .DefaultIfEmpty((double)request.ConsumptionGallons)
            .Average();

        double rolling24hMax = recentReadings
            .Where(r => r.ReadingTimestamp >= DateTime.UtcNow.AddHours(-24))
            .Select(r => (double)r.ConsumptionGallons)
            .DefaultIfEmpty((double)request.ConsumptionGallons)
            .Max();

        double rolling7dAvg = recentReadings
            .Select(r => (double)r.ConsumptionGallons)
            .DefaultIfEmpty((double)request.ConsumptionGallons)
            .Average();

        double consumption = (double)request.ConsumptionGallons;
        double deltaFromPrev = consumption - prevConsumption;
        double deviationFrom24hAvg = consumption - rolling24hAvg;
        var ts = request.ReadingTimestamp;

        var apiRequest = new PredictionRequest
        {
            MeterId = request.MeterId,
            AccountId = request.AccountId,
            ReadingTimestamp = ts.ToString("o"),
            ConsumptionGallons = consumption,
            PrevConsumptionGallons = prevConsumption,
            Rolling24hAvg = rolling24hAvg,
            Rolling24hMax = rolling24hMax,
            Rolling7dAvg = rolling7dAvg,
            DeltaFromPrev = deltaFromPrev,
            OvernightFlag = ts.Hour is >= 22 or <= 5 ? 1 : 0,
            WeekendFlag = ts.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday ? 1 : 0,
            HourOfDay = ts.Hour,
            DayOfWeek = (int)ts.DayOfWeek,
            DeviationFrom24hAvg = deviationFrom24hAvg
        };

        var apiResponse = await _apiClient.PredictAsync(apiRequest, ct)
            ?? throw new InvalidOperationException("No response from anomaly API.");

        var prediction = new AnomalyPrediction
        {
            MeterId = request.MeterId,
            AccountId = request.AccountId,
            ReadingTimestamp = ts,
            AnomalyScore = apiResponse.AnomalyScore,
            IsAnomaly = apiResponse.IsAnomaly == 1,
            ConsumptionGallons = request.ConsumptionGallons,
            Rolling24hAvg = (decimal)rolling24hAvg,
            Rolling7dAvg = (decimal)rolling7dAvg
        };

        await _readingRepo.InsertAsync(new MeterReading
        {
            UtilityId = request.UtilityId,
            AccountId = request.AccountId,
            MeterId = request.MeterId,
            ReadingTimestamp = ts,
            ConsumptionGallons = request.ConsumptionGallons
        });

        await _predictionRepo.InsertAsync(prediction);

        return prediction;
    }
}

public class ScoreRequest
{
    public int UtilityId { get; set; }
    public string MeterId { get; set; } = string.Empty;
    public string AccountId { get; set; } = string.Empty;
    public DateTime ReadingTimestamp { get; set; }
    public decimal ConsumptionGallons { get; set; }
}

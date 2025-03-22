using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace WaterAnomalyDetection.Integration;

public sealed class WaterAnomalyApiClient
{
    private readonly HttpClient _httpClient;

    public WaterAnomalyApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PredictionResponse?> PredictAsync(PredictionRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("predict", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PredictionResponse>(cancellationToken: cancellationToken);
    }
}

public sealed class PredictionRequest
{
    [JsonPropertyName("meter_id")]
    public string MeterId { get; set; } = string.Empty;

    [JsonPropertyName("account_id")]
    public string AccountId { get; set; } = string.Empty;

    [JsonPropertyName("reading_timestamp")]
    public string ReadingTimestamp { get; set; } = string.Empty;

    [JsonPropertyName("consumption_gallons")]
    public double ConsumptionGallons { get; set; }

    [JsonPropertyName("prev_consumption_gallons")]
    public double PrevConsumptionGallons { get; set; }

    [JsonPropertyName("rolling_24h_avg")]
    public double Rolling24hAvg { get; set; }

    [JsonPropertyName("rolling_24h_max")]
    public double Rolling24hMax { get; set; }

    [JsonPropertyName("rolling_7d_avg")]
    public double Rolling7dAvg { get; set; }

    [JsonPropertyName("delta_from_prev")]
    public double DeltaFromPrev { get; set; }

    [JsonPropertyName("overnight_flag")]
    public int OvernightFlag { get; set; }

    [JsonPropertyName("weekend_flag")]
    public int WeekendFlag { get; set; }

    [JsonPropertyName("hour_of_day")]
    public int HourOfDay { get; set; }

    [JsonPropertyName("day_of_week")]
    public int DayOfWeek { get; set; }

    [JsonPropertyName("deviation_from_24h_avg")]
    public double DeviationFrom24hAvg { get; set; }
}

public sealed class PredictionResponse
{
    [JsonPropertyName("meter_id")]
    public string MeterId { get; set; } = string.Empty;

    [JsonPropertyName("account_id")]
    public string AccountId { get; set; } = string.Empty;

    [JsonPropertyName("reading_timestamp")]
    public string ReadingTimestamp { get; set; } = string.Empty;

    [JsonPropertyName("anomaly_score")]
    public double AnomalyScore { get; set; }

    [JsonPropertyName("is_anomaly")]
    public int IsAnomaly { get; set; }

    [JsonPropertyName("model_name")]
    public string ModelName { get; set; } = string.Empty;
}

namespace WaterAnomalyBackend.Models;

public class AnomalyPrediction
{
    public long PredictionId { get; set; }
    public string MeterId { get; set; } = string.Empty;
    public string AccountId { get; set; } = string.Empty;
    public DateTime ReadingTimestamp { get; set; }
    public double AnomalyScore { get; set; }
    public bool IsAnomaly { get; set; }
    public decimal ConsumptionGallons { get; set; }
    public decimal Rolling24hAvg { get; set; }
    public decimal Rolling7dAvg { get; set; }
    public DateTime CreatedAt { get; set; }
}

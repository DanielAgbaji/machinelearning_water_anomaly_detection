namespace WaterAnomalyBackend.Models;

public class MeterReading
{
    public long ReadingId { get; set; }
    public int UtilityId { get; set; }
    public string AccountId { get; set; } = string.Empty;
    public string MeterId { get; set; } = string.Empty;
    public DateTime ReadingTimestamp { get; set; }
    public decimal ConsumptionGallons { get; set; }
    public DateTime CreatedAt { get; set; }
}

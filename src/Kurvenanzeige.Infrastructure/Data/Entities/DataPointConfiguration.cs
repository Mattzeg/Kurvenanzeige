namespace Kurvenanzeige.Infrastructure.Data.Entities;

public class DataPointConfiguration
{
    public int Id { get; set; }
    public string TagName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public int DbNumber { get; set; }
    public int Offset { get; set; }
    public int? Bit { get; set; }
    public string? Unit { get; set; }
    public float? MinValue { get; set; }
    public float? MaxValue { get; set; }
    public bool IsEnabled { get; set; } = true;
    public int PollingInterval { get; set; } = 5000;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

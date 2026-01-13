namespace Kurvenanzeige.Shared.Dtos;

public class LiveDataDto
{
    public string TagName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public object? Value { get; set; }
    public string? Unit { get; set; }
    public int Quality { get; set; }
    public DateTime Timestamp { get; set; }
    public string DataType { get; set; } = string.Empty;
}

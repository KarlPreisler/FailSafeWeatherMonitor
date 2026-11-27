using System.Text.Json.Serialization;

namespace FailSafeWeatherMonitor.Services;

public class OpenMeteoResponse
{
    [JsonPropertyName("hourly")]
    public HourlyData? Hourly { get; set; }
}

public class HourlyData
{
    [JsonPropertyName("time")]
    public List<string> Time { get; set; } = new();

    [JsonPropertyName("temperature_2m")]
    public List<double?> Temperature { get; set; } = new();

    [JsonPropertyName("relative_humidity_2m")]
    public List<double?> Humidity { get; set; } = new();
}
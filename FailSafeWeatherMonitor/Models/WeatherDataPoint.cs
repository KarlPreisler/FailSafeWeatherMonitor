using System.ComponentModel.DataAnnotations;

namespace FailSafeWeatherMonitor.Models;

public class WeatherDataPoint
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string StationName { get; set; } = null!;

    public DateTime Timestamp { get; set; }

    public double Temperature { get; set; }

    public double Humidity { get; set; }

    [Required, MaxLength(20)]
    public string DataSource { get; set; } = null!;
}
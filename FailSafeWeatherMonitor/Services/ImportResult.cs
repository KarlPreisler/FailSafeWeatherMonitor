using FailSafeWeatherMonitor.Models;

namespace FailSafeWeatherMonitor.Services;

public record ImportResult(List<WeatherDataPoint> ValidPoints, List<string> RejectedLines);
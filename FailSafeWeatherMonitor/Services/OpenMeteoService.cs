using System.Globalization;
using System.Net.Http.Json;
using FailSafeWeatherMonitor.Models;

namespace FailSafeWeatherMonitor.Services;

public class OpenMeteoService
{
    private const string ApiUrl =
        "https://api.open-meteo.com/v1/forecast?latitude=57.6832&longitude=11.9511&hourly=temperature_2m,relative_humidity_2m";

    private const string StationName = "Goteborg_OpenMeteo";
    private const string DataSource = "API";

    private static readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(10) };

  
    public async Task<WeatherDataPoint?> GetCurrentWeatherAsync()
    {
        OpenMeteoResponse? response = await _http.GetFromJsonAsync<OpenMeteoResponse>(ApiUrl);
        if (response?.Hourly is null)
            return null;

        DateTime now = DateTime.UtcNow;
        DateTime currentHour = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);
        string currentHourText = currentHour.ToString("yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture);

        int index = response.Hourly.Time.IndexOf(currentHourText);
        if (index == -1)
            return null;

        double? temperature = response.Hourly.Temperature[index];
        double? humidity = response.Hourly.Humidity[index];
        if (temperature is null || humidity is null)
            return null;

        return new WeatherDataPoint
        {
            StationName = StationName,
            Timestamp = currentHour,
            Temperature = temperature.Value,
            Humidity = humidity.Value,
            DataSource = DataSource
        };
    }
}
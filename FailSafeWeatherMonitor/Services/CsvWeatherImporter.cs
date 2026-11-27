using System.Globalization;
using FailSafeWeatherMonitor.Models;

namespace FailSafeWeatherMonitor.Services;

public class CsvWeatherImporter
{
    private const string DataSource = "CSV-File";
    private const int MaxStationNameLength = 50;

    private const double MinTemperature = -60;
    private const double MaxTemperature = 60;
    private const double MinHumidity = 0;
    private const double MaxHumidity = 100;

    public async Task<ImportResult> ImportAsync(string dataFilePath, string badDataLogPath)
    {
        var validPoints = new List<WeatherDataPoint>();
        var rejectedLines = new List<string>();
        bool isHeader = true;

        await foreach (string line in File.ReadLinesAsync(dataFilePath))
        {
            if (isHeader)
            {
                isHeader = false;   
                continue;
            }

            if (string.IsNullOrWhiteSpace(line))
                continue;

            WeatherDataPoint? point = ParseLine(line);

            if (point is null)
                rejectedLines.Add(line);
            else
                validPoints.Add(point);
        }

        await File.WriteAllLinesAsync(badDataLogPath, rejectedLines);

        return new ImportResult(validPoints, rejectedLines);
    }

    private static WeatherDataPoint? ParseLine(string line)
    {
        string[] fields = line.Split('\t');
        if (fields.Length != 4)
            return null;

        string stationName = fields[0].Trim();
        if (string.IsNullOrWhiteSpace(stationName) || stationName.Length > MaxStationNameLength)
            return null;

        if (!DateTime.TryParse(fields[1].Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime timestamp))
            return null;

        if (!TryParseMeasurement(fields[2], out double temperature)
            || temperature < MinTemperature || temperature > MaxTemperature)
            return null;

        if (!TryParseMeasurement(fields[3], out double humidity)
            || humidity < MinHumidity || humidity > MaxHumidity)
            return null;

        return new WeatherDataPoint
        {
            StationName = stationName,
            Timestamp = timestamp,
            Temperature = temperature,
            Humidity = humidity,
            DataSource = DataSource
        };
    }

    private static bool TryParseMeasurement(string text, out double value)
    {
        string normalized = text.Trim().Replace(',', '.');

        return double.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
    }
}
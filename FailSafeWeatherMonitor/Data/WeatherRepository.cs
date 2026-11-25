using FailSafeWeatherMonitor.Models;
using Microsoft.EntityFrameworkCore;

namespace FailSafeWeatherMonitor.Data;

public class WeatherRepository(WeatherDbContext _db)
{
    public async Task<int> AddNewAsync(List<WeatherDataPoint> points)
    {
        var existingKeys = (await _db.WeatherDataPoints
                .Select(w => new { w.StationName, w.Timestamp })
                .ToListAsync())
            .Select(key => (key.StationName, key.Timestamp))
            .ToHashSet();

        List<WeatherDataPoint> newPoints = points
            .Where(point => existingKeys.Add((point.StationName, point.Timestamp)))
            .ToList();

        await _db.WeatherDataPoints.AddRangeAsync(newPoints);
        await _db.SaveChangesAsync();

        return newPoints.Count;
    }
}
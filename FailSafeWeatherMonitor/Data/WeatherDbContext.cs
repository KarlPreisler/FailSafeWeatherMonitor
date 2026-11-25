using FailSafeWeatherMonitor.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FailSafeWeatherMonitor.Data;

public class WeatherDbContext : DbContext
{
    public DbSet<WeatherDataPoint> WeatherDataPoints { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
            return;

        string? environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");

        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .Build();

        string connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection saknas i appsettings.json.");

        optionsBuilder.UseSqlServer(connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WeatherDataPoint>()
            .HasIndex(w => new { w.StationName, w.Timestamp })
            .IsUnique();
    }
}
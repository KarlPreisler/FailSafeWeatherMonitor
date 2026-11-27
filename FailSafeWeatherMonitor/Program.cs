using System.Text;
using FailSafeWeatherMonitor.Data;
using FailSafeWeatherMonitor.Models;
using FailSafeWeatherMonitor.Services;
using Microsoft.EntityFrameworkCore;
using Spectre.Console;

const string ImportFromFile = "Importera historisk data från fil";
const string FetchFromApi = "Hämta live-data från API";
const string Exit = "Avsluta";

const string DataFilePath = "weather_data.csv";
const string BadDataLogPath = "bad_data.log";

Console.OutputEncoding = Encoding.UTF8;

using var _db = new WeatherDbContext();

try
{
    await _db.Database.MigrateAsync();
}
catch (Exception ex)
{
    AnsiConsole.MarkupLine($"[red]Kunde inte ansluta till databasen:[/] {Markup.Escape(ex.Message)}");
    return;
}

var repository = new WeatherRepository(_db);
var importer = new CsvWeatherImporter();
var apiService = new OpenMeteoService();

AnsiConsole.Write(new FigletText("WeatherMonitor").Color(Color.Teal));

while (true)
{
    string choice = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("[bold]Vad vill du göra?[/]")
            .AddChoices(ImportFromFile, FetchFromApi, Exit));

    if (choice == Exit)
        break;

    switch (choice)
    {
        case ImportFromFile:
            await ImportFromFileAsync();
            break;
        case FetchFromApi:
            await FetchFromApiAsync();
            break;
    }

    AnsiConsole.WriteLine();
}

AnsiConsole.MarkupLine("[grey]Programmet avslutas.[/]");

// Menyfunktioner

async Task ImportFromFileAsync()
{
    try
    {
        ImportResult result = await importer.ImportAsync(DataFilePath, BadDataLogPath);
        int savedCount = await repository.AddNewAsync(result.ValidPoints);
        int duplicateCount = result.ValidPoints.Count - savedCount;

        AnsiConsole.MarkupLine(
            $"[green]{savedCount} rader importerade[/], [red]{result.RejectedLines.Count} rader förkastade på grund av fel[/].");

        if (duplicateCount > 0)
            AnsiConsole.MarkupLine($"[yellow]{duplicateCount} rader fanns redan i databasen och sparades inte.[/]");

        if (result.RejectedLines.Count > 0)
            AnsiConsole.MarkupLine($"[grey]Förkastade rader har loggats i {Markup.Escape(Path.GetFullPath(BadDataLogPath))}[/]");
    }
    catch (FileNotFoundException)
    {
        ShowError($"Hittade inte datafilen {DataFilePath}.");
    }
    catch (Exception ex)
    {
        ShowError($"Importen misslyckades: {ex.Message}");
    }
}

async Task FetchFromApiAsync()
{
    try
    {
        WeatherDataPoint? point = await apiService.GetCurrentWeatherAsync();
        if (point is null)
        {
            ShowError("Open-Meteo svarade, men svaret saknade värden för innevarande timme.");
            return;
        }

        int savedCount = await repository.AddNewAsync(new List<WeatherDataPoint> { point });

        var table = new Table()
            .Border(TableBorder.Rounded)
            .Title("[bold]Aktuellt väder[/]")
            .AddColumn("Station")
            .AddColumn("Tidpunkt (UTC)")
            .AddColumn(new TableColumn("Temperatur").RightAligned())
            .AddColumn(new TableColumn("Luftfuktighet").RightAligned());

        table.AddRow(
            Markup.Escape(point.StationName),
            point.Timestamp.ToString("yyyy-MM-dd HH:mm"),
            $"{point.Temperature:F1} °C",
            $"{point.Humidity:F0} %");

        AnsiConsole.Write(table);

        if (savedCount == 1)
            AnsiConsole.MarkupLine("[green]Mätningen sparades i databasen.[/]");
        else
            AnsiConsole.MarkupLine("[yellow]En mätning för den här timmen fanns redan och sparades inte igen.[/]");
    }
    catch (HttpRequestException ex)
    {
        ShowError($"Nätverksfel – kunde inte nå Open-Meteo: {ex.Message}");
    }
    catch (Exception ex)
    {
        ShowError($"Hämtningen misslyckades: {ex.Message}");
    }
}

void ShowError(string message)
{
    AnsiConsole.MarkupLine($"[red]Fel:[/] {Markup.Escape(message)}");
}
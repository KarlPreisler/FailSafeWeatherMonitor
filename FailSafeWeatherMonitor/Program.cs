using FailSafeWeatherMonitor.Data;
using FailSafeWeatherMonitor.Services;
using Microsoft.EntityFrameworkCore;
using Spectre.Console;
using System.Text;

const string ImportFromFile = "Importera historisk data från fil";
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

AnsiConsole.Write(new FigletText("WeatherMonitor").Color(Color.Teal));

while (true)
{
    string choice = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("[bold]Vad vill du göra?[/]")
            .AddChoices(ImportFromFile, Exit));

    if (choice == Exit)
        break;

    switch (choice)
    {
        case ImportFromFile:
            await ImportFromFileAsync();
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
            AnsiConsole.MarkupLine($"[yellow]{duplicateCount} rader fanns redan i databasen och importerades inte.[/]");

        if (result.RejectedLines.Count > 0)
            AnsiConsole.MarkupLine($"[grey]Förkastade rader har loggats i {Markup.Escape(Path.GetFullPath(BadDataLogPath))}[/]");

    }
    catch (FileNotFoundException)
    {
        ShowError($"Hittade inte Datafilen {DataFilePath}.");
    }
    catch (Exception ex)
    {
        ShowError($"Ett fel uppstod under importen: {ex.Message}");
    }
}

    void ShowError(string message)
    {
        AnsiConsole.MarkupLine($"[red]Fel:{Markup.Escape(message)}[/]");
    }

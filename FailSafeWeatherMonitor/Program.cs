using System.Text;
using FailSafeWeatherMonitor.Data;
using Microsoft.EntityFrameworkCore;
using Spectre.Console;

const string Exit = "Avsluta";

Console.OutputEncoding = Encoding.UTF8;   // så att å, ä, ö visas korrekt i konsolen

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

AnsiConsole.Write(new FigletText("WeatherMonitor").Color(Color.Teal));

while (true)
{
    string choice = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("[bold]Vad vill du göra?[/]")
            .AddChoices(Exit));

    if (choice == Exit)
        break;
}

AnsiConsole.MarkupLine("[grey]Programmet avslutas.[/]");
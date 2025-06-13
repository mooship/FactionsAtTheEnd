using FactionsAtTheEnd.Core;
using FactionsAtTheEnd.UI;
using Spectre.Console;

namespace FactionsAtTheEnd;

class Program
{
    static async Task Main(string[] args)
    {
        AnsiConsole.MarkupLine("[bold red]🔮 FACTIONS AT THE END 🔮[/]");
        AnsiConsole.MarkupLine("[dim]Grimdark diplomacy in a collapsing empire[/]");
        AnsiConsole.WriteLine();

        var gameEngine = new GameEngine();
        var gameUI = new GameUI(gameEngine);

        await gameUI.RunMainMenuAsync();
    }
}

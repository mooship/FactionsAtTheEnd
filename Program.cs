using FactionsAtTheEnd.Core;
using FactionsAtTheEnd.Interfaces;
using FactionsAtTheEnd.Models;
using FactionsAtTheEnd.Services;
using FactionsAtTheEnd.UI;
using FactionsAtTheEnd.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace FactionsAtTheEnd;

class Program
{
    static async Task Main(string[] args)
    {
        // Setup DI
        var services = new ServiceCollection();
        ConfigureServices(services);
        var serviceProvider = services.BuildServiceProvider();

        var gameEngine = serviceProvider.GetRequiredService<GameEngine>();
        var factionService = serviceProvider.GetRequiredService<IFactionService>();
        var factionValidator = serviceProvider.GetRequiredService<IValidator<Faction>>();
        var playerActionValidator = serviceProvider.GetRequiredService<IValidator<PlayerAction>>();
        var gameUI = new GameUI(
            gameEngine,
            factionService,
            factionValidator,
            playerActionValidator
        );

        AnsiConsole.MarkupLine("[bold red]🔮 FACTIONS AT THE END 🔮[/]");
        AnsiConsole.MarkupLine("[dim]Grimdark diplomacy in a collapsing empire[/]");
        AnsiConsole.WriteLine();

        await gameUI.RunMainMenuAsync();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Register services and engine
        services.AddSingleton<IEventService, EventService>();
        services.AddSingleton<IFactionService, FactionService>();
        services.AddSingleton<IGameDataService, GameDataService>();
        services.AddSingleton<GameEngine>();
        // Register validators
        services.AddTransient<IValidator<Faction>, FactionValidator>();
        services.AddTransient<IValidator<PlayerAction>, PlayerActionValidator>();
    }
}

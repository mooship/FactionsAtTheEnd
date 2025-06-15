using FactionsAtTheEnd.Core;
using FactionsAtTheEnd.Interfaces;
using FactionsAtTheEnd.Models;
using FactionsAtTheEnd.Services;
using FactionsAtTheEnd.UI;
using FactionsAtTheEnd.Validators;
using FluentValidation;
using LiteDB;
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

        var gameUI = serviceProvider.GetRequiredService<GameUI>();

        AnsiConsole.MarkupLine("[bold red]🔮 FACTIONS AT THE END 🔮[/]");
        AnsiConsole.MarkupLine("[dim]Grimdark diplomacy in a collapsing empire[/]");
        AnsiConsole.WriteLine();

        await gameUI.RunMainMenuAsync();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Register services and engine
        services.AddSingleton<ILiteDatabase>(sp => new LiteDatabase("factionsattheend.db"));
        services.AddSingleton<IEventService, EventService>();
        services.AddSingleton<IFactionService, FactionService>();
        services.AddSingleton<IGameDataService, GameDataService>();
        services.AddSingleton<GameEngine>();
        // Register validators
        services.AddTransient<IValidator<Faction>, FactionValidator>();
        services.AddTransient<IValidator<PlayerAction>, PlayerActionValidator>();
        // Register UI
        services.AddTransient<GameUI>();
    }
}

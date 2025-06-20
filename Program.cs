using FactionsAtTheEnd.Core;
using FactionsAtTheEnd.Interfaces;
using FactionsAtTheEnd.Models;
using FactionsAtTheEnd.Services;
using FactionsAtTheEnd.UI;
using FactionsAtTheEnd.Validators;
using FluentValidation;
using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Spectre.Console;

namespace FactionsAtTheEnd;

class Program
{
    static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.File(".log", rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true)
            .MinimumLevel.Debug()
            .CreateLogger();

        try
        {
            Log.Information("Starting Factions At The End");
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
        catch (Exception ex)
        {
            Log.Error(ex, "Unhandled exception");
            throw;
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IAppLogger>(sp => new AppLogger(Log.Logger));
        services.AddSingleton<ILiteDatabase>(sp => new LiteDatabase("factionsattheend.db"));
        services.AddSingleton<IEventService, EventService>();
        services.AddSingleton<IFactionService, FactionService>();
        services.AddSingleton<IGameDataService, GameDataService>();
        services.AddSingleton<IGlobalAchievementService, GlobalAchievementService>();
        services.AddTransient(sp => new GameUI(
            sp.GetRequiredService<GameEngine>(),
            sp.GetRequiredService<IValidator<Faction>>(),
            sp.GetRequiredService<IValidator<PlayerAction>>(),
            sp.GetRequiredService<IGlobalAchievementService>()
        ));
        services.AddSingleton<GameEngine>(sp => new GameEngine(
            sp.GetRequiredService<IEventService>(),
            sp.GetRequiredService<IFactionService>(),
            sp.GetRequiredService<IGameDataService>(),
            sp.GetRequiredService<IValidator<PlayerAction>>(),
            sp.GetRequiredService<IValidator<GameEvent>>(),
            sp.GetRequiredService<IValidator<EventChoice>>(),
            sp.GetRequiredService<IGlobalAchievementService>(),
            sp.GetRequiredService<IAppLogger>()
        ));
        services.AddSingleton<IValidator<Faction>, FactionValidator>();
        services.AddSingleton<IValidator<PlayerAction>, PlayerActionValidator>();
        services.AddSingleton<IValidator<GlobalAchievement>, GlobalAchievementValidator>();
        services.AddSingleton<IValidator<GameEvent>, GameEventValidator>();
        services.AddSingleton<IValidator<EventChoice>, EventChoiceValidator>();
    }
}

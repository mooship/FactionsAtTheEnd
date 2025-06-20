using CommunityToolkit.Diagnostics;
using FactionsAtTheEnd.Constants;
using FactionsAtTheEnd.Core;
using FactionsAtTheEnd.Enums;
using FactionsAtTheEnd.Extensions;
using FactionsAtTheEnd.Interfaces;
using FactionsAtTheEnd.Models;
using FluentValidation;
using Serilog;
using Spectre.Console;
using TextCopy;

namespace FactionsAtTheEnd.UI;

/// <summary>
/// Handles all user interface and game loop logic for Factions at the End, including menus, turn flow, and achievement display.
/// </summary>
public class GameUI
{
    private readonly GameEngine _gameEngine;
    private readonly IValidator<Faction> _factionValidator;
    private readonly IValidator<PlayerAction> _playerActionValidator;
    private readonly IGlobalAchievementService _globalAchievementService;
    private static readonly ILogger _logger = Log.Logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GameUI"/> class.
    /// </summary>
    /// <param name="gameEngine">The main game engine instance.</param>
    /// <param name="factionValidator">Validator for faction creation.</param>
    /// <param name="playerActionValidator">Validator for player actions.</param>
    /// <param name="globalAchievementService">Service for global achievements.</param>
    public GameUI(
        GameEngine gameEngine,
        IValidator<Faction> factionValidator,
        IValidator<PlayerAction> playerActionValidator,
        IGlobalAchievementService globalAchievementService
    )
    {
        Guard.IsNotNull(gameEngine, nameof(gameEngine));
        Guard.IsNotNull(factionValidator, nameof(factionValidator));
        Guard.IsNotNull(playerActionValidator, nameof(playerActionValidator));
        Guard.IsNotNull(globalAchievementService, nameof(globalAchievementService));
        _gameEngine = gameEngine;
        _factionValidator = factionValidator;
        _playerActionValidator = playerActionValidator;
        _globalAchievementService = globalAchievementService;
        _logger.Debug("GameUI initialized.");
    }

    /// <summary>
    /// Runs the main menu loop, allowing the player to start/load games, view achievements, or exit.
    /// </summary>
    public async Task RunMainMenuAsync()
    {
        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine("[bold red]🔮 FACTIONS AT THE END 🔮[/]");
            AnsiConsole.WriteLine();
            _logger.Debug("Main menu displayed.");

            var menuOptions = new[]
            {
                MenuOption.NewGame,
                MenuOption.LoadGame,
                MenuOption.Help,
                MenuOption.Exit,
                MenuOption.ExportSave,
                MenuOption.ImportSave,
                MenuOption.ShowGlobalAchievements,
            };
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<MenuOption>()
                    .Title("What would you like to do?")
                    .AddChoices(menuOptions)
                    .UseConverter(opt => opt.GetDisplayName())
            );

            try
            {
                switch (choice)
                {
                    case MenuOption.NewGame:
                        await StartNewGameAsync();
                        break;
                    case MenuOption.LoadGame:
                        await LoadGameAsync();
                        break;
                    case MenuOption.Help:
                        ShowHelp();
                        break;
                    case MenuOption.Exit:
                        _logger.Information("User exited the game from main menu.");
                        return;
                    case MenuOption.ExportSave:
                        ExportSave();
                        break;
                    case MenuOption.ImportSave:
                        await ImportSaveAsync();
                        break;
                    case MenuOption.ShowGlobalAchievements:
                        ShowGlobalAchievements();
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in main menu selection: {Choice}", choice);
                AnsiConsole.MarkupLine($"[red]An error occurred: {ex.Message}[/]");
            }
        }
    }

    /// <summary>
    /// Displays help and gameplay tips to the player.
    /// </summary>
    private static void ShowHelp()
    {
        AnsiConsole.MarkupLine("[bold yellow]Help & Tips[/]");
        AnsiConsole.MarkupLine(
            "- [green]Single Faction[/]: You control the only surviving faction. There are no AI, diplomacy, or multifaction mechanics."
        );
        AnsiConsole.MarkupLine(
            "- [green]Actions[/]: Each turn, choose actions like [blue]Diplomacy[/] or [blue]Espionage[/] to shape your faction's fate."
        );
        AnsiConsole.MarkupLine(
            "- [green]Unique Abilities[/]: Each faction type has a unique trait. Try different types for new strategies!"
        );
        AnsiConsole.MarkupLine(
            "- [green]Events[/]: Some events let you choose a response. Your choices can change your stats or unlock new storylines."
        );
        AnsiConsole.MarkupLine(
            "- [green]Event Effects[/]: After each event, you'll see exactly how it affected your stats, resources, or available actions."
        );
        AnsiConsole.MarkupLine(
            "- [green]Event Log[/]: Review all past events and narrative history at any time from the in-game menu."
        );
        AnsiConsole.MarkupLine(
            "- [green]Faction Overview[/]: View your stats, traits, and description from the in-game menu."
        );
        AnsiConsole.MarkupLine(
            "- [green]Win/Lose[/]: Survive 20 cycles or reach 100 Technology to win. Lose if Population, Resources, or Stability hit zero."
        );
        AnsiConsole.MarkupLine(
            "- [green]Achievements[/]: Special milestones will be announced as you play. View your global achievements from the main menu."
        );
        AnsiConsole.MarkupLine("- [green]Tooltips[/]: Hover or select actions for more info.");
        AnsiConsole.MarkupLine("\nPress any key to return...");
        Console.ReadKey();
    }

    /// <summary>
    /// Gets a description for a given player action type.
    /// </summary>
    /// <param name="action">The action type.</param>
    /// <returns>Description string.</returns>
    private static string GetActionDescription(PlayerActionType action)
    {
        return action switch
        {
            PlayerActionType.BuildDefenses => ActionDescriptions.BuildDefenses,
            PlayerActionType.RecruitTroops => ActionDescriptions.RecruitTroops,
            PlayerActionType.DevelopInfrastructure => ActionDescriptions.DevelopInfrastructure,
            PlayerActionType.ExploitResources => ActionDescriptions.ExploitResources,
            PlayerActionType.MilitaryTech => ActionDescriptions.MilitaryTech,
            PlayerActionType.EconomicTech => ActionDescriptions.EconomicTech,
            PlayerActionType.AncientStudies => ActionDescriptions.AncientStudies,
            PlayerActionType.GateNetworkResearch => ActionDescriptions.GateNetworkResearch,
            PlayerActionType.Diplomacy => ActionDescriptions.Diplomacy,
            PlayerActionType.Espionage => ActionDescriptions.Espionage,
            _ => ActionDescriptions.Default,
        };
    }

    /// <summary>
    /// Starts a new game, prompting the user for faction details and launching the game loop.
    /// </summary>
    private async Task StartNewGameAsync()
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine("[bold cyan]Creating New Game[/]");
        AnsiConsole.WriteLine();

        var factionName = AnsiConsole.Ask<string>("What is your faction's [bold green]name[/]?");

        // Prepare choices with both name and description
        var factionChoices = Enum.GetValues<FactionType>()
            .Select(type => new
            {
                Type = type,
                Display = $"[yellow]{type.GetDisplayName()}[/] - [grey]{GetFactionTypeDescription(type)}[/]",
            })
            .ToList();

        var selectedDisplay = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Choose your faction type:")
                .AddChoices(factionChoices.Select(fc => fc.Display))
                .HighlightStyle("bold yellow")
                .PageSize(8)
                .MoreChoicesText("[grey](Move up and down to reveal more factions)[/]")
        );
        var selectedFaction = factionChoices.First(fc => fc.Display == selectedDisplay).Type;

        // Validate faction input
        var tempFaction = new Faction
        {
            Name = factionName,
            Type = selectedFaction,
            IsPlayer = true,
        };
        var validationResult = _factionValidator.Validate(tempFaction);
        if (!validationResult.IsValid)
        {
            _logger.Warning(
                "Invalid faction details provided: {Errors}",
                string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))
            );
            AnsiConsole.MarkupLine("[red]Invalid faction details. Please try again.[/]");
            return;
        }

        AnsiConsole.MarkupLine(
            $"[yellow]Creating faction '{factionName}' of type {selectedFaction.GetDisplayName()}...[/]"
        );

        var gameState = await _gameEngine.CreateNewGameAsync(factionName, selectedFaction);

        AnsiConsole.MarkupLine("[green]Game created successfully![/]");
        AnsiConsole.MarkupLine("Press any key to begin...");
        Console.ReadKey();

        await RunGameLoopAsync();
    }

    /// <summary>
    /// Loads a saved game and launches the game loop.
    /// </summary>
    private async Task LoadGameAsync()
    {
        _logger.Debug("Loading game from UI.");
        var savedGames = await _gameEngine.GetSavedGamesAsync();

        if (savedGames.Count == 0)
        {
            _logger.Warning("No saved games found when attempting to load.");
            AnsiConsole.MarkupLine("[yellow]No saved games found.[/]");
            return;
        }

        var selectedGame = AnsiConsole.Prompt(
            new SelectionPrompt<GameState>()
                .Title("Select a game to load:")
                .AddChoices(savedGames)
                .UseConverter(game =>
                    $"{game.SaveName} - Cycle {game.CurrentCycle} ({game.LastPlayed:yyyy-MM-dd HH:mm})"
                )
        );

        await _gameEngine.LoadGameAsync(selectedGame.Id);
        AnsiConsole.MarkupLine("[green]Game loaded successfully![/]");
        AnsiConsole.MarkupLine("Press any key to continue...");
        Console.ReadKey();

        await RunGameLoopAsync();
    }

    /// <summary>
    /// Runs the main game loop, handling turns, actions, and win/loss conditions.
    /// </summary>
    private async Task RunGameLoopAsync()
    {
        _logger.Debug("Game loop started.");
        while (true)
        {
            var game = _gameEngine.CurrentGame;
            Guard.IsNotNull(game);
            var playerFaction = game.PlayerFaction;
            Guard.IsNotNull(playerFaction);

            // Show blocked actions from events
            if (game.BlockedActions.Count > 0)
            {
                AnsiConsole.MarkupLine(
                    "[yellow]The following actions are blocked this turn due to recent events:[/]"
                );
                foreach (var blocked in game.BlockedActions)
                {
                    AnsiConsole.MarkupLine($"[red]- {blocked.GetDisplayName()}[/]");
                }
            }
            // Show tooltips for blocked actions
            if (game.BlockedActions.Count > 0)
            {
                foreach (var blocked in game.BlockedActions)
                {
                    ShowActionTooltip(blocked);
                }
            }
            AnsiConsole.MarkupLine("[grey]Press [b]H[/] at any time for help.[/]");

            // Game over condition
            if (game.HasLost)
            {
                _logger.Information("Player has lost the game.");
                AnsiConsole.MarkupLine("[red]You have lost the game![/]");
                break;
            }
            // Win condition
            if (game.HasWon || playerFaction.Technology >= 100 || game.CurrentCycle > 20)
            {
                _logger.Information("Player has won the game or met win conditions.");
                AnsiConsole.MarkupLine("[green]Congratulations! You have won the game![/]");
                break;
            }

            AnsiConsole.Clear();
            // Display current turn number
            AnsiConsole.MarkupLine($"[bold blue]Turn:[/] {game.CurrentCycle}");
            AnsiConsole.MarkupLine("");
            AnsiConsole.MarkupLine(
                $"[bold]Faction:[/] {playerFaction?.Name} ({playerFaction?.Type.GetDisplayName()})"
            );
            AnsiConsole.MarkupLine($"[bold]Population:[/] {playerFaction?.Population}");
            AnsiConsole.MarkupLine($"[bold]Military:[/] {playerFaction?.Military}");
            AnsiConsole.MarkupLine($"[bold]Technology:[/] {playerFaction?.Technology}");
            AnsiConsole.MarkupLine($"[bold]Influence:[/] {playerFaction?.Influence}");
            AnsiConsole.MarkupLine($"[bold]Resources:[/] {playerFaction?.Resources}");
            AnsiConsole.MarkupLine($"[bold]Stability:[/] {playerFaction?.Stability}");
            AnsiConsole.MarkupLine(
                $"[bold]Reputation:[/] {playerFaction?.Reputation} {GetReputationDescription(playerFaction?.Reputation ?? 0)}"
            );
            AnsiConsole.MarkupLine("");
            if (game.GalacticNews.Count > 0)
            {
                AnsiConsole.MarkupLine("[bold underline]Galactic News:[/]");
                foreach (var news in game.GalacticNews.TakeLast(5))
                {
                    AnsiConsole.MarkupLine($"[aqua]{news}[/]");
                }
                AnsiConsole.MarkupLine("");
            }
            if (game.GalacticHistory.Count > 0)
            {
                AnsiConsole.MarkupLine("[bold underline]Galactic History:[/]");
                AnsiConsole.MarkupLine($"[grey]{game.GalacticHistory.Last()}[/]");
                AnsiConsole.MarkupLine("");
            }
            if (game.BlockedActions.Count > 0)
            {
                AnsiConsole.MarkupLine(
                    "[yellow]Some actions are unavailable due to recent overuse. Your rivals are watching your every move...[/]"
                );
            }

            var playerActions = new List<PlayerAction>();

            var mainMenuOptions = new[]
            {
                MenuOption.TakeAction,
                MenuOption.ViewFactionOverview,
                MenuOption.ViewEventLog,
                MenuOption.Help,
                MenuOption.ExitToMainMenu,
            };
            var mainChoice = AnsiConsole.Prompt(
                new SelectionPrompt<MenuOption>()
                    .Title("[bold]What will you do?[/]")
                    .AddChoices(mainMenuOptions)
                    .UseConverter(opt => opt.GetDisplayName())
            );
            if (mainChoice == MenuOption.Help)
            {
                ShowHelp();
                continue;
            }
            if (mainChoice == MenuOption.ViewFactionOverview)
            {
                AnsiConsole.Clear();
                var overviewOptions = new[]
                {
                    "Table Overview (Stats Table)",
                    "Detailed Game State (Narrative)",
                    "Show Both",
                    "Back",
                };
                var overviewChoice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("How would you like to view your faction?")
                        .AddChoices(overviewOptions)
                );
                AnsiConsole.Clear();
                if (overviewChoice == "Table Overview (Stats Table)")
                {
                    DisplayFactionsOverview();
                }
                else if (overviewChoice == "Detailed Game State (Narrative)")
                {
                    DisplayGameState();
                }
                else if (overviewChoice == "Show Both")
                {
                    DisplayFactionsOverview();
                    AnsiConsole.MarkupLine("\n[grey]--- Press any key for detailed view ---[/]");
                    Console.ReadKey();
                    AnsiConsole.Clear();
                    DisplayGameState();
                }
                AnsiConsole.MarkupLine("Press any key to return...");
                Console.ReadKey();
                continue;
            }
            if (mainChoice == MenuOption.ViewEventLog)
            {
                ShowEventLog(_gameEngine.CurrentGame);
                continue;
            }
            if (mainChoice == MenuOption.ExitToMainMenu)
            {
                break;
            }

            var actionChoices = new[]
            {
                PlayerActionType.BuildDefenses,
                PlayerActionType.RecruitTroops,
                PlayerActionType.DevelopInfrastructure,
                PlayerActionType.ExploitResources,
                PlayerActionType.MilitaryTech,
                PlayerActionType.EconomicTech,
                PlayerActionType.AncientStudies,
                PlayerActionType.GateNetworkResearch,
                PlayerActionType.Diplomacy,
                PlayerActionType.Espionage,
            };

            var chosenActions = new HashSet<PlayerActionType>();
            for (int i = 0; i < 2; i++)
            {
                var availableActions = actionChoices.Except(chosenActions).ToArray();
                var actionDisplayNames = availableActions
                    .Select(a =>
                        game.BlockedActions.Contains(a)
                            ? $"[red]{a.GetDisplayName()} (Blocked)[/]"
                            : a.GetDisplayName()
                    )
                    .ToList();
                actionDisplayNames.Add("Finish Turn");
                var promptTitle = $"Choose action {i + 1} of 2 (Actions remaining: {2 - i})";
                var selectedActionDisplay = AnsiConsole.Prompt(
                    new SelectionPrompt<string>().Title(promptTitle).AddChoices(actionDisplayNames)
                );
                if (selectedActionDisplay == "Finish Turn")
                {
                    break;
                }
                var selectedIdx = actionDisplayNames.IndexOf(selectedActionDisplay);
                var selectedAction = availableActions[selectedIdx];
                if (game.BlockedActions.Contains(selectedAction))
                {
                    AnsiConsole.MarkupLine(
                        $"[red]That action is blocked this turn! Choose another.[/]"
                    );
                    i--;
                    continue;
                }
                Guard.IsNotNull(playerFaction);
                playerActions.Add(new PlayerAction { ActionType = selectedAction });
                chosenActions.Add(selectedAction);
            }

            Guard.IsNotNull(playerFaction);
            var preStats = new
            {
                playerFaction.Population,
                playerFaction.Military,
                playerFaction.Technology,
                playerFaction.Influence,
                playerFaction.Resources,
                playerFaction.Stability,
                playerFaction.Reputation,
            };

            await ProcessTurnAsync(playerActions);

            game = _gameEngine.CurrentGame;
            Guard.IsNotNull(game, nameof(game));
            playerFaction = game.PlayerFaction;
            Guard.IsNotNull(playerFaction, nameof(playerFaction));

            var recentEvents = game.RecentEvents.Where(e => e.Cycle == game.CurrentCycle).ToList();
            if (recentEvents.Count > 0)
            {
                AnsiConsole.MarkupLine("[bold yellow]Events this turn:[/]");
                foreach (var gameEvent in recentEvents)
                {
                    AnsiConsole.MarkupLine($"[bold]{gameEvent.Title}[/]");
                    AnsiConsole.MarkupLine(gameEvent.Description);
                    ShowEventEffects(gameEvent);
                    AnsiConsole.WriteLine();
                }
            }

            var choiceEvent = game.RecentEvents.FirstOrDefault(e =>
                e.Choices != null && e.Choices.Count > 0
            );
            if (choiceEvent?.Choices != null && choiceEvent.Choices.Count > 0)
            {
                EventChoice? selectedChoice;
                if (
                    choiceEvent.Choices.Any(c =>
                        c.NextStepChoices != null && c.NextStepChoices.Count > 0
                    )
                )
                {
                    var firstStepDescriptions = choiceEvent
                        .Choices.Select((c, i) => $"[{i + 1}] {c.Description}")
                        .ToList();
                    int selectedIndex = 0;
                    do
                    {
                        AnsiConsole.MarkupLine("\n[bold]Choose:[/]");
                        for (int i = 0; i < firstStepDescriptions.Count; i++)
                            AnsiConsole.MarkupLine(firstStepDescriptions[i]);
                        var input = AnsiConsole.Ask<string>("[yellow]Enter choice number:[/]");
                        if (
                            int.TryParse(input, out int idx)
                            && idx > 0
                            && idx <= firstStepDescriptions.Count
                        )
                        {
                            selectedIndex = idx - 1;
                            break;
                        }
                        AnsiConsole.MarkupLine(
                            "[red]Invalid input. Please enter a valid number.[/]"
                        );
                    } while (true);
                    var initialChoice = choiceEvent.Choices[selectedIndex];
                    selectedChoice = GameEngine.RunMultiStepChoice(initialChoice);
                }
                else
                {
                    var choiceDescriptions = choiceEvent
                        .Choices.Select(c => c.Description)
                        .ToList();
                    var choice = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title($"[yellow]{choiceEvent.Title}[/] - {choiceEvent.Description}")
                            .AddChoices(choiceDescriptions)
                    );
                    selectedChoice = choiceEvent.Choices.First(c => c.Description == choice);
                }

                Guard.IsNotNull(selectedChoice);
                foreach (var effect in selectedChoice.Effects)
                {
                    switch (effect.Key)
                    {
                        case StatKey.Population:
                            playerFaction.Population += effect.Value;
                            break;
                        case StatKey.Military:
                            playerFaction.Military += effect.Value;
                            break;
                        case StatKey.Technology:
                            playerFaction.Technology += effect.Value;
                            break;
                        case StatKey.Influence:
                            playerFaction.Influence += effect.Value;
                            break;
                        case StatKey.Resources:
                            playerFaction.Resources += effect.Value;
                            break;
                        case StatKey.Stability:
                            playerFaction.Stability += effect.Value;
                            break;
                        case StatKey.Reputation:
                            playerFaction.Reputation += effect.Value;
                            break;
                    }
                }
                if (
                    selectedChoice.BlockedActions != null
                    && selectedChoice.BlockedActions.Count > 0
                )
                {
                    foreach (var blocked in selectedChoice.BlockedActions)
                    {
                        if (!game.BlockedActions.Contains(blocked))
                        {
                            game.BlockedActions.Add(blocked);
                        }
                    }
                }
                playerFaction.ClampResources();
                AnsiConsole.MarkupLine($"[green]Choice applied: {selectedChoice.Description}[/]");
                game.RecentEvents.Remove(choiceEvent);
            }

            var postStats = new
            {
                playerFaction.Population,
                playerFaction.Military,
                playerFaction.Technology,
                playerFaction.Influence,
                playerFaction.Resources,
                playerFaction.Stability,
                playerFaction.Reputation,
            };
            AnsiConsole.MarkupLine("[bold green]Turn Results:[/]");
            ShowStatChange("Population", preStats.Population, postStats.Population);
            ShowStatChange("Military", preStats.Military, postStats.Military);
            ShowStatChange("Technology", preStats.Technology, postStats.Technology);
            ShowStatChange("Influence", preStats.Influence, postStats.Influence);
            ShowStatChange("Resources", preStats.Resources, postStats.Resources);
            ShowStatChange("Stability", preStats.Stability, postStats.Stability);
            ShowStatChange("Reputation", preStats.Reputation, postStats.Reputation);
            AnsiConsole.MarkupLine("Press any key to continue...");
            Console.ReadKey();

            var unblockedActions = new List<PlayerActionType>();
            var prevBlocked = new HashSet<PlayerActionType>(game.BlockedActions);
            var blockedLastTurn = prevBlocked;
            var blockedThisTurn = new HashSet<PlayerActionType>(game.BlockedActions);
            foreach (var action in Enum.GetValues<PlayerActionType>())
            {
                if (blockedLastTurn.Contains(action) && !blockedThisTurn.Contains(action))
                {
                    unblockedActions.Add(action);
                }
            }
            if (unblockedActions.Count > 0)
            {
                foreach (var action in unblockedActions)
                {
                    AnsiConsole.MarkupLine(
                        $"[bold green]Action Unlocked:[/] {action.GetDisplayName()} is now available!"
                    );
                }
            }

            if (playerFaction.Technology >= 50)
            {
                AnsiConsole.MarkupLine("[aqua]Achievement unlocked: Tech Ascendant![/]");
            }
            if (playerFaction.Military >= 80)
            {
                AnsiConsole.MarkupLine("[aqua]Achievement unlocked: Warlord![/]");
            }
            if (playerFaction.Influence >= 80)
            {
                AnsiConsole.MarkupLine("[aqua]Achievement unlocked: Kingmaker![/]");
            }
        }
    }

    /// <summary>
    /// Displays stat changes after a turn.
    /// </summary>
    /// <param name="stat">Stat name.</param>
    /// <param name="before">Value before the turn.</param>
    /// <param name="after">Value after the turn.</param>
    private static void ShowStatChange(string stat, int before, int after)
    {
        int diff = after - before;
        string icon =
            diff > 0 ? "[green]+[/]"
            : diff < 0 ? "[red]-[/]"
            : "[grey]=[/]";
        if (diff == 0)
        {
            AnsiConsole.MarkupLine($"{icon} {stat}: {after}");
        }
        else if (diff > 0)
        {
            AnsiConsole.MarkupLine($"{icon} {stat}: {before} [green]+{diff}[/] = {after}");
        }
        else
        {
            AnsiConsole.MarkupLine($"{icon} {stat}: {before} [red]{diff}[/] = {after}");
        }
    }

    /// <summary>
    /// Shows a detailed view of the current game state.
    /// </summary>
    private void DisplayGameState()
    {
        var game = _gameEngine.CurrentGame!;
        var playerFaction = game.PlayerFaction;
        Guard.IsNotNull(game, nameof(game));
        Guard.IsNotNull(playerFaction, nameof(playerFaction));
        AnsiConsole.MarkupLine($"[bold]Cycle:[/] {game.CurrentCycle}");
        AnsiConsole.MarkupLine("");
        AnsiConsole.MarkupLine(
            $"[bold]Faction:[/] {playerFaction?.Name} ({playerFaction?.Type.GetDisplayName()})"
        );
        AnsiConsole.MarkupLine($"[bold]Population:[/] {playerFaction?.Population}");
        AnsiConsole.MarkupLine($"[bold]Military:[/] {playerFaction?.Military}");
        AnsiConsole.MarkupLine($"[bold]Technology:[/] {playerFaction?.Technology}");
        AnsiConsole.MarkupLine($"[bold]Influence:[/] {playerFaction?.Influence}");
        AnsiConsole.MarkupLine($"[bold]Resources:[/] {playerFaction?.Resources}");
        AnsiConsole.MarkupLine($"[bold]Stability:[/] {playerFaction?.Stability}");
        AnsiConsole.MarkupLine($"[bold]Reputation:[/] {playerFaction?.Reputation}");
        AnsiConsole.MarkupLine(
            $"[bold]Reputation:[/] {playerFaction?.Reputation} {GetReputationDescription(playerFaction?.Reputation ?? 0)}"
        );
        AnsiConsole.MarkupLine("");
        if (game.GalacticNews.Count > 0)
        {
            AnsiConsole.MarkupLine("[bold underline]Galactic News:[/]");
            foreach (var news in game.GalacticNews.TakeLast(5))
            {
                AnsiConsole.MarkupLine($"[aqua]{news}[/]");
            }
            AnsiConsole.MarkupLine("");
        }
        if (game.GalacticHistory.Count > 0)
        {
            AnsiConsole.MarkupLine("[bold underline]Galactic History:[/]");
            AnsiConsole.MarkupLine($"[grey]{game.GalacticHistory.Last()}[/]");
            AnsiConsole.MarkupLine("");
        }
        if (game.BlockedActions.Count > 0)
        {
            AnsiConsole.MarkupLine(
                "[yellow]Some actions are unavailable due to recent overuse. Your rivals are watching your every move...[/]"
            );
        }
    }

    /// <summary>
    /// Shows a table overview of the player's faction.
    /// </summary>
    private void DisplayFactionsOverview()
    {
        var game = _gameEngine.CurrentGame!;
        var playerFaction = game.PlayerFaction;
        var table = new Table().Border(TableBorder.Rounded).Title("[bold cyan]Faction Overview[/]");
        table.AddColumn("[bold]Property[/]");
        table.AddColumn("[bold]Value[/]");
        table.AddRow("Name", playerFaction.Name);
        table.AddRow("Type", playerFaction.Type.GetDisplayName());
        table.AddRow("Description", playerFaction.Description);
        table.AddRow("Traits", string.Join(", ", playerFaction.Traits));
        table.AddRow("Population", playerFaction.Population.ToString());
        table.AddRow("Military", playerFaction.Military.ToString());
        table.AddRow("Technology", playerFaction.Technology.ToString());
        table.AddRow("Influence", playerFaction.Influence.ToString());
        table.AddRow("Resources", playerFaction.Resources.ToString());
        table.AddRow("Stability", playerFaction.Stability.ToString());
        table.AddRow(
            "Reputation",
            $"{playerFaction.Reputation} {GetReputationDescription(playerFaction.Reputation)}"
        );
        AnsiConsole.Write(table);
    }

    /// <summary>
    /// Validates and processes player actions for the current turn.
    /// </summary>
    /// <param name="playerActions">Actions to process.</param>
    private async Task ProcessTurnAsync(List<PlayerAction> playerActions)
    {
        if (_gameEngine.CurrentGame == null)
        {
            return;
        }
        var validActions = new List<PlayerAction>();
        foreach (var action in playerActions)
        {
            var result = _playerActionValidator.Validate(action);
            if (result.IsValid)
            {
                validActions.Add(action);
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    AnsiConsole.MarkupLine($"[red]{error.ErrorMessage}[/]");
                }
            }
        }
        await _gameEngine.ProcessTurnAsync(validActions);
        AnsiConsole.MarkupLine("[green]Turn processed![/]");
        AnsiConsole.MarkupLine("Press any key to continue...");
        Console.ReadKey();
    }

    /// <summary>
    /// Gets a description for a faction type.
    /// </summary>
    /// <param name="type">Faction type.</param>
    /// <returns>Description string.</returns>
    private static string GetFactionTypeDescription(FactionType type)
    {
        return type switch
        {
            FactionType.MilitaryJunta => FactionDescriptions.MilitaryJunta,
            FactionType.CorporateCouncil => FactionDescriptions.CorporateCouncil,
            FactionType.ReligiousOrder => FactionDescriptions.ReligiousOrder,
            FactionType.PirateAlliance => FactionDescriptions.PirateAlliance,
            FactionType.TechnocraticUnion => FactionDescriptions.TechnocraticUnion,
            FactionType.RebellionCell => FactionDescriptions.RebellionCell,
            FactionType.ImperialRemnant => FactionDescriptions.ImperialRemnant,
            FactionType.AncientAwakened => FactionDescriptions.AncientAwakened,
            _ => FactionDescriptions.Default,
        };
    }

    /// <summary>
    /// Shows a tooltip if an action is currently blocked.
    /// </summary>
    /// <param name="actionType">The action type to check.</param>
    private void ShowActionTooltip(PlayerActionType actionType)
    {
        if (
            _gameEngine.CurrentGame != null
            && _gameEngine.CurrentGame.BlockedActions.Contains(actionType)
        )
        {
            AnsiConsole.MarkupLine(
                "[red]This action is currently blocked due to a recent event![/]"
            );
        }
        string desc = actionType switch
        {
            PlayerActionType.BuildDefenses => ActionDescriptions.BuildDefenses,
            PlayerActionType.RecruitTroops => ActionDescriptions.RecruitTroops,
            PlayerActionType.DevelopInfrastructure => ActionDescriptions.DevelopInfrastructure,
            PlayerActionType.ExploitResources => ActionDescriptions.ExploitResources,
            PlayerActionType.MilitaryTech => ActionDescriptions.MilitaryTech,
            PlayerActionType.EconomicTech => ActionDescriptions.EconomicTech,
            PlayerActionType.AncientStudies => ActionDescriptions.AncientStudies,
            PlayerActionType.GateNetworkResearch => ActionDescriptions.GateNetworkResearch,
            PlayerActionType.Diplomacy => ActionDescriptions.Diplomacy,
            PlayerActionType.Espionage => ActionDescriptions.Espionage,
            _ => ActionDescriptions.Default,
        };
        AnsiConsole.MarkupLine($"[grey]{desc}[/]");
    }

    /// <summary>
    /// Gets a short reputation description for a given value.
    /// </summary>
    /// <param name="reputation">Reputation value.</param>
    /// <returns>Description string.</returns>
    private static string GetReputationDescription(int reputation)
    {
        reputation = Math.Max(
            GameConstants.MinReputation,
            Math.Min(reputation, GameConstants.MaxReputation)
        );
        if (reputation >= 80)
        {
            return NewsTemplates.LegendaryDesc;
        }
        if (reputation >= 40)
        {
            return NewsTemplates.RespectedDesc;
        }
        if (reputation >= 10)
        {
            return NewsTemplates.NotedDesc;
        }
        if (reputation <= -80)
        {
            return "[red](Infamous)[/]";
        }
        if (reputation <= -40)
        {
            return "[red](Feared)[/]";
        }
        if (reputation <= -10)
        {
            return "[yellow](Notorious)[/]";
        }
        return NewsTemplates.NeutralDesc;
    }

    /// <summary>
    /// Displays the effects of a game event.
    /// </summary>
    /// <param name="gameEvent">The event to display.</param>
    private static void ShowEventEffects(GameEvent gameEvent)
    {
        if (gameEvent.Effects != null && gameEvent.Effects.Count > 0)
        {
            AnsiConsole.MarkupLine("[bold yellow]Effects:[/]");
            foreach (var effect in gameEvent.Effects)
            {
                var statName = effect.Key.GetDisplayName();
                int value = effect.Value;
                string sign = value > 0 ? "+" : "";
                AnsiConsole.MarkupLine($"  [aqua]{statName}[/]: [bold]{sign}{value}[/]");
            }
        }
        if (gameEvent.BlockedActions != null && gameEvent.BlockedActions.Count > 0)
        {
            AnsiConsole.MarkupLine("[bold orange1]Blocked Actions Next Turn:[/]");
            foreach (var action in gameEvent.BlockedActions)
            {
                AnsiConsole.MarkupLine($"  [red]{action.GetDisplayName()}[/]");
            }
        }
    }

    /// <summary>
    /// Shows the event log for the current game.
    /// </summary>
    /// <param name="game">The game state to show events for.</param>
    private static void ShowEventLog(GameState? game)
    {
        if (game == null || (game.GalacticHistory.Count == 0 && game.RecentEvents.Count == 0))
        {
            AnsiConsole.MarkupLine("[grey]No events have occurred yet.[/]");
            return;
        }
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold yellow]Event Log:[/]");
        int count = 1;
        foreach (var entry in game.GalacticHistory)
        {
            AnsiConsole.MarkupLine($"[dim]{count++}.[/] {entry}");
        }
        if (game.RecentEvents.Count > 0)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[bold]Recent Events:[/]");
            foreach (var ev in game.RecentEvents)
            {
                AnsiConsole.MarkupLine($"[dim]{count++}.[/] [aqua]{ev.Title}[/]: {ev.Description}");
            }
        }
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[grey]Press any key to return to the main menu...[/]");
        Console.ReadKey(true);
    }

    /// <summary>
    /// Exports the current game save as JSON.
    /// </summary>
    private void ExportSave()
    {
        var game = _gameEngine.CurrentGame;
        if (game == null)
        {
            AnsiConsole.MarkupLine("[red]No game loaded to export.[/]");
            AnsiConsole.MarkupLine("Press any key to return...");
            Console.ReadKey();
            return;
        }
        var json =
            _gameEngine.CurrentGame != null
                ? _gameEngine.GameDataService.ExportGameState(game)
                : string.Empty;
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold green]Exported Save (JSON):[/]");
        AnsiConsole.WriteLine(json);
        ClipboardService.SetText(json);
        AnsiConsole.MarkupLine(
            "[grey]Save JSON copied to clipboard! Paste it anywhere to back up or share.[/]"
        );
        AnsiConsole.MarkupLine("Press any key to return...");
        Console.ReadKey();
    }

    /// <summary>
    /// Imports a game save from JSON.
    /// </summary>
    private async Task ImportSaveAsync()
    {
        AnsiConsole.MarkupLine(
            "[bold yellow]Paste your exported save JSON below. Press Enter when done.[/]"
        );
        var json = AnsiConsole.Ask<string>("Paste JSON:");
        try
        {
            var imported = _gameEngine.GameDataService.ImportGameState(json);
            Guard.IsNotNull(imported, "Failed to import game state from JSON.");
            await _gameEngine.GameDataService.SaveGameAsync(imported);
            _gameEngine.SetCurrentGame(imported);
            AnsiConsole.MarkupLine("[green]Game imported and loaded successfully![/]");
            AnsiConsole.MarkupLine("Press any key to continue...");
            Console.ReadKey();
            await RunGameLoopAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error importing save JSON.");
            AnsiConsole.MarkupLine($"[red]Failed to import save: {ex.Message}[/]");
            AnsiConsole.MarkupLine("Press any key to return...");
            Console.ReadKey();
        }
    }

    /// <summary>
    /// Displays all unlocked global achievements.
    /// </summary>
    private void ShowGlobalAchievements()
    {
        _logger.Debug("Displaying global achievements.");
        var achievements = _globalAchievementService.GetAllAchievements();
        if (achievements.Count == 0)
        {
            _logger.Warning("No global achievements unlocked.");
            AnsiConsole.MarkupLine("[yellow]No global achievements unlocked yet.[/]");
        }
        var table = new Table()
            .Border(TableBorder.Rounded)
            .Title("[bold green]Global Achievements[/]");
        table.AddColumn("Name");
        table.AddColumn("Description");
        table.AddColumn("Unlocked At");
        foreach (var ach in achievements)
        {
            table.AddRow($"[bold]{ach.Name}[/]", ach.Description, ach.UnlockedAt.ToString("u"));
        }
        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("Press any key to return to the menu...");
        Console.ReadKey(true);
    }
}

using CommunityToolkit.Diagnostics;
using FactionsAtTheEnd.Core;
using FactionsAtTheEnd.Models;
using FluentValidation;
using Spectre.Console;
using TextCopy;

namespace FactionsAtTheEnd.UI;

public class GameUI
{
    private readonly GameEngine _gameEngine;
    private readonly IValidator<Faction> _factionValidator;
    private readonly IValidator<PlayerAction> _playerActionValidator;

    public GameUI(
        GameEngine gameEngine,
        IValidator<Faction> factionValidator,
        IValidator<PlayerAction> playerActionValidator
    )
    {
        Guard.IsNotNull(gameEngine, nameof(gameEngine));
        Guard.IsNotNull(factionValidator, nameof(factionValidator));
        Guard.IsNotNull(playerActionValidator, nameof(playerActionValidator));
        _gameEngine = gameEngine;
        _factionValidator = factionValidator;
        _playerActionValidator = playerActionValidator;
    }

    public async Task RunMainMenuAsync()
    {
        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine("[bold red]🔮 FACTIONS AT THE END 🔮[/]");
            AnsiConsole.WriteLine();

            var menuOptions = new[]
            {
                MenuOption.NewGame,
                MenuOption.LoadGame,
                MenuOption.Help,
                MenuOption.Exit,
                (MenuOption)1000,
                (MenuOption)1001,
            };
            var menuLabels = new Dictionary<MenuOption, string>
            {
                { MenuOption.NewGame, "New Game" },
                { MenuOption.LoadGame, "Load Game" },
                { MenuOption.Help, "Help" },
                { MenuOption.Exit, "Exit" },
                { (MenuOption)1000, "Export Save (JSON)" },
                { (MenuOption)1001, "Import Save (JSON)" },
            };
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<MenuOption>()
                    .Title("What would you like to do?")
                    .AddChoices(menuOptions)
                    .UseConverter(opt =>
                        menuLabels.TryGetValue(opt, out string? value) ? value : opt.ToString()
                    )
            );

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
                    AnsiConsole.MarkupLine("[yellow]May your faction survive the darkness...[/]");
                    return;
                case (MenuOption)1000:
                    ExportSave();
                    break;
                case (MenuOption)1001:
                    await ImportSaveAsync();
                    break;
            }
        }
    }

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
            "- [green]Achievements[/]: Special milestones will be announced as you play."
        );
        AnsiConsole.MarkupLine("- [green]Tooltips[/]: Hover or select actions for more info.");
        AnsiConsole.MarkupLine("\nPress any key to return...");
        Console.ReadKey();
    }

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
            foreach (var error in validationResult.Errors)
            {
                AnsiConsole.MarkupLine($"[red]{error.ErrorMessage}[/]");
            }
            AnsiConsole.MarkupLine("Press any key to try again...");
            Console.ReadKey();
            await StartNewGameAsync();
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

    private async Task LoadGameAsync()
    {
        var savedGames = await _gameEngine.GetSavedGamesAsync();

        if (savedGames.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]No saved games found.[/]");
            AnsiConsole.MarkupLine("Press any key to continue...");
            Console.ReadKey();
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

    private async Task RunGameLoopAsync()
    {
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
            if (
                playerFaction.Population <= 0
                || playerFaction.Resources <= 0
                || playerFaction.Stability <= 0
            )
            {
                AnsiConsole.MarkupLine("[bold red]Your faction has collapsed![/]");
                AnsiConsole.MarkupLine(
                    "[yellow]Game Over. Press any key to return to the main menu.[/]"
                );
                Console.ReadKey();
                break;
            }
            // Win condition
            if (
                game.SaveName == "WINNER"
                || playerFaction.Technology >= 100
                || game.CurrentCycle > 20
            )
            {
                AnsiConsole.MarkupLine(
                    "[bold green]Congratulations! You have survived and triumphed in the dying galaxy![/]"
                );
                AnsiConsole.MarkupLine(
                    "[yellow]You win! Press any key to return to the main menu.[/]"
                );
                Console.ReadKey();
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
            // Show Galactic News
            if (game.GalacticNews.Count > 0)
            {
                AnsiConsole.MarkupLine("[bold underline]Galactic News:[/]");
                foreach (var news in game.GalacticNews.TakeLast(5))
                {
                    AnsiConsole.MarkupLine($"[aqua]{news}[/]");
                }
                AnsiConsole.MarkupLine("");
            }
            // Show Galactic History snippet
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
            // Show pre-turn stats for feedback
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

            // After turn processing, refresh game state
            game = _gameEngine.CurrentGame;
            Guard.IsNotNull(game, nameof(game));
            playerFaction = game.PlayerFaction;
            Guard.IsNotNull(playerFaction, nameof(playerFaction));

            // Display events that occurred this turn (only once, after processing)
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

            // Handle player choice events
            var choiceEvent = game.RecentEvents.FirstOrDefault(e =>
                e.Choices != null && e.Choices.Count > 0
            );
            if (choiceEvent?.Choices != null && choiceEvent.Choices.Count > 0)
            {
                var choiceDescriptions = choiceEvent.Choices.Select(c => c.Description).ToList();
                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title($"[yellow]{choiceEvent.Title}[/] - {choiceEvent.Description}")
                        .AddChoices(choiceDescriptions)
                );
                var selectedChoice = choiceEvent.Choices.First(c => c.Description == choice);
                // Apply effects
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
                // Block actions if any
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

            // Show post-turn feedback
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

            // Check if "Ancient Studies" was just unblocked by an event this turn
            var unblockedActions = new List<PlayerActionType>();
            var prevBlocked = new HashSet<PlayerActionType>(game.BlockedActions);
            // After processing turn, compare blocked actions before and after
            var blockedLastTurn = prevBlocked;
            var blockedThisTurn = new HashSet<PlayerActionType>(game.BlockedActions);
            // Find actions that were blocked last turn but not this turn
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

            // Achievements
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
        // Show Galactic News
        if (game.GalacticNews.Count > 0)
        {
            AnsiConsole.MarkupLine("[bold underline]Galactic News:[/]");
            foreach (var news in game.GalacticNews.TakeLast(5))
            {
                AnsiConsole.MarkupLine($"[aqua]{news}[/]");
            }
            AnsiConsole.MarkupLine("");
        }
        // Show Galactic History snippet
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

    // Returns a short description for the player's reputation
    private static string GetReputationDescription(int reputation)
    {
        // Clamp reputation to -100 to 100
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

    private static void ShowEventEffects(GameEvent gameEvent)
    {
        // Show stat/resource changes
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
        // Show blocked actions
        if (gameEvent.BlockedActions != null && gameEvent.BlockedActions.Count > 0)
        {
            AnsiConsole.MarkupLine("[bold orange1]Blocked Actions Next Turn:[/]");
            foreach (var action in gameEvent.BlockedActions)
            {
                AnsiConsole.MarkupLine($"  [red]{action.GetDisplayName()}[/]");
            }
        }
    }

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
        // Show all galactic history entries (narrative log)
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
        // Copy to clipboard
        ClipboardService.SetText(json);
        AnsiConsole.MarkupLine(
            "[grey]Save JSON copied to clipboard! Paste it anywhere to back up or share.[/]"
        );
        AnsiConsole.MarkupLine("Press any key to return...");
        Console.ReadKey();
    }

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
            AnsiConsole.MarkupLine($"[red]Failed to import save: {ex.Message}[/]");
            AnsiConsole.MarkupLine("Press any key to return...");
            Console.ReadKey();
        }
    }
}

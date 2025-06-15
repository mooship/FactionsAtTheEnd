using FactionsAtTheEnd.Core;
using FactionsAtTheEnd.Interfaces;
using FactionsAtTheEnd.Models;
using FluentValidation;
using Spectre.Console;

namespace FactionsAtTheEnd.UI;

// Single-player, single-faction MVP.
// This UI is focused on the player faction only.
public class GameUI(
    GameEngine gameEngine,
    IFactionService factionService,
    IValidator<Faction> factionValidator,
    IValidator<PlayerAction> playerActionValidator
)
{
    private readonly GameEngine _gameEngine = gameEngine;
    private readonly IFactionService _factionService = factionService;
    private readonly IValidator<Faction> _factionValidator = factionValidator;
    private readonly IValidator<PlayerAction> _playerActionValidator = playerActionValidator;

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
            };
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<MenuOption>()
                    .Title("What would you like to do?")
                    .AddChoices(menuOptions)
                    .UseConverter(opt => opt.GetDisplayName())
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
            }
        }
    }

    private void ShowHelp()
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine("[bold cyan]Game Help & Tips[/]");
        AnsiConsole.MarkupLine("[yellow]Actions:[/]");
        foreach (var action in Enum.GetValues<PlayerActionType>())
        {
            AnsiConsole.MarkupLine(
                $"[green]{action.GetDisplayName()}[/]: {GetActionDescription(action)}"
            );
        }
        AnsiConsole.MarkupLine("\n[yellow]Stats:[/]");
        AnsiConsole.MarkupLine(
            "Population: Number of people in your faction. If this reaches 0, you lose."
        );
        AnsiConsole.MarkupLine(
            "Military: Your armed strength. Needed for defense and some events."
        );
        AnsiConsole.MarkupLine("Technology: Your scientific progress. Reach 100 to win.");
        AnsiConsole.MarkupLine("Influence: Your political and social sway.");
        AnsiConsole.MarkupLine("Resources: Supplies and wealth. If this reaches 0, you lose.");
        AnsiConsole.MarkupLine(
            "Stability: How stable your faction is. If this reaches 0, you lose."
        );
        AnsiConsole.MarkupLine(
            "\n[grey]Blocked actions are shown in red and cannot be selected during a turn. Hover for details.[/]"
        );
        AnsiConsole.MarkupLine("\n[bold yellow]Faction Types & Traits:[/]");
        foreach (var type in Enum.GetValues<FactionType>())
        {
            AnsiConsole.MarkupLine(
                $"[aqua]{type.GetDisplayName()}[/]: {GetFactionTypeDescription(type)} Traits: [grey]{string.Join(", ", _factionService.CreateFaction("", type).Traits)}[/]"
            );
        }
        AnsiConsole.MarkupLine("\nPress any key to return...");
        Console.ReadKey();
    }

    private static string GetActionDescription(PlayerActionType action)
    {
        return action switch
        {
            PlayerActionType.Build_Defenses => "Increase your defenses to resist attacks.",
            PlayerActionType.Recruit_Troops => "Recruit new soldiers to boost military.",
            PlayerActionType.Develop_Infrastructure => "Improve facilities for long-term growth.",
            PlayerActionType.Exploit_Resources => "Gather more resources for your faction.",
            PlayerActionType.Military_Tech => "Research new military technologies.",
            PlayerActionType.Economic_Tech => "Research economic improvements.",
            PlayerActionType.Ancient_Studies => "Study ancient relics for unique benefits.",
            PlayerActionType.Gate_Network_Research => "Research the lost gate network.",
            _ => "(No description available)",
        };
    }

    private async Task StartNewGameAsync()
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine("[bold cyan]Creating New Game[/]");
        AnsiConsole.WriteLine();

        var factionName = AnsiConsole.Ask<string>("What is your faction's [bold green]name[/]?");

        var factionType = AnsiConsole.Prompt(
            new SelectionPrompt<FactionType>()
                .Title("Choose your faction type:")
                .AddChoices(Enum.GetValues<FactionType>())
                .UseConverter(type => type.GetDisplayName())
        );

        // Validate faction input
        var tempFaction = new Faction
        {
            Name = factionName,
            Type = factionType,
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
            $"\n[yellow]Creating faction '{factionName}' of type {factionType}...[/]"
        );

        var gameState = await _gameEngine.CreateNewGameAsync(factionName, factionType);

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
            if (game == null)
            {
                AnsiConsole.MarkupLine("[red]Error: Game state not found.[/]");
                return;
            }
            var playerFaction = game.Factions.FirstOrDefault(f => f.Id == game.PlayerFactionId);
            if (playerFaction == null)
            {
                AnsiConsole.MarkupLine("[red]Error: Player faction not found.[/]");
                return;
            }

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
                AnsiConsole.Clear();
                AnsiConsole.MarkupLine("[bold red]GAME OVER! Your faction has collapsed.[/]");
                AnsiConsole.MarkupLine($"[yellow]You survived {game.CurrentCycle - 1} turns.[/]");
                AnsiConsole.MarkupLine(
                    $"[grey]Final Stats: Population: {playerFaction.Population}, Military: {playerFaction.Military}, Technology: {playerFaction.Technology}, Influence: {playerFaction.Influence}, Resources: {playerFaction.Resources}, Stability: {playerFaction.Stability}[/]"
                );
                AnsiConsole.MarkupLine(
                    "[italic]The galaxy grows darker as your legacy fades...[/]"
                );
                AnsiConsole.MarkupLine("Press any key to return to the main menu...");
                Console.ReadKey();
                return;
            }

            // Win condition
            if (game.SaveName == "WINNER")
            {
                AnsiConsole.Clear();
                AnsiConsole.MarkupLine(
                    "[bold green]CONGRATULATIONS! You have survived the end.[/]"
                );
                AnsiConsole.MarkupLine(
                    $"[yellow]You survived {game.CurrentCycle - 1} turns and reached {playerFaction.Technology} Technology.[/]"
                );
                AnsiConsole.MarkupLine(
                    $"[grey]Final Stats: Population: {playerFaction.Population}, Military: {playerFaction.Military}, Technology: {playerFaction.Technology}, Influence: {playerFaction.Influence}, Resources: {playerFaction.Resources}, Stability: {playerFaction.Stability}[/]"
                );
                AnsiConsole.MarkupLine(
                    "[italic]Your name will echo in the annals of the fallen empire...[/]"
                );
                AnsiConsole.MarkupLine("Press any key to return to the main menu...");
                Console.ReadKey();
                return;
            }

            AnsiConsole.Clear();
            // Display current turn number
            AnsiConsole.MarkupLine($"[bold blue]Turn {game.CurrentCycle}[/]");
            DisplayGameState();

            var playerActions = new List<PlayerAction>();
            // var game = _gameEngine.CurrentGame!;
            // var playerFaction = game.Factions.First(f => f.Id == game.PlayerFactionId);

            // Game over check
            // if (
            //     playerFaction.Population <= 0
            //     || playerFaction.Resources <= 0
            //     || playerFaction.Stability <= 0
            // )
            // {
            //     AnsiConsole.MarkupLine("[bold red]GAME OVER! Your faction has collapsed.[/]");
            //     AnsiConsole.MarkupLine($"Final Cycle: {game.CurrentCycle}");
            //     AnsiConsole.MarkupLine("Press any key to return to the main menu...");
            //     Console.ReadKey();
            //     break;
            // }

            var mainChoices = new[]
            {
                "Take Action",
                "View Faction Overview",
                "Help",
                "Exit to Main Menu",
            };
            var mainChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("What would you like to do?")
                    .AddChoices(mainChoices)
            );
            if (mainChoice == "Help")
            {
                ShowHelp();
                continue;
            }
            if (mainChoice == "View Faction Overview")
            {
                AnsiConsole.Clear();
                DisplayFactionsOverview();
                AnsiConsole.MarkupLine("Press any key to return...");
                Console.ReadKey();
                continue;
            }
            if (mainChoice == "Exit to Main Menu")
            {
                break;
            }

            var actionChoices = new[]
            {
                PlayerActionType.Build_Defenses,
                PlayerActionType.Recruit_Troops,
                PlayerActionType.Develop_Infrastructure,
                PlayerActionType.Exploit_Resources,
                PlayerActionType.Military_Tech,
                PlayerActionType.Economic_Tech,
                PlayerActionType.Ancient_Studies,
                PlayerActionType.Gate_Network_Research,
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
                playerActions.Add(
                    new PlayerAction { ActionType = selectedAction, FactionId = playerFaction.Id }
                );
                chosenActions.Add(selectedAction);
            }

            // Show pre-turn stats for feedback
            var preStats = new
            {
                playerFaction.Population,
                playerFaction.Military,
                playerFaction.Technology,
                playerFaction.Influence,
                playerFaction.Resources,
                playerFaction.Stability,
            };

            await ProcessTurnAsync(playerActions);

            // Display events that occurred this turn
            var recentEvents = game.RecentEvents.Where(e => e.Cycle == game.CurrentCycle).ToList();
            if (recentEvents.Count > 0)
            {
                AnsiConsole.MarkupLine("[bold yellow]Events this turn:[/]");
                foreach (var ev in recentEvents)
                {
                    AnsiConsole.MarkupLine($"[underline]{ev.Title}[/]: {ev.Description}");
                    if (ev.Effects != null && ev.Effects.Count > 0)
                    {
                        var effects = string.Join(
                            ", ",
                            ev.Effects.Select(kv =>
                                $"{kv.Key} {(kv.Value >= 0 ? "+" : "")}{kv.Value}"
                            )
                        );
                        AnsiConsole.MarkupLine($"[grey]Effects: {effects}[/]");
                    }
                }
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
            };
            AnsiConsole.MarkupLine("[bold green]Turn Results:[/]");
            ShowStatChange("Population", preStats.Population, postStats.Population);
            ShowStatChange("Military", preStats.Military, postStats.Military);
            ShowStatChange("Technology", preStats.Technology, postStats.Technology);
            ShowStatChange("Influence", preStats.Influence, postStats.Influence);
            ShowStatChange("Resources", preStats.Resources, postStats.Resources);
            ShowStatChange("Stability", preStats.Stability, postStats.Stability);
            AnsiConsole.MarkupLine("Press any key to continue...");
            Console.ReadKey();

            // --- UI/UX Feedback for Unblocked Actions ---
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
        var playerFaction = game.Factions.First(f => f.Id == game.PlayerFactionId);
        AnsiConsole.MarkupLine($"[bold]Cycle:[/] {game.CurrentCycle}");
        AnsiConsole.MarkupLine(
            $"[bold]Faction:[/] {playerFaction.Name} ({playerFaction.Type.GetDisplayName()})"
        );
        AnsiConsole.MarkupLine($"[bold]Population:[/] {playerFaction.Population}");
        AnsiConsole.MarkupLine($"[bold]Military:[/] {playerFaction.Military}");
        AnsiConsole.MarkupLine($"[bold]Technology:[/] {playerFaction.Technology}");
        AnsiConsole.MarkupLine($"[bold]Influence:[/] {playerFaction.Influence}");
        AnsiConsole.MarkupLine($"[bold]Resources:[/] {playerFaction.Resources}");
        AnsiConsole.MarkupLine($"[bold]Stability:[/] {playerFaction.Stability}");
        AnsiConsole.MarkupLine("");
        if (game.RecentEvents.Count != 0)
        {
            AnsiConsole.MarkupLine("[bold underline]Recent Events:[/]");
            foreach (var e in game.RecentEvents)
            {
                AnsiConsole.MarkupLine($"[yellow]{e.Title}[/]: {e.Description}");
            }
        }
        // Show anti-spam feedback if any actions are blocked due to spamming
        if (game.BlockedActions.Count > 0)
        {
            AnsiConsole.MarkupLine(
                "[yellow]Some actions are temporarily blocked due to repeated use. Vary your strategy to avoid negative effects![/]"
            );
        }
    }

    // DisplayFactionsOverview is now implemented and used in the main game loop.
    private void DisplayFactionsOverview()
    {
        var game = _gameEngine.CurrentGame!;
        var playerFaction = game.Factions.First(f => f.Id == game.PlayerFactionId);
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
        table.AddRow("Status", playerFaction.Status.ToString());
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
        // Show anti-spam warning if action is currently blocked due to spamming
        if (
            _gameEngine.CurrentGame != null
            && _gameEngine.CurrentGame.BlockedActions.Contains(actionType)
        )
        {
            AnsiConsole.MarkupLine(
                $"[yellow]You have been blocked from using [bold]{actionType.GetDisplayName()}[/] this turn due to repeated use. Try varying your strategy![/]"
            );
        }
        switch (actionType)
        {
            case PlayerActionType.Ancient_Studies:
                AnsiConsole.MarkupLine(
                    "[grey]Ancient Studies: Study ancient relics for unique benefits. This action may be blocked until you discover ancient technology through special events.[/]"
                );
                break;
            // Add more tooltips for other actions as needed
            default:
                AnsiConsole.MarkupLine(
                    $"[grey]{actionType.GetDisplayName()}: {GetActionDescription(actionType)}[/]"
                );
                break;
        }
    }
}

// Add extension method for FactionType display name if not present
public static class FactionTypeExtensions
{
    public static string GetDisplayName(this FactionType type)
    {
        var typeInfo = type.GetType();
        var memInfo = typeInfo.GetMember(type.ToString());
        if (memInfo.Length > 0)
        {
            var attrs = memInfo[0]
                .GetCustomAttributes(
                    typeof(System.ComponentModel.DataAnnotations.DisplayAttribute),
                    false
                );
            if (attrs.Length > 0)
            {
                return ((System.ComponentModel.DataAnnotations.DisplayAttribute)attrs[0]).Name
                    ?? type.ToString();
            }
        }
        return type.ToString();
    }
}

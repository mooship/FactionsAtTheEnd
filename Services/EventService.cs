using FactionsAtTheEnd.Interfaces;
using FactionsAtTheEnd.Models;
using FactionsAtTheEnd.UI;

namespace FactionsAtTheEnd.Services;

/// <summary>
/// Handles all event generation for the single-player, single-faction MVP.
/// All events now only affect the player faction.
/// </summary>
public class EventService : IEventService
{
    public static List<GameEvent> GenerateInitialEvents(GameState gameState)
    {
        var events = new List<GameEvent>
        {
            new()
            {
                Title = "Collapse",
                Description =
                    "The imperial government has fallen. You lead the last organized group in your region. Survival is up to you.",
                Type = EventType.Crisis,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
            },
        };

        return events;
    }

    public static List<GameEvent> GenerateRandomEvents(GameState gameState)
    {
        var events = new List<GameEvent>();
        // If player repeats an action 3+ times in recent turns, trigger a negative event
        foreach (var kvp in gameState.RecentActionCounts)
        {
            if (kvp.Value >= 3)
            {
                events.Add(
                    new GameEvent
                    {
                        Title = $"Repetitive Strategy: {kvp.Key.GetDisplayName()}",
                        Description =
                            $"Your repeated use of {kvp.Key.GetDisplayName()} has led to diminishing returns and unrest.",
                        Type = EventType.Crisis,
                        Cycle = gameState.CurrentCycle,
                        AffectedFactions = [gameState.PlayerFactionId],
                        Effects = new() { { StatKey.Stability, -5 }, { StatKey.Resources, -3 } },
                        BlockedActions = [kvp.Key],
                    }
                );
            }
        }

        // 40% chance of a random event each cycle
        if (Random.Shared.Next(1, 101) <= 40)
        {
            var eventType = GetRandomEventType(gameState);
            var gameEvent = GenerateEventByType(eventType, gameState);
            if (gameEvent != null)
            {
                events.Add(gameEvent);
            }
        }

        // Special events based on world state
        if (gameState.GalacticStability <= 20 && Random.Shared.Next(1, 101) <= 30)
        {
            events.Add(GenerateCrisisEvent(gameState));
        }

        if (gameState.AncientTechDiscovery >= 70 && Random.Shared.Next(1, 101) <= 25)
        {
            events.Add(GenerateAncientTechEvent(gameState));
        }

        return events;
    }

    public async Task<List<GameEvent>> GenerateRandomEventsAsync(GameState gameState)
    {
        return await Task.Run(() => GenerateRandomEvents(gameState));
    }

    private static EventType GetRandomEventType(GameState gameState)
    {
        var eventTypes = new[]
        {
            EventType.Military,
            EventType.Economic,
            EventType.Technological,
            EventType.Discovery,
            EventType.Natural,
        };

        // Crisis events become more likely as stability decreases
        if (gameState.GalacticStability < 30 && Random.Shared.Next(1, 101) <= 30)
        {
            return EventType.Crisis;
        }

        return eventTypes[Random.Shared.Next(eventTypes.Length)];
    }

    private static GameEvent? GenerateEventByType(EventType eventType, GameState gameState)
    {
        return eventType switch
        {
            EventType.Military => GenerateMilitaryEvent(gameState),
            EventType.Economic => GenerateEconomicEvent(gameState),
            EventType.Technological => GenerateTechnologicalEvent(gameState),
            EventType.Discovery => GenerateDiscoveryEvent(gameState),
            EventType.Natural => GenerateNaturalEvent(gameState),
            EventType.Crisis => GenerateCrisisEvent(gameState),
            _ => null,
        };
    }

    /// <summary>
    /// Generates a military event, including positive and faction-specific outcomes.
    /// </summary>
    private static GameEvent GenerateMilitaryEvent(GameState gameState)
    {
        var player = gameState.Factions.FirstOrDefault(f => f.Id == gameState.PlayerFactionId);
        var index = Random.Shared.Next(10);
        // Faction-specific positive event for Military Junta
        if (player?.Type == FactionType.MilitaryJunta && Random.Shared.Next(1, 101) <= 20)
        {
            return new GameEvent
            {
                Title = "Veteran Parade",
                Description = "A parade of veterans inspires your troops and citizens alike.",
                Type = EventType.Military,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Military, 8 }, { StatKey.Stability, 4 } },
            };
        }
        // Faction-specific event for Pirate Alliance
        if (player?.Type == FactionType.PirateAlliance && Random.Shared.Next(1, 101) <= 20)
        {
            return new GameEvent
            {
                Title = "Successful Raid",
                Description = "Your pirates pull off a daring raid, boosting resources and morale!",
                Type = EventType.Military,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new()
                {
                    { StatKey.Military, 5 },
                    { StatKey.Resources, 8 },
                    { StatKey.Stability, 2 },
                },
            };
        }
        // Faction-specific event for Rebellion Cell
        if (player?.Type == FactionType.RebellionCell && Random.Shared.Next(1, 101) <= 20)
        {
            return new GameEvent
            {
                Title = "Sabotage Success",
                Description = "Your rebels sabotage enemy supplies, gaining support and resources.",
                Type = EventType.Military,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new()
                {
                    { StatKey.Military, 4 },
                    { StatKey.Resources, 6 },
                    { StatKey.Influence, 3 },
                },
            };
        }
        return index switch
        {
            0 => new GameEvent
            {
                Title = "Raiders Attack",
                Description =
                    "A group of raiders attacks your supply lines, straining your defenses.",
                Type = EventType.Military,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Military, -5 }, { StatKey.Resources, -10 } },
                BlockedActions = [PlayerActionType.Exploit_Resources],
            },
            1 => new GameEvent
            {
                Title = "Internal Mutiny",
                Description = "A mutiny breaks out among your troops, threatening stability.",
                Type = EventType.Military,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Military, -8 }, { StatKey.Stability, -5 } },
                BlockedActions = [PlayerActionType.Recruit_Troops],
            },
            2 => new GameEvent
            {
                Title = "Mercenary Trouble",
                Description = "Mercenaries demand higher pay or threaten to desert.",
                Type = EventType.Military,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Resources, -7 }, { StatKey.Military, -3 } },
            },
            3 => new GameEvent
            {
                Title = "Elite Training",
                Description = "Your officers organize elite training, boosting your forces.",
                Type = EventType.Military,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Military, 10 }, { StatKey.Stability, 2 } },
            },
            4 => new GameEvent
            {
                Title = "Border Skirmish",
                Description =
                    "A border skirmish tests your readiness. Losses are minimal, but morale is shaken.",
                Type = EventType.Military,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Military, -3 }, { StatKey.Stability, -2 } },
            },
            5 => new GameEvent
            {
                Title = "Veteran Recruits",
                Description =
                    "Veterans from other regions join your cause, strengthening your army.",
                Type = EventType.Military,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Military, 7 }, { StatKey.Population, 2 } },
            },
            6 => new GameEvent
            {
                Title = "Peaceful Garrison",
                Description = "Your garrisons report no incidents. Troops rest and recover.",
                Type = EventType.Military,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Military, 3 }, { StatKey.Stability, 2 } },
            },
            7 => new GameEvent
            {
                Title = "Training Accident",
                Description = "A minor accident during training causes a brief setback.",
                Type = EventType.Military,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Military, -2 } },
            },
            _ => new GameEvent(),
        };
    }

    /// <summary>
    /// Generates an economic event, including positive and faction-specific outcomes.
    /// </summary>
    private static GameEvent GenerateEconomicEvent(GameState gameState)
    {
        var player = gameState.Factions.FirstOrDefault(f => f.Id == gameState.PlayerFactionId);
        var index = Random.Shared.Next(8);
        // Faction-specific positive event for Corporate Council
        if (player?.Type == FactionType.CorporateCouncil && Random.Shared.Next(1, 101) <= 20)
        {
            return new GameEvent
            {
                Title = "Market Boom",
                Description = "A surge in the market brings a windfall to your coffers.",
                Type = EventType.Economic,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Resources, 12 }, { StatKey.Influence, 4 } },
            };
        }
        // Faction-specific event for Religious Order
        if (player?.Type == FactionType.ReligiousOrder && Random.Shared.Next(1, 101) <= 20)
        {
            return new GameEvent
            {
                Title = "Tithes and Offerings",
                Description = "The faithful donate generously, swelling your resources.",
                Type = EventType.Economic,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Resources, 8 }, { StatKey.Stability, 3 } },
            };
        }
        return index switch
        {
            0 => new GameEvent
            {
                Title = "Resource Shortage",
                Description = "Critical resources become scarce due to supply chain disruptions.",
                Type = EventType.Economic,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Resources, -12 } },
                BlockedActions = [PlayerActionType.Exploit_Resources],
            },
            1 => new GameEvent
            {
                Title = "Market Instability",
                Description = "Economic uncertainty causes prices to fluctuate wildly.",
                Type = EventType.Economic,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Resources, -8 }, { StatKey.Stability, -3 } },
            },
            2 => new GameEvent
            {
                Title = "Black Market Surge",
                Description = "Illegal goods flood your markets as law enforcement breaks down.",
                Type = EventType.Economic,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Influence, -5 }, { StatKey.Resources, -5 } },
            },
            3 => new GameEvent
            {
                Title = "Trade Convoy Arrives",
                Description = "A friendly trade convoy brings much-needed supplies.",
                Type = EventType.Economic,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Resources, 15 }, { StatKey.Stability, 2 } },
            },
            4 => new GameEvent
            {
                Title = "Smuggling Ring Busted",
                Description = "You uncover a smuggling ring, recovering stolen goods.",
                Type = EventType.Economic,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Resources, 8 }, { StatKey.Influence, 3 } },
            },
            5 => new GameEvent
            {
                Title = "Resource Windfall",
                Description = "A new resource deposit is discovered, boosting your economy!",
                Type = EventType.Economic,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Resources, 12 } },
            },
            6 => new GameEvent
            {
                Title = "Efficient Logistics",
                Description = "Your supply officers optimize routes, saving resources.",
                Type = EventType.Economic,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Resources, 5 } },
            },
            7 => new GameEvent
            {
                Title = "Charity Drive",
                Description = "A charity drive boosts morale and stability among the people.",
                Type = EventType.Economic,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Stability, 3 } },
            },
            _ => new GameEvent(),
        };
    }

    /// <summary>
    /// Generates a technological event, including positive and faction-specific outcomes.
    /// </summary>
    private static GameEvent GenerateTechnologicalEvent(GameState gameState)
    {
        var player = gameState.Factions.FirstOrDefault(f => f.Id == gameState.PlayerFactionId);
        var index = Random.Shared.Next(8);
        // Faction-specific positive event for Technocratic Union
        if (player?.Type == FactionType.TechnocraticUnion && Random.Shared.Next(1, 101) <= 20)
        {
            return new GameEvent
            {
                Title = "Breakthrough Algorithm",
                Description =
                    "Your scientists develop a revolutionary algorithm, accelerating research.",
                Type = EventType.Technological,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Technology, 15 }, { StatKey.Stability, 3 } },
            };
        }
        // Faction-specific event for Imperial Remnant
        if (player?.Type == FactionType.ImperialRemnant && Random.Shared.Next(1, 101) <= 20)
        {
            return new GameEvent
            {
                Title = "Recovered Imperial Database",
                Description =
                    "You recover a lost imperial database, boosting your technological edge.",
                Type = EventType.Technological,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Technology, 10 }, { StatKey.Influence, 5 } },
            };
        }
        return index switch
        {
            0 => new GameEvent
            {
                Title = "Tech Breakdown",
                Description = "A critical system fails, requiring urgent repairs.",
                Type = EventType.Technological,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Technology, -7 }, { StatKey.Stability, -3 } },
                BlockedActions = [PlayerActionType.Military_Tech],
            },
            1 => new GameEvent
            {
                Title = "Research Breakthrough",
                Description = "Your scientists make a breakthrough, advancing your technology.",
                Type = EventType.Technological,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Technology, 10 }, { StatKey.Resources, -3 } },
            },
            2 => new GameEvent
            {
                Title = "Sabotage Attempt",
                Description = "Saboteurs attempt to disrupt your research efforts.",
                Type = EventType.Technological,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Technology, -5 }, { StatKey.Military, -2 } },
            },
            3 => new GameEvent
            {
                Title = "Unexpected Innovation",
                Description = "A junior scientist invents a new process, boosting morale and tech!",
                Type = EventType.Technological,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Technology, 7 }, { StatKey.Stability, 4 } },
            },
            4 => new GameEvent
            {
                Title = "Prototype Success",
                Description = "A risky prototype works perfectly, giving you an edge.",
                Type = EventType.Technological,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Technology, 8 }, { StatKey.Resources, -2 } },
            },
            5 => new GameEvent
            {
                Title = "Equipment Theft",
                Description = "Thieves steal valuable research equipment, setting you back.",
                Type = EventType.Technological,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Technology, -6 }, { StatKey.Resources, -4 } },
            },
            6 => new GameEvent
            {
                Title = "Tech Festival",
                Description = "A festival celebrating innovation inspires your scientists.",
                Type = EventType.Technological,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Technology, 5 }, { StatKey.Influence, 2 } },
            },
            7 => new GameEvent
            {
                Title = "Failed Experiment",
                Description = "A failed experiment causes a minor setback.",
                Type = EventType.Technological,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Technology, -2 } },
            },
            _ => new GameEvent(),
        };
    }

    /// <summary>
    /// Generates a discovery event, including positive and faction-specific outcomes.
    /// Also includes rare story-driven content for Ancient Awakened.
    /// </summary>
    private static GameEvent GenerateDiscoveryEvent(GameState gameState)
    {
        var player = gameState.Factions.FirstOrDefault(f => f.Id == gameState.PlayerFactionId);
        var index = Random.Shared.Next(8);
        // Faction-specific positive event for Ancient Awakened
        if (player?.Type == FactionType.AncientAwakened && Random.Shared.Next(1, 101) <= 20)
        {
            return new GameEvent
            {
                Title = "Ancient Memory Stirred",
                Description = "A memory from a forgotten age grants your people new insight.",
                Type = EventType.Discovery,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Technology, 10 }, { StatKey.Stability, 5 } },
            };
        }
        // Rare story-driven event for Ancient Awakened
        if (player?.Type == FactionType.AncientAwakened && Random.Shared.Next(1, 101) <= 5)
        {
            return new GameEvent
            {
                Title = "Echoes of the First Empire",
                Description =
                    "Your people recall secrets of the First Empire, unlocking new paths.",
                Type = EventType.Discovery,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Technology, 20 }, { StatKey.Influence, 10 } },
            };
        }
        return index switch
        {
            0 => new GameEvent
            {
                Title = "Ancient Ruins Found",
                Description = "You discover ancient ruins containing valuable technology.",
                Type = EventType.Discovery,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Technology, 5 }, { StatKey.Resources, 3 } },
            },
            1 => new GameEvent
            {
                Title = "Lost Data Recovered",
                Description = "Lost data archives are recovered, revealing secrets of the past.",
                Type = EventType.Discovery,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Technology, 3 }, { StatKey.Stability, 2 } },
            },
            2 => new GameEvent
            {
                Title = "Mysterious Signal Detected",
                Description = "A mysterious signal is detected, hinting at unknown opportunities.",
                Type = EventType.Discovery,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Influence, 4 } },
            },
            3 => new GameEvent
            {
                Title = "Dangerous Relic Activated",
                Description = "A relic malfunctions, causing chaos and blocking research!",
                Type = EventType.Discovery,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Stability, -6 }, { StatKey.Technology, -2 } },
                BlockedActions = [PlayerActionType.Ancient_Studies],
            },
            4 => new GameEvent
            {
                Title = "Alien Artifact",
                Description = "An alien artifact is found, boosting your influence and curiosity.",
                Type = EventType.Discovery,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Influence, 6 }, { StatKey.Technology, 2 } },
            },
            5 => new GameEvent
            {
                Title = "Forgotten Cache",
                Description = "A forgotten cache of supplies is discovered in the ruins.",
                Type = EventType.Discovery,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Resources, 7 }, { StatKey.Stability, 1 } },
            },
            6 => new GameEvent
            {
                Title = "Cultural Exchange",
                Description = "A cultural exchange with outsiders brings new ideas.",
                Type = EventType.Discovery,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Influence, 4 }, { StatKey.Stability, 2 } },
            },
            7 => new GameEvent
            {
                Title = "False Lead",
                Description = "A promising lead turns out to be a dead end.",
                Type = EventType.Discovery,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Technology, -1 } },
            },
            _ => new GameEvent(),
        };
    }

    /// <summary>
    /// Generates a natural event, including positive and faction-specific outcomes.
    /// </summary>
    private static GameEvent GenerateNaturalEvent(GameState gameState)
    {
        var player = gameState.Factions.FirstOrDefault(f => f.Id == gameState.PlayerFactionId);
        var index = Random.Shared.Next(8);
        // Faction-specific positive event for Religious Order
        if (player?.Type == FactionType.ReligiousOrder && Random.Shared.Next(1, 101) <= 20)
        {
            return new GameEvent
            {
                Title = "Pilgrimage Miracle",
                Description = "A miracle during a pilgrimage inspires hope and unity.",
                Type = EventType.Natural,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Stability, 7 }, { StatKey.Population, 3 } },
            };
        }
        return index switch
        {
            0 => new GameEvent
            {
                Title = "Solar Flare",
                Description = "A solar flare disrupts communications and damages equipment.",
                Type = EventType.Natural,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Resources, -6 }, { StatKey.Technology, -3 } },
            },
            1 => new GameEvent
            {
                Title = "Meteor Shower",
                Description = "A meteor shower causes damage to infrastructure.",
                Type = EventType.Natural,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Population, -4 }, { StatKey.Stability, -2 } },
            },
            2 => new GameEvent
            {
                Title = "Plague Outbreak",
                Description = "A sudden outbreak of disease threatens your population.",
                Type = EventType.Natural,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Population, -8 }, { StatKey.Stability, -4 } },
                BlockedActions = [PlayerActionType.Develop_Infrastructure],
            },
            3 => new GameEvent
            {
                Title = "Bountiful Harvest",
                Description = "Against all odds, your crops thrive!",
                Type = EventType.Natural,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Resources, 10 }, { StatKey.Stability, 3 } },
            },
            4 => new GameEvent
            {
                Title = "Earthquake!",
                Description = "A powerful earthquake shakes your settlements, causing damage.",
                Type = EventType.Natural,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Population, -6 }, { StatKey.Resources, -5 } },
            },
            5 => new GameEvent
            {
                Title = "Mild Season",
                Description = "The weather is calm and pleasant, helping your people recover.",
                Type = EventType.Natural,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Stability, 4 }, { StatKey.Population, 2 } },
            },
            6 => new GameEvent
            {
                Title = "Gentle Rains",
                Description = "Gentle rains bring a season of prosperity.",
                Type = EventType.Natural,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Resources, 6 }, { StatKey.Population, 2 } },
            },
            7 => new GameEvent
            {
                Title = "Minor Flood",
                Description = "A minor flood causes inconvenience but little damage.",
                Type = EventType.Natural,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Resources, -2 } },
            },
            _ => new GameEvent(),
        };
    }

    /// <summary>
    /// Generates a crisis event, including soft-fail warnings for low stats.
    /// </summary>
    private static GameEvent GenerateCrisisEvent(GameState gameState)
    {
        var player = gameState.Factions.FirstOrDefault(f => f.Id == gameState.PlayerFactionId);
        // Soft-fail warning if a stat is about to hit 0
        if (player != null)
        {
            if (player.Population <= 5)
            {
                return new GameEvent
                {
                    Title = "Population Crisis",
                    Description =
                        "Your population is on the brink of collapse! Take urgent action.",
                    Type = EventType.Crisis,
                    Cycle = gameState.CurrentCycle,
                    AffectedFactions = [gameState.PlayerFactionId],
                    Effects = new() { { StatKey.Stability, -3 } },
                };
            }
            if (player.Resources <= 5)
            {
                return new GameEvent
                {
                    Title = "Resource Crisis",
                    Description = "Your resources are nearly depleted! Find new supplies soon.",
                    Type = EventType.Crisis,
                    Cycle = gameState.CurrentCycle,
                    AffectedFactions = [gameState.PlayerFactionId],
                    Effects = new() { { StatKey.Stability, -2 } },
                };
            }
            if (player.Stability <= 5)
            {
                return new GameEvent
                {
                    Title = "Stability Crisis",
                    Description =
                        "Your people are losing faith in your leadership! Restore order quickly.",
                    Type = EventType.Crisis,
                    Cycle = gameState.CurrentCycle,
                    AffectedFactions = [gameState.PlayerFactionId],
                    Effects = new() { { StatKey.Population, -2 } },
                };
            }
            if (player.Stability < 20)
            {
                return new GameEvent
                {
                    Title = "Faction on the Brink!",
                    Description = "Your people are losing hope. Desperate measures are needed.",
                    Type = EventType.Crisis,
                    Cycle = gameState.CurrentCycle,
                    AffectedFactions = [gameState.PlayerFactionId],
                    Effects = new() { { StatKey.Stability, -10 }, { StatKey.Population, -5 } },
                    BlockedActions =
                    [
                        PlayerActionType.Develop_Infrastructure,
                        PlayerActionType.Economic_Tech,
                    ],
                };
            }
        }
        // Default major crisis
        return new GameEvent
        {
            Title = "Major Crisis",
            Description =
                "A major crisis shakes your faction to its core, testing your leadership.",
            Type = EventType.Crisis,
            Cycle = gameState.CurrentCycle,
            AffectedFactions = [gameState.PlayerFactionId],
            Effects = new() { { StatKey.Stability, -7 }, { StatKey.Resources, -5 } },
        };
    }

    private static GameEvent GenerateAncientTechEvent(GameState gameState)
    {
        // Rare beneficial event, can unlock Ancient Studies if previously blocked
        // Remove Ancient_Studies from BlockedActions if present (unblock for next turn)
        gameState.BlockedActions.Remove(PlayerActionType.Ancient_Studies);
        return new GameEvent
        {
            Title = "Ancient Technology Unearthed",
            Description = "You unearth powerful ancient technology, offering new possibilities.",
            Type = EventType.Discovery,
            Cycle = gameState.CurrentCycle,
            AffectedFactions = [gameState.PlayerFactionId],
            Effects = new() { { StatKey.Technology, 15 }, { StatKey.Stability, 5 } },
            BlockedActions = [],
        };
    }
}

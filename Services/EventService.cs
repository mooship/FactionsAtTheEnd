using CommunityToolkit.Diagnostics;
using FactionsAtTheEnd.Enums;
using FactionsAtTheEnd.Extensions;
using FactionsAtTheEnd.Interfaces;
using FactionsAtTheEnd.Models;
using static FactionsAtTheEnd.UI.ChoiceEventTemplates;
using static FactionsAtTheEnd.UI.EventTemplates;
using static FactionsAtTheEnd.UI.NewsTemplates;

namespace FactionsAtTheEnd.Services;

/// <summary>
/// Handles all event generation.
/// </summary>
public class EventService : IEventService
{
    /// <summary>
    /// Generates the initial set of events when a new game starts.
    /// This typically includes a major crisis event to kick off the narrative.
    /// </summary>
    /// <param name="gameState">The current game state.</param>
    /// <returns>A list of initial game events.</returns>
    public static List<GameEvent> GenerateInitialEvents(GameState gameState)
    {
        Guard.IsNotNull(gameState, nameof(gameState));

        var initialEvents = new List<GameEvent>
        {
            new()
            {
                Title = InitialCrisisTitle,
                Description = InitialCrisisDescription,
                Type = EventType.Crisis,
                Cycle = gameState.CurrentCycle,
                Effects = new Dictionary<StatKey, int>
                {
                    { StatKey.Stability, -20 },
                    { StatKey.Reputation, -10 },
                },
            },
        };
        return initialEvents;
    }

    public static List<GameEvent> GenerateRandomEvents(GameState gameState)
    {
        Guard.IsNotNull(gameState, nameof(gameState));
        var events = new List<GameEvent>();
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
                        Effects = new() { { StatKey.Stability, -5 }, { StatKey.Resources, -3 } },
                        BlockedActions = [kvp.Key],
                    }
                );
            }
        }

        int baseChance = 40;
        int rep = gameState.PlayerFaction.Reputation;
        int positiveBonus = rep > 0 ? rep / 20 * 5 : 0;
        int negativeBonus = rep < 0 ? Math.Abs(rep) / 20 * 5 : 0;
        positiveBonus = Math.Min(positiveBonus, 25);
        negativeBonus = Math.Min(negativeBonus, 25);
        int eventRoll = Random.Shared.Next(1, 101);
        bool forcePositive = eventRoll <= (baseChance + positiveBonus);
        bool forceNegative = eventRoll > (100 - negativeBonus);

        if (forcePositive || forceNegative || eventRoll <= baseChance)
        {
            var eventType = GetRandomEventType(gameState, forcePositive, forceNegative);
            var gameEvent = GenerateEventByType(eventType, gameState);
            Guard.IsNotNull(gameEvent);
            events.Add(gameEvent);
        }

        if (gameState.PlayerFaction.Stability > 75 && Random.Shared.Next(1, 101) <= 20)
        {
            events.Add(
                new GameEvent
                {
                    Title = ProsperityWaveTitle,
                    Description = ProsperityWaveDescription,
                    Type = EventType.Economic,
                    Cycle = gameState.CurrentCycle,
                    Effects = new() { { StatKey.Resources, 2 }, { StatKey.Influence, 1 } },
                }
            );
        }

        if (gameState.GalacticStability <= 20 && Random.Shared.Next(1, 101) <= 30)
        {
            events.Add(GenerateCrisisEvent(gameState));
        }

        if (gameState.AncientTechDiscovery >= 70 && Random.Shared.Next(1, 101) <= 25)
        {
            events.Add(GenerateAncientTechEvent(gameState));
        }

        if (Random.Shared.Next(1, 101) <= 10)
        {
            events.Add(
                new GameEvent
                {
                    Title = DiplomaticOvertureTitle,
                    Description = DiplomaticOvertureDescription,
                    Type = EventType.Military,
                    Cycle = gameState.CurrentCycle,
                    Parameters = new Dictionary<string, object> { { "Choice", "AllianceOffer" } },
                    Effects = new Dictionary<StatKey, int> { { StatKey.Influence, 5 } },
                    BlockedActions = [],
                }
            );
        }

        return events;
    }

    public async Task<List<GameEvent>> GenerateRandomEventsAsync(GameState gameState)
    {
        Guard.IsNotNull(gameState, nameof(gameState));
        if (Random.Shared.Next(1, 101) <= 10)
        {
            var choiceGenerators = new Func<GameState, GameEvent>[]
            {
                GenerateChoiceEvent,
                GenerateMilitaryChoiceEvent,
                GenerateEconomicChoiceEvent,
                GenerateTechnologicalChoiceEvent,
                GenerateDiscoveryChoiceEvent,
                GenerateNaturalChoiceEvent,
                GenerateEspionageChoiceEvent,
                GenerateReputationChoiceEvent,
            };
            var selected = choiceGenerators[Random.Shared.Next(choiceGenerators.Length)];
            return [selected(gameState)];
        }
        return await Task.Run(() => GenerateRandomEvents(gameState));
    }

    private static EventType GetRandomEventType(
        GameState gameState,
        bool forcePositive = false,
        bool forceNegative = false
    )
    {
        Guard.IsNotNull(gameState, nameof(gameState));

        var eventTypes = new[]
        {
            EventType.Military,
            EventType.Economic,
            EventType.Technological,
            EventType.Discovery,
            EventType.Natural,
        };

        if (forceNegative || (gameState.GalacticStability < 30 && Random.Shared.Next(1, 101) <= 30))
        {
            return EventType.Crisis;
        }
        if (forcePositive)
        {
            return Random.Shared.Next(2) == 0 ? EventType.Discovery : EventType.Technological;
        }
        return eventTypes[Random.Shared.Next(eventTypes.Length)];
    }

    private static GameEvent? GenerateEventByType(EventType eventType, GameState gameState)
    {
        Guard.IsNotNull(gameState, nameof(gameState));
        Guard.IsTrue(Enum.IsDefined(typeof(EventType), eventType), nameof(eventType));

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
        var player = gameState.PlayerFaction;
        var index = Random.Shared.Next(10);
        if (player?.Type == FactionType.MilitaryJunta && Random.Shared.Next(1, 101) <= 20)
        {
            return new GameEvent
            {
                Title = VeteranParadeTitle,
                Description = VeteranParadeDescription,
                Type = EventType.Military,
                Cycle = gameState.CurrentCycle,
                Effects = new() { { StatKey.Military, 8 }, { StatKey.Stability, 4 } },
            };
        }
        if (player?.Type == FactionType.PirateAlliance && Random.Shared.Next(1, 101) <= 20)
        {
            return new GameEvent
            {
                Title = SuccessfulRaidTitle,
                Description = SuccessfulRaidDescription,
                Type = EventType.Military,
                Cycle = gameState.CurrentCycle,
                Effects = new()
                {
                    { StatKey.Military, 5 },
                    { StatKey.Resources, 8 },
                    { StatKey.Stability, 2 },
                },
            };
        }
        if (player?.Type == FactionType.RebellionCell && Random.Shared.Next(1, 101) <= 20)
        {
            return new GameEvent
            {
                Title = UprisingIgnitedTitle,
                Description = UprisingIgnitedDescription,
                Type = EventType.Military,
                Cycle = gameState.CurrentCycle,
                Effects = new()
                {
                    { StatKey.Military, 3 },
                    { StatKey.Influence, 4 },
                    { StatKey.Stability, 2 },
                },
            };
        }
        return index switch
        {
            0 => new GameEvent
            {
                Title = RaidersAttackTitle,
                Description = RaidersAttackDescription,
                Type = EventType.Military,
                Cycle = gameState.CurrentCycle,
                Effects = new() { { StatKey.Military, -5 }, { StatKey.Resources, -10 } },
                BlockedActions = [PlayerActionType.ExploitResources],
            },
            1 => new GameEvent
            {
                Title = InternalMutinyTitle,
                Description = InternalMutinyDescription,
                Type = EventType.Military,
                Cycle = gameState.CurrentCycle,
                Effects = new() { { StatKey.Military, -8 }, { StatKey.Stability, -5 } },
                BlockedActions = [PlayerActionType.RecruitTroops],
            },
            2 => new GameEvent
            {
                Title = MercenaryTroubleTitle,
                Description = MercenaryTroubleDescription,
                Type = EventType.Military,
                Cycle = gameState.CurrentCycle,
                Effects = new() { { StatKey.Resources, -7 }, { StatKey.Military, -3 } },
            },
            3 => new GameEvent
            {
                Title = EliteTrainingTitle,
                Description = EliteTrainingDescription,
                Type = EventType.Military,
                Cycle = gameState.CurrentCycle,
                Effects = new() { { StatKey.Military, 10 }, { StatKey.Stability, 2 } },
            },
            4 => new GameEvent
            {
                Title = BorderSkirmishTitle,
                Description = BorderSkirmishDescription,
                Type = EventType.Military,
                Cycle = gameState.CurrentCycle,
                Effects = new() { { StatKey.Military, -3 }, { StatKey.Stability, -2 } },
            },
            5 => new GameEvent
            {
                Title = VeteranRecruitsTitle,
                Description = VeteranRecruitsDescription,
                Type = EventType.Military,
                Cycle = gameState.CurrentCycle,
                Effects = new() { { StatKey.Military, 7 }, { StatKey.Population, 2 } },
            },
            6 => new GameEvent
            {
                Title = PeacefulGarrisonTitle,
                Description = PeacefulGarrisonDescription,
                Type = EventType.Military,
                Cycle = gameState.CurrentCycle,
                Effects = new() { { StatKey.Military, 3 }, { StatKey.Stability, 2 } },
            },
            7 => new GameEvent
            {
                Title = TrainingAccidentTitle,
                Description = TrainingAccidentDescription,
                Type = EventType.Military,
                Cycle = gameState.CurrentCycle,
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
        var player = gameState.PlayerFaction;
        var index = Random.Shared.Next(8);
        if (player?.Type == FactionType.CorporateCouncil && Random.Shared.Next(1, 101) <= 20)
        {
            return new GameEvent
            {
                Title = MarketBoomTitle,
                Description = MarketBoomDescription,
                Type = EventType.Economic,
                Cycle = gameState.CurrentCycle,
                Effects = new() { { StatKey.Resources, 12 }, { StatKey.Influence, 4 } },
            };
        }
        if (player?.Type == FactionType.ReligiousOrder && Random.Shared.Next(1, 101) <= 20)
        {
            return new GameEvent
            {
                Title = TithesOfferingsTitle,
                Description = TithesOfferingsDescription,
                Type = EventType.Economic,
                Cycle = gameState.CurrentCycle,
                Effects = new() { { StatKey.Resources, 8 }, { StatKey.Stability, 3 } },
            };
        }
        return index switch
        {
            0 => new GameEvent
            {
                Title = ResourceShortageTitle,
                Description = ResourceShortageDescription,
                Type = EventType.Economic,
                Cycle = gameState.CurrentCycle,
                Effects = new() { { StatKey.Resources, -12 } },
                BlockedActions = [PlayerActionType.ExploitResources],
            },
            1 => new GameEvent
            {
                Title = MarketInstabilityTitle,
                Description = MarketInstabilityDescription,
                Type = EventType.Economic,
                Cycle = gameState.CurrentCycle,
                Effects = new() { { StatKey.Resources, -8 }, { StatKey.Stability, -3 } },
            },
            2 => new GameEvent
            {
                Title = BlackMarketSurgeTitle,
                Description = BlackMarketSurgeDescription,
                Type = EventType.Economic,
                Cycle = gameState.CurrentCycle,
                Effects = new() { { StatKey.Influence, -5 }, { StatKey.Resources, -5 } },
            },
            3 => new GameEvent
            {
                Title = TradeConvoyArrivesTitle,
                Description = TradeConvoyArrivesDescription,
                Type = EventType.Economic,
                Cycle = gameState.CurrentCycle,
                Effects = new() { { StatKey.Resources, 15 }, { StatKey.Stability, 2 } },
            },
            4 => new GameEvent
            {
                Title = SmugglingRingBustedTitle,
                Description = SmugglingRingBustedDescription,
                Type = EventType.Economic,
                Cycle = gameState.CurrentCycle,
                Effects = new() { { StatKey.Resources, 8 }, { StatKey.Influence, 3 } },
            },
            5 => new GameEvent
            {
                Title = ResourceWindfallTitle,
                Description = ResourceWindfallDescription,
                Type = EventType.Economic,
                Cycle = gameState.CurrentCycle,
                Effects = new() { { StatKey.Resources, 12 } },
            },
            6 => new GameEvent
            {
                Title = EfficientLogisticsTitle,
                Description = EfficientLogisticsDescription,
                Type = EventType.Economic,
                Cycle = gameState.CurrentCycle,
                Effects = new() { { StatKey.Resources, 5 } },
            },
            7 => new GameEvent
            {
                Title = CharityDriveTitle,
                Description = CharityDriveDescription,
                Type = EventType.Economic,
                Cycle = gameState.CurrentCycle,
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
        var player = gameState.PlayerFaction;
        var index = Random.Shared.Next(8);
        if (player?.Type == FactionType.TechnocraticUnion && Random.Shared.Next(1, 101) <= 20)
        {
            return new GameEvent
            {
                Title = BreakthroughAlgorithmTitle,
                Description = BreakthroughAlgorithmDescription,
                Type = EventType.Technological,
                Cycle = gameState.CurrentCycle,
                Effects = new() { { StatKey.Technology, 15 }, { StatKey.Stability, 3 } },
            };
        }
        if (player?.Type == FactionType.ImperialRemnant && Random.Shared.Next(1, 101) <= 20)
        {
            return new GameEvent
            {
                Title = RecoveredImperialDatabaseTitle,
                Description = RecoveredImperialDatabaseDescription,
                Type = EventType.Technological,
                Cycle = gameState.CurrentCycle,
                Effects = new() { { StatKey.Technology, 10 }, { StatKey.Influence, 5 } },
            };
        }
        return index switch
        {
            0 => new GameEvent
            {
                Title = TechBreakdownTitle,
                Description = TechBreakdownDescription,
                Type = EventType.Technological,
                Cycle = gameState.CurrentCycle,
                Effects = new() { { StatKey.Technology, -7 }, { StatKey.Stability, -3 } },
                BlockedActions = [PlayerActionType.MilitaryTech],
            },
            1 => new GameEvent
            {
                Title = ResearchBreakthroughTitle,
                Description = ResearchBreakthroughDescription,
                Type = EventType.Technological,
                Cycle = gameState.CurrentCycle,
                Effects = new() { { StatKey.Technology, 10 }, { StatKey.Resources, -3 } },
            },
            2 => new GameEvent
            {
                Title = "System Malfunction",
                Description = "A critical system malfunction disrupts your research efforts.",
                Type = EventType.Technological,
                Cycle = gameState.CurrentCycle,
                Effects = new() { { StatKey.Technology, -5 }, { StatKey.Military, -2 } },
            },
            3 => new GameEvent
            {
                Title = UnexpectedInnovationTitle,
                Description = UnexpectedInnovationDescription,
                Type = EventType.Technological,
                Cycle = gameState.CurrentCycle,
                Effects = new() { { StatKey.Technology, 7 }, { StatKey.Stability, 4 } },
            },
            4 => new GameEvent
            {
                Title = PrototypeSuccessTitle,
                Description = PrototypeSuccessDescription,
                Type = EventType.Technological,
                Cycle = gameState.CurrentCycle,
                Effects = new() { { StatKey.Technology, 8 }, { StatKey.Resources, -2 } },
            },
            5 => new GameEvent
            {
                Title = EquipmentTheftTitle,
                Description = EquipmentTheftDescription,
                Type = EventType.Technological,
                Cycle = gameState.CurrentCycle,
                Effects = new() { { StatKey.Technology, -6 }, { StatKey.Resources, -4 } },
            },
            6 => new GameEvent
            {
                Title = TechFestivalTitle,
                Description = TechFestivalDescription,
                Type = EventType.Technological,
                Cycle = gameState.CurrentCycle,
                Effects = new() { { StatKey.Technology, 5 }, { StatKey.Influence, 2 } },
            },
            7 => new GameEvent
            {
                Title = FailedExperimentTitle,
                Description = FailedExperimentDescription,
                Type = EventType.Technological,
                Cycle = gameState.CurrentCycle,
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
        var player = gameState.PlayerFaction;
        var index = Random.Shared.Next(8);
        if (player?.Type == FactionType.AncientAwakened && Random.Shared.Next(1, 101) <= 20)
        {
            return new GameEvent
            {
                Title = AncientMemoryStirredTitle,
                Description = AncientMemoryStirredDescription,
                Type = EventType.Discovery,
                Cycle = gameState.CurrentCycle,
                Effects = new() { { StatKey.Technology, 10 }, { StatKey.Stability, 5 } },
            };
        }
        if (player?.Type == FactionType.AncientAwakened && Random.Shared.Next(1, 101) <= 5)
        {
            return new GameEvent
            {
                Title = EchoesOfTheFirstEmpireTitle,
                Description = EchoesOfTheFirstEmpireDescription,
                Type = EventType.Discovery,
                Cycle = gameState.CurrentCycle,
                Effects = new() { { StatKey.Technology, 20 }, { StatKey.Influence, 10 } },
            };
        }
        return index switch
        {
            0 => new GameEvent
            {
                Title = AncientRuinsFoundTitle,
                Description = AncientRuinsFoundDescription,
                Type = EventType.Discovery,
                Cycle = gameState.CurrentCycle,
                Effects = new() { { StatKey.Technology, 5 }, { StatKey.Resources, 3 } },
            },
            1 => new GameEvent
            {
                Title = LostDataRecoveredTitle,
                Description = LostDataRecoveredDescription,
                Type = EventType.Discovery,
                Cycle = gameState.CurrentCycle,
                Effects = new() { { StatKey.Technology, 3 }, { StatKey.Stability, 2 } },
            },
            2 => new GameEvent
            {
                Title = MysteriousSignalDetectedTitle,
                Description = MysteriousSignalDetectedDescription,
                Type = EventType.Discovery,
                Cycle = gameState.CurrentCycle,
                Effects = new() { { StatKey.Influence, 4 } },
            },
            3 => new GameEvent
            {
                Title = DangerousRelicActivatedTitle,
                Description = DangerousRelicActivatedDescription,
                Type = EventType.Discovery,
                Cycle = gameState.CurrentCycle,
                Effects = new() { { StatKey.Stability, -6 }, { StatKey.Technology, -2 } },
                BlockedActions = [PlayerActionType.AncientStudies],
            },
            4 => new GameEvent
            {
                Title = AlienArtifactTitle,
                Description = AlienArtifactDescription,
                Type = EventType.Discovery,
                Cycle = gameState.CurrentCycle,
                Effects = new() { { StatKey.Influence, 6 }, { StatKey.Technology, 2 } },
            },
            5 => new GameEvent
            {
                Title = ForgottenCacheTitle,
                Description = ForgottenCacheDescription,
                Type = EventType.Discovery,
                Cycle = gameState.CurrentCycle,
                Effects = new() { { StatKey.Resources, 7 }, { StatKey.Stability, 1 } },
            },
            6 => new GameEvent
            {
                Title = CulturalExchangeTitle,
                Description = CulturalExchangeDescription,
                Type = EventType.Discovery,
                Cycle = gameState.CurrentCycle,
                Effects = new() { { StatKey.Influence, 4 }, { StatKey.Stability, 2 } },
            },
            7 => new GameEvent
            {
                Title = FalseLeadTitle,
                Description = FalseLeadDescription,
                Type = EventType.Discovery,
                Cycle = gameState.CurrentCycle,
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
        var player = gameState.PlayerFaction;
        var index = Random.Shared.Next(8);
        if (player?.Type == FactionType.ReligiousOrder && Random.Shared.Next(1, 101) <= 20)
        {
            return new GameEvent
            {
                Title = PilgrimageMiracleTitle,
                Description = PilgrimageMiracleDescription,
                Type = EventType.Natural,
                Cycle = gameState.CurrentCycle,
                Effects = new() { { StatKey.Stability, 7 }, { StatKey.Population, 3 } },
            };
        }
        return index switch
        {
            0 => new GameEvent
            {
                Title = SolarFlareTitle,
                Description = SolarFlareDescription,
                Type = EventType.Natural,
                Cycle = gameState.CurrentCycle,
                Effects = new() { { StatKey.Resources, -6 }, { StatKey.Technology, -3 } },
            },
            1 => new GameEvent
            {
                Title = MeteorShowerTitle,
                Description = MeteorShowerDescription,
                Type = EventType.Natural,
                Cycle = gameState.CurrentCycle,
                Effects = new() { { StatKey.Population, -4 }, { StatKey.Stability, -2 } },
            },
            2 => new GameEvent
            {
                Title = PlagueOutbreakTitle,
                Description = PlagueOutbreakDescription,
                Type = EventType.Natural,
                Cycle = gameState.CurrentCycle,
                Effects = new() { { StatKey.Population, -8 }, { StatKey.Stability, -4 } },
                BlockedActions = [PlayerActionType.DevelopInfrastructure],
            },
            3 => new GameEvent
            {
                Title = BountifulHarvestTitle,
                Description = BountifulHarvestDescription,
                Type = EventType.Natural,
                Cycle = gameState.CurrentCycle,
                Effects = new() { { StatKey.Resources, 10 }, { StatKey.Stability, 3 } },
            },
            4 => new GameEvent
            {
                Title = EarthquakeTitle,
                Description = EarthquakeDescription,
                Type = EventType.Natural,
                Cycle = gameState.CurrentCycle,
                Effects = new() { { StatKey.Population, -6 }, { StatKey.Resources, -5 } },
            },
            5 => new GameEvent
            {
                Title = MildSeasonTitle,
                Description = MildSeasonDescription,
                Type = EventType.Natural,
                Cycle = gameState.CurrentCycle,
                Effects = new() { { StatKey.Stability, 4 }, { StatKey.Population, 2 } },
            },
            6 => new GameEvent
            {
                Title = GentleRainsTitle,
                Description = GentleRainsDescription,
                Type = EventType.Natural,
                Cycle = gameState.CurrentCycle,
                Effects = new() { { StatKey.Resources, 6 }, { StatKey.Population, 2 } },
            },
            7 => new GameEvent
            {
                Title = MinorFloodTitle,
                Description = MinorFloodDescription,
                Type = EventType.Natural,
                Cycle = gameState.CurrentCycle,
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
        var player = gameState.PlayerFaction;
        Guard.IsNotNull(player);
        if (player.Population <= 5)
        {
            return new GameEvent
            {
                Title = PopulationCrisisTitle,
                Description = PopulationCrisisDescription,
                Type = EventType.Crisis,
                Cycle = gameState.CurrentCycle,
                Effects = new() { { StatKey.Stability, -3 } },
            };
        }
        if (player.Resources <= 5)
        {
            return new GameEvent
            {
                Title = ResourceCrisisTitle,
                Description = ResourceCrisisDescription,
                Type = EventType.Crisis,
                Cycle = gameState.CurrentCycle,
                Effects = new() { { StatKey.Stability, -2 } },
            };
        }
        if (player.Stability <= 5)
        {
            return new GameEvent
            {
                Title = StabilityCrisisTitle,
                Description = StabilityCrisisDescription,
                Type = EventType.Crisis,
                Cycle = gameState.CurrentCycle,
                Effects = new() { { StatKey.Population, -2 } },
            };
        }
        if (player.Stability < 20)
        {
            return new GameEvent
            {
                Title = FactionOnTheBrinkTitle,
                Description = FactionOnTheBrinkDescription,
                Type = EventType.Crisis,
                Cycle = gameState.CurrentCycle,
                Effects = new() { { StatKey.Stability, -10 }, { StatKey.Population, -5 } },
                BlockedActions =
                [
                    PlayerActionType.DevelopInfrastructure,
                    PlayerActionType.EconomicTech,
                ],
            };
        }
        return new GameEvent
        {
            Title = MajorCrisisTitle,
            Description = MajorCrisisDescription,
            Type = EventType.Crisis,
            Cycle = gameState.CurrentCycle,
            Effects = new() { { StatKey.Stability, -7 }, { StatKey.Resources, -5 } },
        };
    }

    private static GameEvent GenerateAncientTechEvent(GameState gameState)
    {
        gameState.BlockedActions.Remove(PlayerActionType.AncientStudies);
        return new GameEvent
        {
            Title = AncientTechnologyUnearthedTitle,
            Description = AncientTechnologyUnearthedDescription,
            Type = EventType.Discovery,
            Cycle = gameState.CurrentCycle,
            Effects = new() { { StatKey.Technology, 15 }, { StatKey.Stability, 5 } },
            BlockedActions = [],
        };
    }

    /// <summary>
    /// Generates galactic news headlines based on recent player actions, major events, and world state.
    /// </summary>
    public List<string> GenerateGalacticNews(GameState gameState, List<GameEvent> recentEvents)
    {
        Guard.IsNotNull(gameState, nameof(gameState));
        Guard.IsNotNull(recentEvents, nameof(recentEvents));
        var news = new List<string>();
        var player = gameState.PlayerFaction;

        foreach (var gameEvent in recentEvents)
        {
            switch (gameEvent.Type)
            {
                case EventType.Crisis:
                    news.Add(string.Format(CrisisHeadline, gameEvent.Title, player.Name));
                    break;
                case EventType.Military:
                    news.Add(string.Format(MilitaryHeadline, gameEvent.Title, player.Name));
                    break;
                case EventType.Economic:
                    news.Add(string.Format(EconomicHeadline, gameEvent.Title, player.Name));
                    break;
                case EventType.Technological:
                    news.Add(string.Format(TechHeadline, gameEvent.Title, player.Name));
                    break;
                case EventType.Discovery:
                    news.Add(string.Format(DiscoveryHeadline, gameEvent.Title, player.Name));
                    break;
                case EventType.Natural:
                    news.Add(string.Format(NaturalHeadline, gameEvent.Title, player.Name));
                    break;
            }
        }

        if (player.Reputation >= 80)
        {
            news.Add(string.Format(LegendaryReputation, player.Name));
        }
        else if (player.Reputation >= 40)
        {
            news.Add(string.Format(RisingStarReputation, player.Name));
        }
        else if (player.Reputation <= -80)
        {
            news.Add($"Infamous: {player.Name} is feared across the galaxy!");
        }
        else if (player.Reputation <= -40)
        {
            news.Add($"Feared: {player.Name}'s reputation strikes terror in many.");
        }
        else if (player.Reputation <= -10)
        {
            news.Add($"Notorious: {player.Name} is gaining a dark reputation.");
        }

        if (gameState.GalacticStability < 20)
        {
            news.Add(TurmoilNews);
        }
        if (gameState.GateNetworkIntegrity < 30)
        {
            news.Add(GateFailingNews);
        }
        if (gameState.AncientTechDiscovery > 70)
        {
            news.Add(AncientTechNews);
        }

        return [.. news.Take(5)];
    }

    public static GameEvent GenerateChoiceEvent(GameState gameState)
    {
        return new GameEvent
        {
            Title = CrisisChoiceTitle,
            Description = CrisisChoiceDescription,
            Type = EventType.Crisis,
            Cycle = gameState.CurrentCycle,
            Choices =
            [
                new EventChoice
                {
                    Description = CrisisChoiceAidDescription,
                    Effects = new() { { StatKey.Resources, -10 }, { StatKey.Stability, +5 } },
                },
                new EventChoice
                {
                    Description = CrisisChoiceIgnoreDescription,
                    Effects = new() { { StatKey.Stability, -5 } },
                },
            ],
        };
    }

    public static GameEvent GenerateMilitaryChoiceEvent(GameState gameState)
    {
        return new GameEvent
        {
            Title = MilitaryChoiceTitle,
            Description = MilitaryChoiceDescription,
            Type = EventType.Military,
            Cycle = gameState.CurrentCycle,
            Choices =
            [
                new EventChoice
                {
                    Description = MilitaryChoiceHireDescription,
                    Effects = new() { { StatKey.Resources, -12 }, { StatKey.Military, +8 } },
                },
                new EventChoice
                {
                    Description = MilitaryChoiceRefuseDescription,
                    Effects = new() { { StatKey.Stability, -4 }, { StatKey.Resources, +2 } },
                    BlockedActions = [PlayerActionType.RecruitTroops],
                },
            ],
        };
    }

    public static GameEvent GenerateEconomicChoiceEvent(GameState gameState)
    {
        return new GameEvent
        {
            Title = EconomicChoiceTitle,
            Description = EconomicChoiceDescription,
            Type = EventType.Economic,
            Cycle = gameState.CurrentCycle,
            Choices =
            [
                new EventChoice
                {
                    Description = EconomicChoiceAcceptDescription,
                    Effects = new() { { StatKey.Resources, +10 }, { StatKey.Influence, -5 } },
                },
                new EventChoice
                {
                    Description = EconomicChoiceRefuseDescription,
                    Effects = new() { { StatKey.Stability, +3 } },
                },
            ],
        };
    }

    public static GameEvent GenerateTechnologicalChoiceEvent(GameState gameState)
    {
        return new GameEvent
        {
            Title = TechnologicalChoiceTitle,
            Description = TechnologicalChoiceDescription,
            Type = EventType.Technological,
            Cycle = gameState.CurrentCycle,
            Choices =
            [
                new EventChoice
                {
                    Description = TechnologicalChoiceApproveDescription,
                    Effects = new() { { StatKey.Technology, +12 }, { StatKey.Stability, -6 } },
                },
                new EventChoice
                {
                    Description = TechnologicalChoiceRejectDescription,
                    Effects = new() { { StatKey.Technology, +2 } },
                },
            ],
        };
    }

    public static GameEvent GenerateDiscoveryChoiceEvent(GameState gameState)
    {
        return new GameEvent
        {
            Title = DiscoveryChoiceTitle,
            Description = DiscoveryChoiceDescription,
            Type = EventType.Discovery,
            Cycle = gameState.CurrentCycle,
            Choices =
            [
                new EventChoice
                {
                    Description = DiscoveryChoiceStudyDescription,
                    Effects = new() { { StatKey.Technology, +8 }, { StatKey.Stability, -3 } },
                },
                new EventChoice
                {
                    Description = DiscoveryChoiceSellDescription,
                    Effects = new() { { StatKey.Resources, +10 } },
                },
            ],
        };
    }

    public static GameEvent GenerateNaturalChoiceEvent(GameState gameState)
    {
        return new GameEvent
        {
            Title = NaturalChoiceTitle,
            Description = NaturalChoiceDescription,
            Type = EventType.Natural,
            Cycle = gameState.CurrentCycle,
            Choices =
            [
                new EventChoice
                {
                    Description = NaturalChoiceAidDescription,
                    Effects = new() { { StatKey.Resources, -8 }, { StatKey.Stability, +5 } },
                },
                new EventChoice
                {
                    Description = NaturalChoiceFocusDescription,
                    Effects = new() { { StatKey.Stability, -4 }, { StatKey.Resources, +2 } },
                },
            ],
        };
    }

    public static GameEvent GenerateEspionageChoiceEvent(GameState gameState)
    {
        return new GameEvent
        {
            Title = EspionageChoiceTitle,
            Description = EspionageChoiceDescription,
            Type = EventType.Crisis,
            Cycle = gameState.CurrentCycle,
            Choices =
            [
                new EventChoice
                {
                    Description = EspionageChoiceAttemptDescription,
                    Effects = new() { { StatKey.Technology, +6 }, { StatKey.Stability, -4 } },
                },
                new EventChoice
                {
                    Description = EspionageChoiceSafeDescription,
                    Effects = new() { { StatKey.Influence, +3 } },
                },
            ],
        };
    }

    public static GameEvent GenerateReputationChoiceEvent(GameState gameState)
    {
        return new GameEvent
        {
            Title = ReputationChoiceTitle,
            Description = ReputationChoiceDescription,
            Type = EventType.Crisis,
            Cycle = gameState.CurrentCycle,
            Choices =
            [
                new EventChoice
                {
                    Description = ReputationChoiceCoverupDescription,
                    Effects = new() { { StatKey.Resources, -7 } },
                },
                new EventChoice
                {
                    Description = ReputationChoiceAcceptDescription,
                    Effects = new() { { StatKey.Reputation, -6 }, { StatKey.Stability, +4 } },
                },
            ],
        };
    }
}

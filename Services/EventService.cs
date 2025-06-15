using FactionsAtTheEnd.Interfaces;
using FactionsAtTheEnd.Models;
using FactionsAtTheEnd.UI;
using static FactionsAtTheEnd.UI.EventTemplates;
using static FactionsAtTheEnd.UI.NewsTemplates;

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
                Title = CollapseTitle,
                Description = CollapseDescription,
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

        if (Random.Shared.Next(1, 101) <= 10)
        {
            events.Add(
                new GameEvent
                {
                    Title = DiplomaticOvertureTitle,
                    Description = DiplomaticOvertureDescription,
                    Type = EventType.Military,
                    Cycle = gameState.CurrentCycle,
                    AffectedFactions = [gameState.PlayerFactionId],
                    Parameters = new Dictionary<string, object> { { "Choice", "AllianceOffer" } },
                    Effects = new Dictionary<UI.StatKey, int> { { StatKey.Influence, 5 } },
                    BlockedActions = [],
                }
            );
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
                Title = VeteranParadeTitle,
                Description = VeteranParadeDescription,
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
                Title = SuccessfulRaidTitle,
                Description = SuccessfulRaidDescription,
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
                Title = SabotageSuccessTitle,
                Description = SabotageSuccessDescription,
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
                Title = RaidersAttackTitle,
                Description = RaidersAttackDescription,
                Type = EventType.Military,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Military, -5 }, { StatKey.Resources, -10 } },
                BlockedActions = [PlayerActionType.Exploit_Resources],
            },
            1 => new GameEvent
            {
                Title = InternalMutinyTitle,
                Description = InternalMutinyDescription,
                Type = EventType.Military,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Military, -8 }, { StatKey.Stability, -5 } },
                BlockedActions = [PlayerActionType.Recruit_Troops],
            },
            2 => new GameEvent
            {
                Title = MercenaryTroubleTitle,
                Description = MercenaryTroubleDescription,
                Type = EventType.Military,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Resources, -7 }, { StatKey.Military, -3 } },
            },
            3 => new GameEvent
            {
                Title = EliteTrainingTitle,
                Description = EliteTrainingDescription,
                Type = EventType.Military,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Military, 10 }, { StatKey.Stability, 2 } },
            },
            4 => new GameEvent
            {
                Title = BorderSkirmishTitle,
                Description = BorderSkirmishDescription,
                Type = EventType.Military,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Military, -3 }, { StatKey.Stability, -2 } },
            },
            5 => new GameEvent
            {
                Title = VeteranRecruitsTitle,
                Description = VeteranRecruitsDescription,
                Type = EventType.Military,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Military, 7 }, { StatKey.Population, 2 } },
            },
            6 => new GameEvent
            {
                Title = PeacefulGarrisonTitle,
                Description = PeacefulGarrisonDescription,
                Type = EventType.Military,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Military, 3 }, { StatKey.Stability, 2 } },
            },
            7 => new GameEvent
            {
                Title = TrainingAccidentTitle,
                Description = TrainingAccidentDescription,
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
                Title = MarketBoomTitle,
                Description = MarketBoomDescription,
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
                Title = TithesOfferingsTitle,
                Description = TithesOfferingsDescription,
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
                Title = ResourceShortageTitle,
                Description = ResourceShortageDescription,
                Type = EventType.Economic,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Resources, -12 } },
                BlockedActions = [PlayerActionType.Exploit_Resources],
            },
            1 => new GameEvent
            {
                Title = MarketInstabilityTitle,
                Description = MarketInstabilityDescription,
                Type = EventType.Economic,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Resources, -8 }, { StatKey.Stability, -3 } },
            },
            2 => new GameEvent
            {
                Title = BlackMarketSurgeTitle,
                Description = BlackMarketSurgeDescription,
                Type = EventType.Economic,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Influence, -5 }, { StatKey.Resources, -5 } },
            },
            3 => new GameEvent
            {
                Title = TradeConvoyArrivesTitle,
                Description = TradeConvoyArrivesDescription,
                Type = EventType.Economic,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Resources, 15 }, { StatKey.Stability, 2 } },
            },
            4 => new GameEvent
            {
                Title = SmugglingRingBustedTitle,
                Description = SmugglingRingBustedDescription,
                Type = EventType.Economic,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Resources, 8 }, { StatKey.Influence, 3 } },
            },
            5 => new GameEvent
            {
                Title = ResourceWindfallTitle,
                Description = ResourceWindfallDescription,
                Type = EventType.Economic,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Resources, 12 } },
            },
            6 => new GameEvent
            {
                Title = EfficientLogisticsTitle,
                Description = EfficientLogisticsDescription,
                Type = EventType.Economic,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Resources, 5 } },
            },
            7 => new GameEvent
            {
                Title = CharityDriveTitle,
                Description = CharityDriveDescription,
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
                Title = BreakthroughAlgorithmTitle,
                Description = BreakthroughAlgorithmDescription,
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
                Title = RecoveredImperialDatabaseTitle,
                Description = RecoveredImperialDatabaseDescription,
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
                Title = TechBreakdownTitle,
                Description = TechBreakdownDescription,
                Type = EventType.Technological,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Technology, -7 }, { StatKey.Stability, -3 } },
                BlockedActions = [PlayerActionType.Military_Tech],
            },
            1 => new GameEvent
            {
                Title = ResearchBreakthroughTitle,
                Description = ResearchBreakthroughDescription,
                Type = EventType.Technological,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Technology, 10 }, { StatKey.Resources, -3 } },
            },
            2 => new GameEvent
            {
                Title = SabotageAttemptTitle,
                Description = SabotageAttemptDescription,
                Type = EventType.Technological,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Technology, -5 }, { StatKey.Military, -2 } },
            },
            3 => new GameEvent
            {
                Title = UnexpectedInnovationTitle,
                Description = UnexpectedInnovationDescription,
                Type = EventType.Technological,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Technology, 7 }, { StatKey.Stability, 4 } },
            },
            4 => new GameEvent
            {
                Title = PrototypeSuccessTitle,
                Description = PrototypeSuccessDescription,
                Type = EventType.Technological,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Technology, 8 }, { StatKey.Resources, -2 } },
            },
            5 => new GameEvent
            {
                Title = EquipmentTheftTitle,
                Description = EquipmentTheftDescription,
                Type = EventType.Technological,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Technology, -6 }, { StatKey.Resources, -4 } },
            },
            6 => new GameEvent
            {
                Title = TechFestivalTitle,
                Description = TechFestivalDescription,
                Type = EventType.Technological,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Technology, 5 }, { StatKey.Influence, 2 } },
            },
            7 => new GameEvent
            {
                Title = FailedExperimentTitle,
                Description = FailedExperimentDescription,
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
                Title = AncientMemoryStirredTitle,
                Description = AncientMemoryStirredDescription,
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
                Title = EchoesOfTheFirstEmpireTitle,
                Description = EchoesOfTheFirstEmpireDescription,
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
                Title = AncientRuinsFoundTitle,
                Description = AncientRuinsFoundDescription,
                Type = EventType.Discovery,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Technology, 5 }, { StatKey.Resources, 3 } },
            },
            1 => new GameEvent
            {
                Title = LostDataRecoveredTitle,
                Description = LostDataRecoveredDescription,
                Type = EventType.Discovery,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Technology, 3 }, { StatKey.Stability, 2 } },
            },
            2 => new GameEvent
            {
                Title = MysteriousSignalDetectedTitle,
                Description = MysteriousSignalDetectedDescription,
                Type = EventType.Discovery,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Influence, 4 } },
            },
            3 => new GameEvent
            {
                Title = DangerousRelicActivatedTitle,
                Description = DangerousRelicActivatedDescription,
                Type = EventType.Discovery,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Stability, -6 }, { StatKey.Technology, -2 } },
                BlockedActions = [PlayerActionType.Ancient_Studies],
            },
            4 => new GameEvent
            {
                Title = AlienArtifactTitle,
                Description = AlienArtifactDescription,
                Type = EventType.Discovery,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Influence, 6 }, { StatKey.Technology, 2 } },
            },
            5 => new GameEvent
            {
                Title = ForgottenCacheTitle,
                Description = ForgottenCacheDescription,
                Type = EventType.Discovery,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Resources, 7 }, { StatKey.Stability, 1 } },
            },
            6 => new GameEvent
            {
                Title = CulturalExchangeTitle,
                Description = CulturalExchangeDescription,
                Type = EventType.Discovery,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Influence, 4 }, { StatKey.Stability, 2 } },
            },
            7 => new GameEvent
            {
                Title = FalseLeadTitle,
                Description = FalseLeadDescription,
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
                Title = PilgrimageMiracleTitle,
                Description = PilgrimageMiracleDescription,
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
                Title = SolarFlareTitle,
                Description = SolarFlareDescription,
                Type = EventType.Natural,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Resources, -6 }, { StatKey.Technology, -3 } },
            },
            1 => new GameEvent
            {
                Title = MeteorShowerTitle,
                Description = MeteorShowerDescription,
                Type = EventType.Natural,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Population, -4 }, { StatKey.Stability, -2 } },
            },
            2 => new GameEvent
            {
                Title = PlagueOutbreakTitle,
                Description = PlagueOutbreakDescription,
                Type = EventType.Natural,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Population, -8 }, { StatKey.Stability, -4 } },
                BlockedActions = [PlayerActionType.Develop_Infrastructure],
            },
            3 => new GameEvent
            {
                Title = BountifulHarvestTitle,
                Description = BountifulHarvestDescription,
                Type = EventType.Natural,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Resources, 10 }, { StatKey.Stability, 3 } },
            },
            4 => new GameEvent
            {
                Title = EarthquakeTitle,
                Description = EarthquakeDescription,
                Type = EventType.Natural,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Population, -6 }, { StatKey.Resources, -5 } },
            },
            5 => new GameEvent
            {
                Title = MildSeasonTitle,
                Description = MildSeasonDescription,
                Type = EventType.Natural,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Stability, 4 }, { StatKey.Population, 2 } },
            },
            6 => new GameEvent
            {
                Title = GentleRainsTitle,
                Description = GentleRainsDescription,
                Type = EventType.Natural,
                Cycle = gameState.CurrentCycle,
                AffectedFactions = [gameState.PlayerFactionId],
                Effects = new() { { StatKey.Resources, 6 }, { StatKey.Population, 2 } },
            },
            7 => new GameEvent
            {
                Title = MinorFloodTitle,
                Description = MinorFloodDescription,
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
                    Title = PopulationCrisisTitle,
                    Description = PopulationCrisisDescription,
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
                    Title = ResourceCrisisTitle,
                    Description = ResourceCrisisDescription,
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
                    Title = StabilityCrisisTitle,
                    Description = StabilityCrisisDescription,
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
                    Title = FactionOnTheBrinkTitle,
                    Description = FactionOnTheBrinkDescription,
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
            Title = MajorCrisisTitle,
            Description = MajorCrisisDescription,
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
            Title = AncientTechnologyUnearthedTitle,
            Description = AncientTechnologyUnearthedDescription,
            Type = EventType.Discovery,
            Cycle = gameState.CurrentCycle,
            AffectedFactions = [gameState.PlayerFactionId],
            Effects = new() { { StatKey.Technology, 15 }, { StatKey.Stability, 5 } },
            BlockedActions = [],
        };
    }

    /// <summary>
    /// Generates galactic news headlines based on recent player actions, major events, and world state.
    /// </summary>
    public List<string> GenerateGalacticNews(GameState gameState, List<GameEvent> recentEvents)
    {
        var news = new List<string>();
        var player = gameState.Factions.FirstOrDefault(f => f.Id == gameState.PlayerFactionId);
        if (player == null)
            return news;

        // Major event headlines
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

        // Reputation-based news
        if (gameState.Reputation >= 80)
        {
            news.Add(string.Format(LegendaryReputation, player.Name));
        }
        else if (gameState.Reputation <= -80)
        {
            news.Add(string.Format(InfamyReputation, player.Name));
        }
        else if (gameState.Reputation >= 40)
        {
            news.Add(string.Format(RisingStarReputation, player.Name));
        }
        else if (gameState.Reputation <= -40)
        {
            news.Add(string.Format(NotoriousReputation, player.Name));
        }

        // World state news
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

        // Limit to 3-5 headlines per cycle
        return [.. news.Take(5)];
    }
}

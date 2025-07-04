using FactionsAtTheEnd.Enums;
using FactionsAtTheEnd.Models;

namespace FactionsAtTheEnd.UI;

public static class EventTemplates
{
    public const string InitialCrisisTitle = "Collapse";
    public const string InitialCrisisDescription =
        "The imperial government has fallen. You lead the last organized group in your region. Survival is up to you.";

    public const string VeteranParadeTitle = "Veteran Parade";
    public const string VeteranParadeDescription =
        "A parade of veterans inspires your troops and citizens alike.";

    public const string SuccessfulRaidTitle = "Successful Raid";
    public const string SuccessfulRaidDescription =
        "Your pirates pull off a daring raid, boosting resources and morale!";

    public const string RaidersAttackTitle = "Raiders Attack";
    public const string RaidersAttackDescription =
        "A group of raiders attacks your supply lines, straining your defenses.";

    public const string InternalMutinyTitle = "Internal Mutiny";
    public const string InternalMutinyDescription =
        "A mutiny breaks out among your troops, threatening stability.";

    public const string MercenaryTroubleTitle = "Mercenary Trouble";
    public const string MercenaryTroubleDescription =
        "Mercenaries demand higher pay or threaten to desert.";

    public const string EliteTrainingTitle = "Elite Training";
    public const string EliteTrainingDescription =
        "Your officers organize elite training, boosting your forces.";

    public const string BorderSkirmishTitle = "Border Skirmish";
    public const string BorderSkirmishDescription =
        "A border skirmish tests your readiness. Losses are minimal, but morale is shaken.";

    public const string VeteranRecruitsTitle = "Veteran Recruits";
    public const string VeteranRecruitsDescription =
        "Veterans from other regions join your cause, strengthening your army.";

    public const string PeacefulGarrisonTitle = "Peaceful Garrison";
    public const string PeacefulGarrisonDescription =
        "Your garrisons report no incidents. Troops rest and recover.";

    public const string TrainingAccidentTitle = "Training Accident";
    public const string TrainingAccidentDescription =
        "A minor accident during training causes a brief setback.";

    public const string SabotageDiscoveredTitle = "Sabotage Discovered";
    public const string SabotageDiscoveredDescription =
        "Saboteurs are caught damaging your military infrastructure. Losses are minimized, but morale suffers.";

    public const string HeroicStandTitle = "Heroic Stand";
    public const string HeroicStandDescription =
        "A small unit makes a heroic stand, inspiring your forces and the populace.";

    public const string MarketBoomTitle = "Market Boom";
    public const string MarketBoomDescription =
        "A surge in the market brings a windfall to your coffers.";

    public const string TithesOfferingsTitle = "Tithes and Offerings";
    public const string TithesOfferingsDescription =
        "The faithful donate generously, swelling your resources.";

    public const string ResourceShortageTitle = "Resource Shortage";
    public const string ResourceShortageDescription =
        "Critical resources become scarce due to supply chain disruptions.";

    public const string MarketInstabilityTitle = "Market Instability";
    public const string MarketInstabilityDescription =
        "Economic uncertainty causes prices to fluctuate wildly.";

    public const string BlackMarketSurgeTitle = "Black Market Surge";
    public const string BlackMarketSurgeDescription =
        "Illegal goods flood your markets as law enforcement breaks down.";

    public const string TradeConvoyArrivesTitle = "Trade Convoy Arrives";
    public const string TradeConvoyArrivesDescription =
        "A friendly trade convoy brings much-needed supplies.";

    public const string SmugglingRingBustedTitle = "Smuggling Ring Busted";
    public const string SmugglingRingBustedDescription =
        "You uncover a smuggling ring, recovering stolen goods.";

    public const string ResourceWindfallTitle = "Resource Windfall";
    public const string ResourceWindfallDescription =
        "A new resource deposit is discovered, boosting your economy!";

    public const string EfficientLogisticsTitle = "Efficient Logistics";
    public const string EfficientLogisticsDescription =
        "Your supply officers optimize routes, saving resources.";

    public const string CharityDriveTitle = "Charity Drive";
    public const string CharityDriveDescription =
        "A charity drive boosts morale and stability among the people.";

    public const string ResourceCacheFoundTitle = "Resource Cache Found";
    public const string ResourceCacheFoundDescription =
        "A hidden cache of resources is discovered, providing a much-needed boost.";

    public const string CorruptOfficialExposedTitle = "Corrupt Official Exposed";
    public const string CorruptOfficialExposedDescription =
        "A corrupt official is exposed, restoring some faith in your leadership but causing short-term instability.";

    public const string BreakthroughAlgorithmTitle = "Breakthrough Algorithm";
    public const string BreakthroughAlgorithmDescription =
        "Your scientists develop a revolutionary algorithm, accelerating research.";

    public const string RecoveredImperialDatabaseTitle = "Recovered Imperial Database";
    public const string RecoveredImperialDatabaseDescription =
        "You recover a lost imperial database, boosting your technological edge.";

    public const string TechBreakdownTitle = "Tech Breakdown";
    public const string TechBreakdownDescription =
        "A critical system fails, requiring urgent repairs.";

    public const string ResearchBreakthroughTitle = "Research Breakthrough";
    public const string ResearchBreakthroughDescription =
        "Your scientists make a breakthrough, advancing your technology.";

    public const string UnexpectedInnovationTitle = "Unexpected Innovation";
    public const string UnexpectedInnovationDescription =
        "A junior scientist invents a new process, boosting morale and tech!";

    public const string PrototypeSuccessTitle = "Prototype Success";
    public const string PrototypeSuccessDescription =
        "A risky prototype works perfectly, giving you an edge.";

    public const string EquipmentTheftTitle = "Equipment Theft";
    public const string EquipmentTheftDescription =
        "Thieves steal valuable research equipment, setting you back.";

    public const string TechFestivalTitle = "Tech Festival";
    public const string TechFestivalDescription =
        "A festival celebrating innovation inspires your scientists.";

    public const string FailedExperimentTitle = "Failed Experiment";
    public const string FailedExperimentDescription = "A failed experiment causes a minor setback.";

    public const string AIMalfunctionTitle = "AI Malfunction";
    public const string AIMalfunctionDescription =
        "A critical AI system malfunctions, disrupting research and operations.";

    public const string EnergyBreakthroughTitle = "Energy Breakthrough";
    public const string EnergyBreakthroughDescription =
        "A breakthrough in energy technology increases your efficiency and output.";

    public const string AncientMemoryStirredTitle = "Ancient Memory Stirred";
    public const string AncientMemoryStirredDescription =
        "A memory from a forgotten age grants your people new insight.";

    public const string EchoesOfTheFirstEmpireTitle = "Echoes of the First Empire";
    public const string EchoesOfTheFirstEmpireDescription =
        "Your people recall secrets of the First Empire, unlocking new paths.";

    public const string AncientRuinsFoundTitle = "Ancient Ruins Found";
    public const string AncientRuinsFoundDescription =
        "You discover ancient ruins containing valuable technology.";

    public const string LostDataRecoveredTitle = "Lost Data Recovered";
    public const string LostDataRecoveredDescription =
        "Lost data archives are recovered, revealing secrets of the past.";

    public const string MysteriousSignalDetectedTitle = "Mysterious Signal Detected";
    public const string MysteriousSignalDetectedDescription =
        "A mysterious signal is detected, hinting at unknown opportunities.";

    public const string DangerousRelicActivatedTitle = "Dangerous Relic Activated";
    public const string DangerousRelicActivatedDescription =
        "A relic malfunctions, causing chaos and blocking research!";

    public const string AlienArtifactTitle = "Alien Artifact";
    public const string AlienArtifactDescription =
        "An alien artifact is found, boosting your influence and curiosity.";

    public const string ForgottenCacheTitle = "Forgotten Cache";
    public const string ForgottenCacheDescription =
        "A forgotten cache of supplies is discovered in the ruins.";

    public const string CulturalExchangeTitle = "Cultural Exchange";
    public const string CulturalExchangeDescription =
        "A cultural exchange with outsiders brings new ideas.";

    public const string FalseLeadTitle = "False Lead";
    public const string FalseLeadDescription = "A promising lead turns out to be a dead end.";

    public const string PilgrimageMiracleTitle = "Pilgrimage Miracle";
    public const string PilgrimageMiracleDescription =
        "A miracle during a pilgrimage inspires hope and unity.";

    public const string SolarFlareTitle = "Solar Flare";
    public const string SolarFlareDescription =
        "A solar flare disrupts communications and damages equipment.";

    public const string MeteorShowerTitle = "Meteor Shower";
    public const string MeteorShowerDescription =
        "A meteor shower causes damage to infrastructure.";

    public const string PlagueOutbreakTitle = "Plague Outbreak";
    public const string PlagueOutbreakDescription =
        "A sudden outbreak of disease threatens your population.";

    public const string BountifulHarvestTitle = "Bountiful Harvest";
    public const string BountifulHarvestDescription = "Against all odds, your crops thrive!";

    public const string EarthquakeTitle = "Earthquake!";
    public const string EarthquakeDescription =
        "A powerful earthquake shakes your settlements, causing damage.";

    public const string MildSeasonTitle = "Mild Season";
    public const string MildSeasonDescription =
        "The weather is calm and pleasant, helping your people recover.";

    public const string GentleRainsTitle = "Gentle Rains";
    public const string GentleRainsDescription = "Gentle rains bring a season of prosperity.";

    public const string MinorFloodTitle = "Minor Flood";
    public const string MinorFloodDescription =
        "A minor flood causes inconvenience but little damage.";

    // Crisis events
    public const string PopulationCrisisTitle = "Population Crisis";
    public const string PopulationCrisisDescription =
        "Your population is on the brink of collapse! Take urgent action.";

    public const string ResourceCrisisTitle = "Resource Crisis";
    public const string ResourceCrisisDescription =
        "Your resources are nearly depleted! Find new supplies soon.";

    public const string StabilityCrisisTitle = "Stability Crisis";
    public const string StabilityCrisisDescription =
        "Your people are losing faith in your leadership! Restore order quickly.";

    public const string FactionOnTheBrinkTitle = "Faction on the Brink!";
    public const string FactionOnTheBrinkDescription =
        "Your people are losing hope. Desperate measures are needed.";

    public const string MajorCrisisTitle = "Major Crisis";
    public const string MajorCrisisDescription =
        "A major crisis shakes your faction to its core, testing your leadership.";

    public const string DiplomaticOvertureTitle = "Diplomatic Overture";
    public const string DiplomaticOvertureDescription =
        "A neighboring faction offers an alliance. Do you accept?";

    public const string AncientTechnologyUnearthedTitle = "Ancient Technology Unearthed";
    public const string AncientTechnologyUnearthedDescription =
        "You unearth powerful ancient technology, offering new possibilities.";

    public const string ProsperityWaveTitle = "Prosperity Wave";
    public const string ProsperityWaveDescription =
        "Your faction's high stability has led to a period of growth and optimism.";

    public const string UprisingIgnitedTitle = "Uprising Ignited";
    public const string UprisingIgnitedDescription =
        "Your agents inspire a local uprising, swelling your ranks and spreading hope among the oppressed.";

    public const string AssignEscortPrompt = "Assign military escort to protect refugees?";
    public const string AssignEscortYes = "Yes, assign escort (costs military, boosts reputation)";
    public const string AssignEscortNo = "No, let them fend for themselves (risk minor loss)";
    public const string RefuseFurtherInvolvement = "Refuse further involvement";

    public const string ProceedWithDeepScan = "Proceed with deep scan";
    public const string ScanSuccess = "Scan successful! Gain major tech.";
    public const string ScanBackfire = "Scan backfires! Lose stability.";
    public const string AbortScan = "Abort scan";

    public const string CommitForcesToBattle = "Commit forces to battle";
    public const string SeekDiplomaticSolution = "Seek diplomatic solution";
    public const string OfferConcessions = "Offer concessions";
    public const string RefuseToCompromise = "Refuse to compromise";

    public static readonly EventChoice AidRefugeesMultiStep = new(
        ChoiceEventTemplates.AidRefugeesDescription,
        new Dictionary<StatKey, int> { { StatKey.Reputation, 10 }, { StatKey.Resources, -10 } },
        null,
        [
            new(
                AssignEscortPrompt,
                null,
                null,
                [
                    new(
                        AssignEscortYes,
                        new Dictionary<StatKey, int>
                        {
                            { StatKey.Military, -5 },
                            { StatKey.Reputation, 5 },
                        }
                    ),
                    new(AssignEscortNo, new Dictionary<StatKey, int> { { StatKey.Stability, -3 } }),
                ]
            ),
            new(RefuseFurtherInvolvement, []),
        ]
    );

    public static readonly EventChoice InvestigateAnomalyMultiStep = new(
        ChoiceEventTemplates.InvestigateAnomalyDescription,
        new Dictionary<StatKey, int> { { StatKey.Technology, 5 }, { StatKey.Stability, -2 } },
        null,
        [
            new(
                ProceedWithDeepScan,
                null,
                null,
                [
                    new(ScanSuccess, new Dictionary<StatKey, int> { { StatKey.Technology, 15 } }),
                    new(ScanBackfire, new Dictionary<StatKey, int> { { StatKey.Stability, -10 } }),
                ]
            ),
            new(AbortScan, []),
        ]
    );

    public static readonly EventChoice MilitaryDilemmaMultiStep = new(
        ChoiceEventTemplates.MilitaryChoiceDescription,
        null,
        null,
        [
            new(
                CommitForcesToBattle,
                new Dictionary<StatKey, int>
                {
                    { StatKey.Military, -10 },
                    { StatKey.Reputation, 5 },
                }
            ),
            new(
                SeekDiplomaticSolution,
                new Dictionary<StatKey, int> { { StatKey.Influence, -5 } },
                null,
                [
                    new(
                        OfferConcessions,
                        new Dictionary<StatKey, int>
                        {
                            { StatKey.Resources, -5 },
                            { StatKey.Stability, 5 },
                        }
                    ),
                    new(
                        RefuseToCompromise,
                        new Dictionary<StatKey, int> { { StatKey.Reputation, -5 } }
                    ),
                ]
            ),
        ]
    );
}

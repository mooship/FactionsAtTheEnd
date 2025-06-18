using System.ComponentModel.DataAnnotations;

namespace FactionsAtTheEnd.Models;

/// <summary>
/// All possible actions a player can take each turn.
/// </summary>
public enum PlayerActionType
{
    [Display(Name = "Build Defenses")]
    BuildDefenses,

    [Display(Name = "Recruit Troops")]
    RecruitTroops,

    [Display(Name = "Develop Infrastructure")]
    DevelopInfrastructure,

    [Display(Name = "Exploit Resources")]
    ExploitResources,

    [Display(Name = "Military Tech Research")]
    MilitaryTech,

    [Display(Name = "Economic Tech Research")]
    EconomicTech,

    [Display(Name = "Ancient Studies")]
    AncientStudies,

    [Display(Name = "Gate Network Research")]
    GateNetworkResearch,

    [Display(Name = "Diplomacy")]
    Diplomacy,

    [Display(Name = "Espionage")]
    Espionage,
}

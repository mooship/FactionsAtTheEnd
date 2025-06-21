using System.ComponentModel.DataAnnotations;

namespace FactionsAtTheEnd.Models;

/// <summary>
/// All possible actions a player can take each turn.
/// </summary>
public enum PlayerActionType
{
    /// <summary>Build Defenses</summary>
    [Display(Name = "Build Defenses")]
    BuildDefenses,

    /// <summary>Recruit Troops</summary>
    [Display(Name = "Recruit Troops")]
    RecruitTroops,

    /// <summary>Develop Infrastructure</summary>
    [Display(Name = "Develop Infrastructure")]
    DevelopInfrastructure,

    /// <summary>Exploit Resources</summary>
    [Display(Name = "Exploit Resources")]
    ExploitResources,

    /// <summary>Military Tech Research</summary>
    [Display(Name = "Military Tech Research")]
    MilitaryTech,

    /// <summary>Economic Tech Research</summary>
    [Display(Name = "Economic Tech Research")]
    EconomicTech,

    /// <summary>Ancient Studies</summary>
    [Display(Name = "Ancient Studies")]
    AncientStudies,

    /// <summary>Gate Network Research</summary>
    [Display(Name = "Gate Network Research")]
    GateNetworkResearch,

    /// <summary>Diplomacy</summary>
    [Display(Name = "Diplomacy")]
    Diplomacy,

    /// <summary>Espionage</summary>
    [Display(Name = "Espionage")]
    Espionage,
}

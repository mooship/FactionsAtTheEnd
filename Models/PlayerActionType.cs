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

    [Display(Name = "Sabotage")]
    Sabotage,
}

public static class PlayerActionTypeExtensions
{
    /// <summary>
    /// Get the display name for a PlayerActionType using its Display attribute.
    /// </summary>
    public static string GetDisplayName(this PlayerActionType actionType)
    {
        var type = actionType.GetType();
        var memInfo = type.GetMember(actionType.ToString());
        if (memInfo.Length > 0)
        {
            var attrs = memInfo[0].GetCustomAttributes(typeof(DisplayAttribute), false);
            if (attrs.Length > 0)
            {
                return ((DisplayAttribute)attrs[0]).Name ?? actionType.ToString();
            }
        }
        return actionType.ToString();
    }
}

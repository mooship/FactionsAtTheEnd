using System.ComponentModel.DataAnnotations;

namespace FactionsAtTheEnd.Models;

// <summary>
// All possible actions a player can take each turn.
// </summary>
public enum PlayerActionType
{
    [Display(Name = "Build Defenses")]
    Build_Defenses,

    [Display(Name = "Recruit Troops")]
    Recruit_Troops,

    [Display(Name = "Develop Infrastructure")]
    Develop_Infrastructure,

    [Display(Name = "Exploit Resources")]
    Exploit_Resources,

    [Display(Name = "Military Tech Research")]
    Military_Tech,

    [Display(Name = "Economic Tech Research")]
    Economic_Tech,

    [Display(Name = "Ancient Studies")]
    Ancient_Studies,

    [Display(Name = "Gate Network Research")]
    Gate_Network_Research,

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

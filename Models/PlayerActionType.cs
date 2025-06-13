using System.ComponentModel.DataAnnotations;

namespace FactionsAtTheEnd.Models;

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
}

public static class PlayerActionTypeExtensions
{
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

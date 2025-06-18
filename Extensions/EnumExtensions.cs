using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace FactionsAtTheEnd.Extensions
{
    public static class EnumExtensions
    {
        /// <summary>
        /// Gets the display name for an enum value using the Display or DisplayName attribute if present.
        /// Falls back to ToString() if no attribute is found.
        /// </summary>
        public static string GetDisplayName<TEnum>(this TEnum value)
            where TEnum : Enum
        {
            var type = value.GetType();
            var member = type.GetMember(value.ToString());
            if (member.Length > 0)
            {
                var displayAttr = member[0].GetCustomAttribute<DisplayAttribute>();
                if (displayAttr != null && !string.IsNullOrEmpty(displayAttr.Name))
                {
                    return displayAttr.Name;
                }
                var displayNameAttr = member[0].GetCustomAttribute<DisplayNameAttribute>();
                if (displayNameAttr != null && !string.IsNullOrEmpty(displayNameAttr.DisplayName))
                {
                    return displayNameAttr.DisplayName;
                }
            }
            return value.ToString();
        }
    }
}

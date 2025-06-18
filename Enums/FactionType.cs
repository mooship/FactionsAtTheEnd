using System.ComponentModel.DataAnnotations;

namespace FactionsAtTheEnd.Enums;

/// <summary>
/// The type of a faction, which determines its starting bonuses and flavor.
/// </summary>
public enum FactionType
{
    [Display(Name = "Military Junta")]
    MilitaryJunta,

    [Display(Name = "Corporate Council")]
    CorporateCouncil,

    [Display(Name = "Religious Order")]
    ReligiousOrder,

    [Display(Name = "Pirate Alliance")]
    PirateAlliance,

    [Display(Name = "Technocratic Union")]
    TechnocraticUnion,

    [Display(Name = "Rebellion Cell")]
    RebellionCell,

    [Display(Name = "Imperial Remnant")]
    ImperialRemnant,

    [Display(Name = "Ancient Awakened")]
    AncientAwakened,
}

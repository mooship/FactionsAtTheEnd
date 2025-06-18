using System.ComponentModel.DataAnnotations;

namespace FactionsAtTheEnd.Enums;

/// <summary>
/// Main menu and in-game menu options for the UI.
/// </summary>
public enum MenuOption
{
    [Display(Name = "New Game")]
    NewGame,

    [Display(Name = "Load Game")]
    LoadGame,

    [Display(Name = "Help")]
    Help,

    [Display(Name = "Exit")]
    Exit,

    [Display(Name = "Take Action")]
    TakeAction,

    [Display(Name = "View Faction Overview")]
    ViewFactionOverview,

    [Display(Name = "View Event Log")]
    ViewEventLog,

    [Display(Name = "Exit To Main Menu")]
    ExitToMainMenu,

    [Display(Name = "Finish Turn")]
    FinishTurn,

    [Display(Name = "Export Save")]
    ExportSave,

    [Display(Name = "Import Save")]
    ImportSave,
}

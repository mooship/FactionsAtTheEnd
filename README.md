# Factions at the End

A single-player, single-faction survival strategy game.

## MVP Features
- Only one player faction (no AI, diplomacy, or multifaction logic)
- Take actions each turn to manage your resources and survive
- Random events challenge your survival, with clear feedback on their effects
- View a log of all past events and narrative history at any time
- Game ends if Population, Resources, or Stability reach 0
- Win by surviving 20 turns or reaching 100 Technology
- Save/load your progress at any time

## How to Play
1. Run the game and create your faction (choose a name and type)
2. Each turn, select up to two actions to improve your faction
3. Watch for random events and manage your resources
4. After each event, review the specific effects on your stats, resources, or available actions
5. Use the in-game menu to view your faction overview or review the event log
6. Survive as long as possible! If you reach 20 turns or 100 Technology, you win
7. If Population, Resources, or Stability reach 0, it's game over

## Controls
- Use the keyboard to select actions and navigate menus
- All choices are presented with user-friendly names
- Access the event log and faction overview from the in-game menu

## Tech Stack
- .NET 8.0
- Spectre.Console for UI
- LiteDB for save/load
- FluentValidation for input validation

## Notes
- All code is async and robust against invalid input
- See in-game help for more details

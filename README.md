# Factions at the End

A single-player, single-faction survival strategy game.

## MVP Features
- Only one player faction
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
- Serilog for development logging

## Notes
- All code is async and robust against invalid input
- The game is strictly single-player and single-faction. There is no AI, no diplomacy, and no multifaction logic anywhere in the codebase, for now.
- See in-game help for more details

## Contributing

We welcome contributions to make **Factions at the End** even better! Whether you're a developer, designer, or tester, your input is valuable.

### How to Get Started
1. Fork the repository and clone it locally.
2. Check out the [issues](https://github.com/mooship/FactionsAtTheEnd/issues) to see what needs help or suggest your own improvements.
3. Create a new branch for your work (`git checkout -b feature-name`).
4. Submit a pull request when you're ready for review.

### Guidelines
- Follow the existing code style and conventions.
- Write clear and concise commit messages.
- Ensure your changes do not introduce bugs or regressions.
- Provide tests for any new functionality.

### Need Help?
Feel free to open a discussion or reach out via GitHub issues. We'd love to hear your thoughts and answer any questions.

---

Together, we can improve the game and make it even more engaging. Happy coding!

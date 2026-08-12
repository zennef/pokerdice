# Poker Dice

A Unity poker dice game — a portfolio project built with an eye toward turn-based multiplayer. Roll five dice, hold what you want, reroll the rest, and beat the bot's hand.

[TODO: screenshot/gif]

## Tech Stack

- **Unity 6000.3.9f1** (Unity 6), Universal Render Pipeline 17.3.0
- **Rigged Dice** plugin v1.2 (`Assets/Plugins/Rigged Dice`) — physics-based dice rolling with forced/predetermined outcomes
- TextMeshPro, new Input System 1.18.0

## Architecture

**Port/Adapter around the dice plugin.** Game logic never talks to the Rigged Dice plugin directly. [`IDiceRoller`](Assets/Scripts/Dice/IDiceRoller.cs) is the port; [`RiggedDiceAdapter`](Assets/Scripts/Dice/RiggedDiceAdapter.cs) is the sole adapter and the only class in `Assets/Scripts` that references the plugin's `PredictedDice` namespace. This keeps the third-party dice plugin swappable and keeps all randomness/outcome decisions in project code.

**All randomness lives in C#.** Outcomes are decided by game logic (`RollMultipleDice`, the bot strategy, player-forced-outcome sliders) and handed to the adapter as forced face values — the plugin itself is a presentation-only physics/animation layer, never a source of truth for the result.

**Turn ownership.** [`TurnAuthority`](Assets/Scripts/Core/TurnAuthority.cs) is a simple singleton holding whose turn it is (`Player` / `Bot`) and broadcasting changes. Everything that needs to gate input, trigger the bot's turn, or reskin the scene subscribes to it rather than polling.

**Orchestration.** [`GameFlowManager`](Assets/Scripts/Core/GameFlowManager.cs) wires the turn loop together: starts each round, listens for the player/bot finishing their turn, shows the per-turn result popup, resolves the round (comparing evaluated poker-dice hands), and tracks match score against `MatchSettings.WinThreshold`.

**Shared dice box + reskin.** Player and bot roll on the same physical dice box rather than separate rigs. [`DiceBoxReskin`](Assets/Scripts/Visual/DiceBoxReskin.cs) recolors it per-turn via `MaterialPropertyBlock` (no material instancing), fading between a player color and a bot color as `TurnAuthority` changes owner.

**Bot AI.** [`BruteForceEvHoldStrategy`](Assets/Scripts/Bot/BruteForceEvHoldStrategy.cs) implements [`IBotHoldStrategy`](Assets/Scripts/Bot/IBotHoldStrategy.cs): it brute-forces all 32 hold patterns and computes the exact expected hand-rank value across every possible reroll outcome. It's a single-ply heuristic — it optimizes the *next* reroll only, with no lookahead across multiple rerolls.

**Hand evaluation.** [`PokerHandEvaluator`](Assets/Scripts/Hand/PokerHandEvaluator.cs) is a static evaluator for the standard poker-dice hand set (High Card → Five of a Kind, including low/high straights).

### Script layout

```
Assets/Scripts/
  Core/       TurnAuthority, GameFlowManager
  Dice/       IDiceRoller (port), RiggedDiceAdapter (adapter)
  Hand/       PokerHandEvaluator
  Bot/        IBotHoldStrategy, BruteForceEvHoldStrategy, BotTurnController
  UI/         PopupBase, RoundResultPopup, TurnResultPopup
  Visual/     DiceBoxReskin
  Data/       RoundResultData
  Utility/    DiceForceUtility, PokerHandNameFormatter, SetTextWithSlider
  (root)      RollMultipleDice, PlayerTurnController, PlayerDiceSelectionController,
              DiceOutcomeSlot, HeldMarkerFollower, HandResultDisplay, MatchSettings, CanvasController
```

## Current Status

**Implemented**

- Full turn loop: player and bot alternate turns, each with configurable rerolls (`MatchSettings.MaxRerolls`) and a hold/select UI
- Bot AI hold decisions via brute-force expected value
- Hand evaluation, round win/loss/tie comparison, match score tracked against a win threshold
- Turn-result popup (reveals each turn's hand) and round-result popup (win/lose/tie), both with fade transitions
- Dice box recolors per turn owner
- Deterministic/rigged dice outcomes — no true randomness in the physics layer

**Not yet implemented**

- Per-die value display on the round-result popup (currently wired with empty arrays — see the `TODO` in `GameFlowManager.ResolveRound`)
- Kicker-based tie-breaking (equal hand categories currently resolve as a draw)
- Turn-based multiplayer (currently a local bot opponent; `TurnAuthority` is the seam this would extend from)
- A real match-over screen (currently just a `Debug.Log`)

## Opening the Project

1. Install **Unity 6000.3.9f1** via Unity Hub.
2. In Unity Hub, **Open** → select this repository's root folder.
3. Open `Assets/Scenes/PokerDiceScene.unity` and press Play.

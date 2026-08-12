# CLAUDE.md

Instructions for Claude Code working in this repo. This is a Unity 6 (6000.3.9f1) poker dice
project. See [README.md](README.md) for architecture and project status.

## Scope — read this first

- Claude Code edits are restricted to **`Assets/Scripts/` only**.
- Never edit, create, or touch: `.unity` scene files, `.prefab` files, or anything under
  `Assets/Plugins/` (including the Rigged Dice plugin).
- Never edit `ProjectSettings/`, `Packages/manifest.json`, or `.meta` files unless explicitly asked.
- Zennef (the dev) owns all Unity Editor wiring, prefab work, and Play Mode testing. Claude Code
  owns implementation inside `Assets/Scripts/` only — implement the C#, then hand off for
  Inspector wiring and in-editor testing. Do not claim a change was "tested"; Claude cannot run
  the Unity Editor or Play Mode.

## Project structure

```
Assets/Scripts/
  Core/       TurnAuthority (turn-owner singleton), GameFlowManager (round/match orchestration)
  Dice/       IDiceRoller (port interface), RiggedDiceAdapter (plugin adapter)
  Hand/       PokerHandEvaluator (static hand evaluation)
  Bot/        IBotHoldStrategy, BruteForceEvHoldStrategy, BotTurnController
  UI/         PopupBase (fade in/out base), RoundResultPopup, TurnResultPopup
  Visual/     DiceBoxReskin (per-turn recolor via MaterialPropertyBlock)
  Data/       RoundResultData (readonly struct)
  Utility/    DiceForceUtility, PokerHandNameFormatter, SetTextWithSlider
  (root)      RollMultipleDice, PlayerTurnController, PlayerDiceSelectionController,
              DiceOutcomeSlot, HeldMarkerFollower, HandResultDisplay, MatchSettings, CanvasController
```

## Port/Adapter boundary around the dice plugin

- `IDiceRoller` (`Dice/IDiceRoller.cs`) is the only dice-rolling interface game logic should
  depend on.
- `RiggedDiceAdapter` (`Dice/RiggedDiceAdapter.cs`) is the **only** script allowed to reference the
  `PredictedDice` namespace (the Rigged Dice plugin's types). If new dice-plugin functionality is
  needed, extend `IDiceRoller` + `RiggedDiceAdapter` — never reference `PredictedDice` from any
  other script.
- **All randomness originates in C# game logic.** Outcomes are decided by callers
  (`RollMultipleDice`, hold logic, forced-outcome UI) and passed into `RollAll`/`RollSubset` as
  explicit forced face values. Never pass `RollData.RandomFace` / a `random = true` path into the
  plugin — it is a presentation-only physics/animation layer, not a source of truth for results.

## Event wiring pattern

- Always wire events via `AddListener` / `RemoveListener`, paired with `OnEnable`/`OnDisable` (or
  `Start`/`OnDestroy` when the subscription target is a static singleton — see
  `TurnAuthority.Instance` usages). **Never** use Inspector-configured persistent listeners on a
  `UnityEvent`.
- Guard every `RemoveListener`/unsubscribe call with a null check on the source object (see
  `RiggedDiceAdapter.OnDestroy`, `GameFlowManager.OnDisable`) so teardown order never throws.

## Known gotchas — follow these, don't rediscover them

1. **`TurnAuthority.Instance` must be read in `Start()`, not `Awake()`.** `Awake()` execution
   order across GameObjects in a scene is not guaranteed, so a consumer's `Awake()` can run before
   `TurnAuthority.Awake()` sets `Instance`. This caused a real duplicate-guard bug. Subscribe to
   `TurnAuthority.Instance.OnTurnOwnerChanged` (and read `CurrentOwner`) from `Start()`. If a
   script must attempt subscription earlier (e.g. `OnEnable`), it must tolerate `Instance == null`
   and retry from `Start()` — see `DiceBoxReskin.TrySubscribe()` for the pattern (silent retry
   before `Start()`, warn only if still null after).

2. **`Dice.OnRollEnd`'s `int` payload is not trustworthy for a genuinely random roll** — a roll
   made with `RollData.RandomFace` reports `-1`, not the landed face. This project avoids the
   issue by always forcing an explicit outcome (see the C# randomness rule above), but if a random
   roll path is ever reintroduced, settle detection must independently compute the landed face
   (the plugin exposes this via `FaceLookingUp` on the die's face map) rather than trusting the
   event's int argument.

3. **Don't parent held-dice markers to the die's transform** — the die's roll rotation couples
   into the marker and it spins with the die. Use position-copying in `LateUpdate` instead (see
   `HeldMarkerFollower`), which follows X/Z only and leaves the marker's own Y and rotation alone.

## Other established conventions

- All scripts live in `namespace PokerDice` (the sole current exception is the empty
  `CanvasController.cs` stub at the root, which predates this convention — don't propagate its
  missing namespace into new files).
- MonoBehaviours with `[SerializeField]` references typically have a private `ValidateReferences()`
  method called from `Start()` that `Debug.LogError`s on any unassigned field, rather than
  throwing or silently no-oping. Follow this pattern for new components with required references.
- `OnValidate()` on components that can exist inside prefab assets should early-out via
  `UnityEditor.PrefabUtility.IsPartOfPrefabAsset(gameObject)` (wrapped in `#if UNITY_EDITOR`)
  before warning about unassigned fields — see `HeldMarkerFollower.OnValidate()`. A prefab asset on
  disk legitimately has unwired scene references; only warn once it's a live instance.
- `MatchSettings` is a `ScriptableObject` (`PokerDice/Match Settings` asset menu) — add new
  match-tunable values there rather than as inspector fields on individual controllers.

## Testing

- There is no automated test suite in `Assets/Scripts` (`com.unity.test-framework` is present as a
  package dependency but currently unused).
- Verify changes by reading the code path end-to-end; do not claim Play Mode behavior was
  confirmed unless the user reports back after testing in-editor.

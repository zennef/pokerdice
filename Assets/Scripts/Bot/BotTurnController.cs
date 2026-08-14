using System;
using System.Collections;
using UnityEngine;

namespace PokerDice
{
    public class BotTurnController : MonoBehaviour
    {
        [SerializeField] private RollMultipleDice rollMultipleDice;
        [SerializeField] private MatchSettings matchSettings;
        [SerializeField] private PlayerDiceSelectionController diceSelectionUI;
        [SerializeField] private DiceRollOverride diceRollOverride;
        [SerializeField] private float holdRevealDelay = 0.5f;

        private readonly IBotHoldStrategy _strategy = new BruteForceEvHoldStrategy();
        private int[] _currentFaces = new int[5];
        private bool _handEvaluated;
        private PokerDiceHand _lastEvaluatedHand;
        private Coroutine _turnRoutine;

        public event Action<PokerHandResult> OnBotFinishedTurn;

        private void Start()
        {
            ValidateReferences();

            if (TurnAuthority.Instance == null)
            {
                Debug.LogWarning("BotTurnController: TurnAuthority.Instance is null in Start — skipping subscription.");
                return;
            }

            TurnAuthority.Instance.OnTurnOwnerChanged += HandleTurnOwnerChanged;
        }

        private void OnEnable()
        {
            if (rollMultipleDice != null)
            {
                rollMultipleDice.OnDieSettled += HandleDieSettled;
                rollMultipleDice.OnHandEvaluated += HandleHandEvaluated;
            }
        }

        private void OnDisable()
        {
            if (rollMultipleDice != null)
            {
                rollMultipleDice.OnDieSettled -= HandleDieSettled;
                rollMultipleDice.OnHandEvaluated -= HandleHandEvaluated;
            }

            if (_turnRoutine != null)
            {
                StopCoroutine(_turnRoutine);
                _turnRoutine = null;
            }
        }

        private void OnDestroy()
        {
            if (TurnAuthority.Instance != null)
            {
                TurnAuthority.Instance.OnTurnOwnerChanged -= HandleTurnOwnerChanged;
            }
        }

        private void ValidateReferences()
        {
            if (rollMultipleDice == null)
            {
                Debug.LogError("BotTurnController: rollMultipleDice is not assigned.");
            }

            if (matchSettings == null)
            {
                Debug.LogError("BotTurnController: matchSettings is not assigned.");
            }

            if (diceSelectionUI == null)
            {
                Debug.LogError("BotTurnController: diceSelectionUI is not assigned.");
            }

            if (diceRollOverride == null)
            {
                Debug.LogError("BotTurnController: diceRollOverride is not assigned.");
            }
        }

        private int GetTargetFace(int dieIndex)
        {
            if (diceRollOverride != null && diceRollOverride.TryGetForcedFace(TurnOwner.Bot, dieIndex, out var forced))
            {
                return forced;
            }

            return UnityEngine.Random.Range(1, 7);
        }

        private void HandleDieSettled(int index, int faceValue)
        {
            _currentFaces[index] = faceValue;
        }

        private void HandleTurnOwnerChanged(TurnOwner newOwner)
        {
            if (_turnRoutine != null)
            {
                StopCoroutine(_turnRoutine);
                _turnRoutine = null;
            }

            if (newOwner == TurnOwner.Bot)
            {
                _turnRoutine = StartCoroutine(RunBotTurn());
            }
        }

        private void HandleHandEvaluated(PokerDiceHand hand)
        {
            if (TurnAuthority.Instance == null || TurnAuthority.Instance.CurrentOwner != TurnOwner.Bot)
            {
                return;
            }

            _lastEvaluatedHand = hand;
            _handEvaluated = true;
        }

        private IEnumerator RunBotTurn()
        {
            for (int i = 0; i < 5; i++)
            {
                diceSelectionUI.SetDieHeldVisual(i, false);
            }

            var shouldRoll = new bool[5];
            var targetFaces = new int[5];
            for (int i = 0; i < 5; i++)
            {
                shouldRoll[i] = true;
                targetFaces[i] = GetTargetFace(i);
            }

            yield return RollAndWait(shouldRoll, targetFaces);

            int maxRerolls = matchSettings.MaxRerolls;
            int rerollsUsed = 0;

            while (rerollsUsed < maxRerolls)
            {
                bool[] holds = _strategy.DecideHolds(_currentFaces, maxRerolls - rerollsUsed);
                Debug.Log($"[HeldMarkerDebug] frame {Time.frameCount}: reroll {rerollsUsed}, holds = [{string.Join(",", holds)}]");

                if (Array.TrueForAll(holds, h => h))
                {
                    Debug.Log($"[HeldMarkerDebug] frame {Time.frameCount}: all dice held, skipping reroll {rerollsUsed}");
                    break;
                }

                for (int i = 0; i < 5; i++)
                {
                    if (!holds[i])
                    {
                        diceSelectionUI.SetDieHeldVisual(i, false);
                    }
                }

                for (int i = 0; i < 5; i++)
                {
                    if (holds[i])
                    {
                        diceSelectionUI.SetDieHeldVisual(i, true);
                        yield return new WaitForSeconds(holdRevealDelay);
                    }
                }

                var rerollShouldRoll = new bool[5];
                var rerollTargetFaces = new int[5];
                for (int i = 0; i < 5; i++)
                {
                    rerollShouldRoll[i] = !holds[i];
                    rerollTargetFaces[i] = holds[i] ? _currentFaces[i] : GetTargetFace(i);
                }

                for (int i = 0; i < 5; i++)
                {
                    diceSelectionUI.SetDieHeldVisual(i, false);
                }

                yield return RollAndWait(rerollShouldRoll, rerollTargetFaces);

                rerollsUsed++;
            }

            Debug.Log($"BotTurnController: final hand evaluated as {_lastEvaluatedHand}");

            OnBotFinishedTurn?.Invoke(PokerHandEvaluator.EvaluateDetailed(_currentFaces));
        }

        private IEnumerator RollAndWait(bool[] shouldRoll, int[] targetFaces)
        {
            _handEvaluated = false;
            rollMultipleDice.RollSubset(shouldRoll, targetFaces);
            yield return new WaitUntil(() => _handEvaluated);
        }
    }
}

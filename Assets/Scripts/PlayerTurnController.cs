using System;
using UnityEngine;

namespace PokerDice
{
    public class PlayerTurnController : MonoBehaviour
    {
        [SerializeField] private RollMultipleDice rollMultipleDice;
        [SerializeField] private PlayerDiceSelectionController playerDiceSelectionController;
        [SerializeField] private MatchSettings matchSettings;

        private bool _hasRolledOnce;
        private int _rerollsUsed;

        public event Action<PokerDiceHand> OnMidTurnHandEvaluated;
        public event Action<int> OnRerollsRemainingChanged;

        private void OnEnable()
        {
            ValidateReferences();

            if (rollMultipleDice != null)
            {
                rollMultipleDice.OnHandEvaluated += HandleHandEvaluated;
                rollMultipleDice.OnRollStarted += HandleRollStarted;
            }
        }

        private void Start()
        {
            if (TurnAuthority.Instance == null)
            {
                Debug.LogError("PlayerTurnController: TurnAuthority.Instance is null in Start — skipping subscription.");
            }
            else
            {
                TurnAuthority.Instance.OnTurnOwnerChanged += HandleTurnOwnerChanged;
            }
        }

        private void OnDisable()
        {
            if (rollMultipleDice != null)
            {
                rollMultipleDice.OnHandEvaluated -= HandleHandEvaluated;
                rollMultipleDice.OnRollStarted -= HandleRollStarted;
            }

            if (TurnAuthority.Instance != null)
            {
                TurnAuthority.Instance.OnTurnOwnerChanged -= HandleTurnOwnerChanged;
            }
        }

        private void ValidateReferences()
        {
            if (rollMultipleDice == null)
            {
                Debug.LogError("PlayerTurnController: rollMultipleDice is not assigned.");
            }

            if (playerDiceSelectionController == null)
            {
                Debug.LogError("PlayerTurnController: playerDiceSelectionController is not assigned.");
            }

            if (matchSettings == null)
            {
                Debug.LogError("PlayerTurnController: matchSettings is not assigned.");
            }
        }

        private void HandleRollStarted()
        {
            if (TurnAuthority.Instance == null || !MatchLaunchOptions.IsHumanControlled(TurnAuthority.Instance.CurrentOwner))
            {
                return;
            }

            if (!_hasRolledOnce)
            {
                return;
            }

            _rerollsUsed++;

            int maxRerolls = matchSettings != null ? matchSettings.MaxRerolls : 1;
            OnRerollsRemainingChanged?.Invoke(Mathf.Max(0, maxRerolls - _rerollsUsed));
        }

        private void HandleHandEvaluated(PokerDiceHand hand)
        {
            if (TurnAuthority.Instance == null)
            {
                Debug.LogWarning("PlayerTurnController: TurnAuthority.Instance is null in HandleHandEvaluated — ignoring event.");
                return;
            }

            if (!MatchLaunchOptions.IsHumanControlled(TurnAuthority.Instance.CurrentOwner))
            {
                return;
            }

            _hasRolledOnce = true;

            int maxRerolls = matchSettings != null ? matchSettings.MaxRerolls : 1;
            bool turnOver = _rerollsUsed >= maxRerolls;
            Debug.Log($"PlayerTurnController: rerolls used {_rerollsUsed}/{maxRerolls}. Turn over: {turnOver}");

            if (playerDiceSelectionController == null)
            {
                return;
            }

            if (turnOver)
            {
                playerDiceSelectionController.FinishTurn();
            }
            else
            {
                playerDiceSelectionController.UnlockRollButton();
                OnMidTurnHandEvaluated?.Invoke(hand);
            }
        }

        private void HandleTurnOwnerChanged(TurnOwner newOwner)
        {
            if (MatchLaunchOptions.IsHumanControlled(newOwner))
            {
                ResetForNewTurn();
            }
        }

        public void ResetForNewTurn()
        {
            _hasRolledOnce = false;
            _rerollsUsed = 0;

            OnRerollsRemainingChanged?.Invoke(matchSettings != null ? matchSettings.MaxRerolls : 1);
        }
    }
}

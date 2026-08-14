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

        private void OnEnable()
        {
            ValidateReferences();

            if (rollMultipleDice != null)
            {
                rollMultipleDice.OnHandEvaluated += HandleHandEvaluated;
            }

            if (playerDiceSelectionController != null)
            {
                playerDiceSelectionController.OnPlayerFinishedTurn += HandlePlayerFinishedTurn;
            }
        }

        private void OnDisable()
        {
            if (rollMultipleDice != null)
            {
                rollMultipleDice.OnHandEvaluated -= HandleHandEvaluated;
            }

            if (playerDiceSelectionController != null)
            {
                playerDiceSelectionController.OnPlayerFinishedTurn -= HandlePlayerFinishedTurn;
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

        private void HandleHandEvaluated(PokerDiceHand hand)
        {
            if (TurnAuthority.Instance == null)
            {
                Debug.LogWarning("PlayerTurnController: TurnAuthority.Instance is null in HandleHandEvaluated — ignoring event.");
                return;
            }

            if (TurnAuthority.Instance.CurrentOwner != TurnOwner.Player)
            {
                return;
            }

            if (_hasRolledOnce)
            {
                _rerollsUsed++;
            }
            else
            {
                _hasRolledOnce = true;
            }

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

        private void HandlePlayerFinishedTurn()
        {
            Debug.Log("PlayerTurnController: player finished turn, resetting.");
            ResetForNewTurn();
        }

        public void ResetForNewTurn()
        {
            _hasRolledOnce = false;
            _rerollsUsed = 0;
        }
    }
}

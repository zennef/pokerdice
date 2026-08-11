using System.Collections;
using UnityEngine;

namespace PokerDice
{
    public class GameFlowManager : MonoBehaviour
    {
        [SerializeField] private RollMultipleDice rollMultipleDice;
        [SerializeField] private MatchSettings matchSettings;
        [SerializeField] private BotTurnController botTurnController;
        [SerializeField] private PlayerDiceSelectionController diceSelectionUI;

        private PokerDiceHand _lastEvaluatedHand;
        private PokerDiceHand _botFinalHand;
        private PokerDiceHand _playerFinalHand;
        private int _botWins;
        private int _playerWins;
        private bool _matchOver;

        private IEnumerator Start()
        {
            ValidateReferences();

            // Let every other component's own Start() run first, so BotTurnController and
            // TurnAuthority.Instance are guaranteed to be subscribed/ready before this class
            // ever calls SetTurnOwner — same category of ordering hazard as the project's known
            // Awake()/TurnAuthority.Instance issue, one frame later.
            yield return null;

            StartRound();
        }

        private void OnEnable()
        {
            if (rollMultipleDice != null)
            {
                rollMultipleDice.OnHandEvaluated += HandleHandEvaluated;
            }

            if (botTurnController != null)
            {
                botTurnController.OnBotFinishedTurn += HandleBotFinishedTurn;
            }

            if (diceSelectionUI != null)
            {
                diceSelectionUI.OnPlayerFinishedTurn += HandlePlayerFinishedTurn;
            }
        }

        private void OnDisable()
        {
            if (rollMultipleDice != null)
            {
                rollMultipleDice.OnHandEvaluated -= HandleHandEvaluated;
            }

            if (botTurnController != null)
            {
                botTurnController.OnBotFinishedTurn -= HandleBotFinishedTurn;
            }

            if (diceSelectionUI != null)
            {
                diceSelectionUI.OnPlayerFinishedTurn -= HandlePlayerFinishedTurn;
            }
        }

        private void ValidateReferences()
        {
            if (rollMultipleDice == null)
            {
                Debug.LogError("GameFlowManager: rollMultipleDice is not assigned.");
            }

            if (matchSettings == null)
            {
                Debug.LogError("GameFlowManager: matchSettings is not assigned.");
            }

            if (botTurnController == null)
            {
                Debug.LogError("GameFlowManager: botTurnController is not assigned.");
            }

            if (diceSelectionUI == null)
            {
                Debug.LogError("GameFlowManager: diceSelectionUI is not assigned.");
            }
        }

        private void HandleHandEvaluated(PokerDiceHand hand)
        {
            _lastEvaluatedHand = hand;
        }

        private void StartRound()
        {
            diceSelectionUI.ResetForNewTurn();

            if (TurnAuthority.Instance == null)
            {
                Debug.LogError("GameFlowManager: TurnAuthority.Instance is null in StartRound — cannot start round.");
                return;
            }

            TurnAuthority.Instance.SetTurnOwner(TurnOwner.Bot);
        }

        private void HandleBotFinishedTurn()
        {
            if (_matchOver)
            {
                return;
            }

            _botFinalHand = _lastEvaluatedHand;
            diceSelectionUI.ResetForNewTurn();
            TurnAuthority.Instance.SetTurnOwner(TurnOwner.Player);
        }

        private void HandlePlayerFinishedTurn()
        {
            if (_matchOver)
            {
                return;
            }

            _playerFinalHand = _lastEvaluatedHand;
            ResolveRound();
        }

        private void ResolveRound()
        {
            if ((int)_botFinalHand > (int)_playerFinalHand)
            {
                _botWins++;
                Debug.Log($"GameFlowManager: Bot wins the round with {_botFinalHand} vs player's {_playerFinalHand}.");
            }
            else if ((int)_playerFinalHand > (int)_botFinalHand)
            {
                _playerWins++;
                Debug.Log($"GameFlowManager: Player wins the round with {_playerFinalHand} vs bot's {_botFinalHand}.");
            }
            else
            {
                Debug.Log($"GameFlowManager: Round is a draw — both hands are {_botFinalHand} (same hand category, no kicker comparison yet).");
            }

            if (_botWins >= matchSettings.WinThreshold || _playerWins >= matchSettings.WinThreshold)
            {
                _matchOver = true;
                string winner = _botWins >= matchSettings.WinThreshold ? "Bot" : "Player";
                Debug.Log($"GameFlowManager: Match over — {winner} wins! Final score — Bot: {_botWins}, Player: {_playerWins}.");
            }
            else
            {
                StartRound();
            }
        }
    }
}

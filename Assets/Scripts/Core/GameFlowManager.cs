using System;
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
        [SerializeField] private RoundResultPopup roundResultPopup;
        [SerializeField] private TurnResultPopup turnResultPopup;
        [SerializeField] private MatchOverPopup matchOverPopup;

        private PokerHandResult _botFinalHand;
        private PokerHandResult _playerFinalHand;
        private int _botWins;
        private int _playerWins;
        private bool _matchOver;
        private Action _pendingTurnResultContinuation;
        private TurnOwner _nextRoundOpener = TurnOwner.Player;
        private TurnOwner _currentRoundOpener;

        public event Action<int, int> OnScoreChanged;
        public event Action OnRoundStarted;
        public event Action<TurnOwner, PokerHandResult> OnOpeningSeatFinished;

        private void Start()
        {
            ValidateReferences();
        }

        public void BeginMatch()
        {
            StartCoroutine(BeginMatchRoutine());
        }

        private IEnumerator BeginMatchRoutine()
        {
            // Let every other component's own Start() run first, so BotTurnController and
            // TurnAuthority.Instance are guaranteed to be subscribed/ready before this class
            // ever calls SetTurnOwner — same category of ordering hazard as the project's known
            // Awake()/TurnAuthority.Instance issue, one frame later.
            yield return null;

            OnScoreChanged?.Invoke(_playerWins, _botWins);

            StartRound();
        }

        private void OnEnable()
        {
            if (botTurnController != null)
            {
                botTurnController.OnBotFinishedTurn += HandleBotFinishedTurn;
            }

            if (diceSelectionUI != null)
            {
                diceSelectionUI.OnPlayerFinishedTurn += HandlePlayerFinishedTurn;
            }

            if (roundResultPopup != null)
            {
                roundResultPopup.OnClosed += HandleRoundResultPopupClosed;
            }

            if (turnResultPopup != null)
            {
                turnResultPopup.OnClosed += HandleTurnResultPopupClosed;
            }
        }

        private void OnDisable()
        {
            if (botTurnController != null)
            {
                botTurnController.OnBotFinishedTurn -= HandleBotFinishedTurn;
            }

            if (diceSelectionUI != null)
            {
                diceSelectionUI.OnPlayerFinishedTurn -= HandlePlayerFinishedTurn;
            }

            if (roundResultPopup != null)
            {
                roundResultPopup.OnClosed -= HandleRoundResultPopupClosed;
            }

            if (turnResultPopup != null)
            {
                turnResultPopup.OnClosed -= HandleTurnResultPopupClosed;
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

            if (roundResultPopup == null)
            {
                Debug.LogError("GameFlowManager: roundResultPopup is not assigned.");
            }

            if (matchOverPopup == null)
            {
                Debug.LogError("GameFlowManager: matchOverPopup is not assigned.");
            }
        }

        private void StartRound()
        {
            diceSelectionUI.ResetForNewTurn();

            if (TurnAuthority.Instance == null)
            {
                Debug.LogError("GameFlowManager: TurnAuthority.Instance is null in StartRound — cannot start round.");
                return;
            }

            _currentRoundOpener = _nextRoundOpener;
            _nextRoundOpener = _nextRoundOpener == TurnOwner.Player ? TurnOwner.Bot : TurnOwner.Player;

            OnRoundStarted?.Invoke();
            TurnAuthority.Instance.SetTurnOwner(_currentRoundOpener);
        }

        private void HandleBotFinishedTurn(PokerHandResult finalHand)
        {
            HandleTurnFinished(TurnOwner.Bot, finalHand);
        }

        private void HandlePlayerFinishedTurn()
        {
            if (TurnAuthority.Instance == null)
            {
                Debug.LogWarning("GameFlowManager: TurnAuthority.Instance is null in HandlePlayerFinishedTurn — ignoring event.");
                return;
            }

            HandleTurnFinished(TurnAuthority.Instance.CurrentOwner, rollMultipleDice.EvaluateDetailedHand());
        }

        private void HandleTurnFinished(TurnOwner finishedOwner, PokerHandResult finishedHand)
        {
            if (_matchOver)
            {
                return;
            }

            if (finishedOwner == TurnOwner.Player)
            {
                _playerFinalHand = finishedHand;
            }
            else
            {
                _botFinalHand = finishedHand;
            }

            if (finishedOwner == _currentRoundOpener)
            {
                TurnOwner nextOwner = finishedOwner == TurnOwner.Player ? TurnOwner.Bot : TurnOwner.Player;
                _pendingTurnResultContinuation = () =>
                {
                    diceSelectionUI.ResetForNewTurn();
                    if (TurnAuthority.Instance != null)
                    {
                        TurnAuthority.Instance.SetTurnOwner(nextOwner);
                    }
                };
                OnOpeningSeatFinished?.Invoke(finishedOwner, finishedHand);
            }
            else
            {
                _pendingTurnResultContinuation = () => ResolveRound();
            }

            turnResultPopup.ShowResult(GetTurnLabel(finishedOwner), PokerHandNameFormatter.Format(finishedHand.Category));
        }

        private string GetTurnLabel(TurnOwner owner)
        {
            if (owner == TurnOwner.Player)
            {
                return MatchLaunchOptions.Mode == MatchMode.Hotseat ? "Player 1's Turn" : "Your Turn";
            }

            return MatchLaunchOptions.Mode == MatchMode.Hotseat ? "Player 2's Turn" : "Bot's Turn";
        }

        private void HandleTurnResultPopupClosed()
        {
            _pendingTurnResultContinuation?.Invoke();
            _pendingTurnResultContinuation = null;
        }

        private void ResolveRound()
        {
            RoundOutcome outcome;

            int comparison = _playerFinalHand.CompareTo(_botFinalHand);

            if (comparison < 0)
            {
                _botWins++;
                outcome = RoundOutcome.BotWins;
                Debug.Log($"GameFlowManager: Bot wins the round with {_botFinalHand.Category} vs player's {_playerFinalHand.Category}.");
            }
            else if (comparison > 0)
            {
                _playerWins++;
                outcome = RoundOutcome.PlayerWins;
                Debug.Log($"GameFlowManager: Player wins the round with {_playerFinalHand.Category} vs bot's {_botFinalHand.Category}.");
            }
            else
            {
                outcome = RoundOutcome.Tie;
                Debug.Log($"GameFlowManager: Round is a draw — both hands are {_botFinalHand.Category} with identical kickers.");
            }

            if (_botWins >= matchSettings.WinThreshold || _playerWins >= matchSettings.WinThreshold)
            {
                _matchOver = true;
            }

            var resultData = new RoundResultData(
                PokerHandNameFormatter.Format(_playerFinalHand.Category),
                PokerHandNameFormatter.Format(_botFinalHand.Category),
                _playerFinalHand.Faces,
                _botFinalHand.Faces,
                outcome);

            OnScoreChanged?.Invoke(_playerWins, _botWins);

            roundResultPopup.ShowResult(resultData);
        }

        private void HandleRoundResultPopupClosed()
        {
            if (_matchOver)
            {
                MatchOutcome outcome = _playerWins >= matchSettings.WinThreshold
                    ? MatchOutcome.PlayerWinsMatch
                    : MatchOutcome.BotWinsMatch;

                var matchResultData = new MatchResultData(outcome, _playerWins, _botWins);
                matchOverPopup.ShowResult(matchResultData);
                return;
            }

            StartRound();
        }
    }
}

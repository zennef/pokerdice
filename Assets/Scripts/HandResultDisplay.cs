using TMPro;
using UnityEngine;

namespace PokerDice
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class HandResultDisplay : MonoBehaviour
    {
        [SerializeField] private RollMultipleDice rollMultipleDice;
        [SerializeField] private PlayerTurnController playerTurnController;
        [SerializeField] private PlayerDiceSelectionController playerDiceSelectionController;
        [SerializeField] private BotTurnController botTurnController;

        private TextMeshProUGUI text;

        private void Awake()
        {
            text = GetComponent<TextMeshProUGUI>();
        }

        private void Start()
        {
            if (rollMultipleDice == null)
            {
                Debug.LogError($"{nameof(HandResultDisplay)} on {name} needs its Roll Multiple Dice field assigned in the Inspector.");
                return;
            }

            if (playerTurnController == null)
            {
                Debug.LogError($"{nameof(HandResultDisplay)} on {name} needs its Player Turn Controller field assigned in the Inspector.");
                return;
            }

            if (playerDiceSelectionController == null)
            {
                Debug.LogError($"{nameof(HandResultDisplay)} on {name} needs its Player Dice Selection Controller field assigned in the Inspector.");
                return;
            }

            if (botTurnController == null)
            {
                Debug.LogError($"{nameof(HandResultDisplay)} on {name} needs its Bot Turn Controller field assigned in the Inspector.");
                return;
            }

            playerTurnController.OnMidTurnHandEvaluated += HandleMidTurnHandEvaluated;
            botTurnController.OnMidTurnHandEvaluated += HandleMidTurnHandEvaluated;
            playerDiceSelectionController.OnPlayerFinishedTurn += HandleTurnFinished;
            rollMultipleDice.OnRollStarted += HandleRollStarted;
        }

        private void HandleMidTurnHandEvaluated(PokerDiceHand evaluatedHand)
        {
            text.text = PokerHandNameFormatter.Format(evaluatedHand);
        }

        private void HandleRollStarted()
        {
            text.text = "";
        }

        private void HandleTurnFinished()
        {
            text.text = "";
        }

        private void OnDestroy()
        {
            if (playerTurnController != null)
            {
                playerTurnController.OnMidTurnHandEvaluated -= HandleMidTurnHandEvaluated;
            }

            if (botTurnController != null)
            {
                botTurnController.OnMidTurnHandEvaluated -= HandleMidTurnHandEvaluated;
            }

            if (playerDiceSelectionController != null)
            {
                playerDiceSelectionController.OnPlayerFinishedTurn -= HandleTurnFinished;
            }

            if (rollMultipleDice != null)
            {
                rollMultipleDice.OnRollStarted -= HandleRollStarted;
            }
        }
    }
}

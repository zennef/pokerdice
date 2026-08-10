using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace PokerDice
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class HandResultDisplay : MonoBehaviour
    {
        [SerializeField] private RollMultipleDice rollMultipleDice;

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

            rollMultipleDice.OnHandEvaluated += HandleHandEvaluated;
            rollMultipleDice.OnRollStarted += HandleRollStarted;
        }

        private void HandleHandEvaluated(PokerDiceHand evaluatedHand)
        {
            text.text = Regex.Replace(evaluatedHand.ToString(), "(\\B[A-Z])", " $1");
        }

        private void HandleRollStarted()
        {
            text.text = "Rolling...";
        }

        private void OnDestroy()
        {
            if (rollMultipleDice != null)
            {
                rollMultipleDice.OnHandEvaluated -= HandleHandEvaluated;
                rollMultipleDice.OnRollStarted -= HandleRollStarted;
            }
        }
    }
}

using System.Text;
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
        }

        private void OnDestroy()
        {
            if (rollMultipleDice != null)
            {
                rollMultipleDice.OnHandEvaluated -= HandleHandEvaluated;
            }
        }

        private void HandleHandEvaluated(RollMultipleDice.PokerDiceHand hand)
        {
            text.text = FormatHandName(hand.ToString());
        }

        private static string FormatHandName(string name)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < name.Length; i++)
            {
                if (i > 0 && char.IsUpper(name[i]))
                {
                    sb.Append(' ');
                }
                sb.Append(name[i]);
            }
            return sb.ToString();
        }
    }
}

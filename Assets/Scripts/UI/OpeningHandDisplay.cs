using System.Linq;
using TMPro;
using UnityEngine;

namespace PokerDice
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class OpeningHandDisplay : MonoBehaviour
    {
        [SerializeField] private BotTurnController botTurnController;

        private TextMeshProUGUI text;

        private void Awake()
        {
            text = GetComponent<TextMeshProUGUI>();
            text.text = "";
        }

        private void Start()
        {
            if (botTurnController == null)
            {
                Debug.LogError($"{nameof(OpeningHandDisplay)} on {name} needs its Bot Turn Controller field assigned in the Inspector.");
                return;
            }

            botTurnController.OnBotFinishedTurn += HandleBotFinishedTurn;

            if (TurnAuthority.Instance == null)
            {
                Debug.LogWarning($"{nameof(OpeningHandDisplay)}: TurnAuthority.Instance is null in Start — skipping subscription.");
                return;
            }

            TurnAuthority.Instance.OnTurnOwnerChanged += HandleTurnOwnerChanged;
        }

        private void HandleBotFinishedTurn(PokerHandResult finalHand)
        {
            text.text = $"Bot: {PokerHandNameFormatter.Format(finalHand.Category)} — {FormatFaces(finalHand.Faces)}";
        }

        private void HandleTurnOwnerChanged(TurnOwner newOwner)
        {
            if (newOwner == TurnOwner.Bot)
            {
                text.text = "";
            }
        }

        private static string FormatFaces(int[] faces)
        {
            var ordered = faces
                .GroupBy(f => f)
                .OrderByDescending(g => g.Count())
                .ThenByDescending(g => g.Key)
                .SelectMany(g => g);

            return string.Join(",", ordered);
        }

        private void OnDestroy()
        {
            if (botTurnController != null)
            {
                botTurnController.OnBotFinishedTurn -= HandleBotFinishedTurn;
            }

            if (TurnAuthority.Instance != null)
            {
                TurnAuthority.Instance.OnTurnOwnerChanged -= HandleTurnOwnerChanged;
            }
        }
    }
}

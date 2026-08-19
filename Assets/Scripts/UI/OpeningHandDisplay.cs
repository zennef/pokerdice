using System.Linq;
using TMPro;
using UnityEngine;

namespace PokerDice
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class OpeningHandDisplay : MonoBehaviour
    {
        [SerializeField] private GameFlowManager gameFlowManager;

        private TextMeshProUGUI text;

        private void Awake()
        {
            text = GetComponent<TextMeshProUGUI>();
            text.text = "";
        }

        private void Start()
        {
            if (gameFlowManager == null)
            {
                Debug.LogError($"{nameof(OpeningHandDisplay)} on {name} needs its Game Flow Manager field assigned in the Inspector.");
                return;
            }

            gameFlowManager.OnOpeningSeatFinished += HandleOpeningSeatFinished;

            if (TurnAuthority.Instance == null)
            {
                Debug.LogWarning($"{nameof(OpeningHandDisplay)}: TurnAuthority.Instance is null in Start — skipping subscription.");
                return;
            }

            TurnAuthority.Instance.OnTurnOwnerChanged += HandleTurnOwnerChanged;
        }

        private void HandleOpeningSeatFinished(PokerHandResult finalHand)
        {
            string prefix = MatchLaunchOptions.Mode == MatchMode.Hotseat ? "Player 2: " : "Bot: ";
            text.text = $"{prefix}{PokerHandNameFormatter.Format(finalHand.Category)}\n{FormatFaces(finalHand.Faces)}";
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
            if (gameFlowManager != null)
            {
                gameFlowManager.OnOpeningSeatFinished -= HandleOpeningSeatFinished;
            }

            if (TurnAuthority.Instance != null)
            {
                TurnAuthority.Instance.OnTurnOwnerChanged -= HandleTurnOwnerChanged;
            }
        }
    }
}

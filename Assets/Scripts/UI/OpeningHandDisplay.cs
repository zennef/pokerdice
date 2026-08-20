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
            gameFlowManager.OnRoundStarted += HandleRoundStarted;
        }

        private void HandleOpeningSeatFinished(TurnOwner owner, PokerHandResult finalHand)
        {
            string prefix = owner == TurnOwner.Player
                ? (MatchLaunchOptions.Mode == MatchMode.Hotseat ? "Player 1: " : "You: ")
                : (MatchLaunchOptions.Mode == MatchMode.Hotseat ? "Player 2: " : "Bot: ");
            text.text = $"{prefix}{PokerHandNameFormatter.Format(finalHand.Category)}\n{FormatFaces(finalHand.Faces)}";
        }

        private void HandleRoundStarted()
        {
            text.text = "";
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
                gameFlowManager.OnRoundStarted -= HandleRoundStarted;
            }
        }
    }
}

using TMPro;
using UnityEngine;

namespace PokerDice
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class ScoreboardDisplay : MonoBehaviour
    {
        [SerializeField] private GameFlowManager gameFlowManager;

        private TextMeshProUGUI text;

        private void Awake()
        {
            text = GetComponent<TextMeshProUGUI>();
        }

        private void Start()
        {
            if (gameFlowManager == null)
            {
                Debug.LogError($"{nameof(ScoreboardDisplay)} on {name} needs its Game Flow Manager field assigned in the Inspector.");
                return;
            }

            gameFlowManager.OnScoreChanged += HandleScoreChanged;
        }

        private void HandleScoreChanged(int playerWins, int botWins)
        {
            text.text = $"Bot {botWins} - {playerWins} Player";
        }

        private void OnDestroy()
        {
            if (gameFlowManager != null)
            {
                gameFlowManager.OnScoreChanged -= HandleScoreChanged;
            }
        }
    }
}

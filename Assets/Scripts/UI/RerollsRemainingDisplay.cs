using TMPro;
using UnityEngine;

namespace PokerDice
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class RerollsRemainingDisplay : MonoBehaviour
    {
        [SerializeField] private PlayerTurnController playerTurnController;
        [SerializeField] private BotTurnController botTurnController;

        private TextMeshProUGUI text;
        private int _lastKnownRemaining;
        private bool _isGameplayActive = true;

        private void Awake()
        {
            text = GetComponent<TextMeshProUGUI>();
        }

        private void Start()
        {
            if (playerTurnController == null)
            {
                Debug.LogError($"{nameof(RerollsRemainingDisplay)} on {name} needs its Player Turn Controller field assigned in the Inspector.");
                return;
            }

            if (botTurnController == null)
            {
                Debug.LogError($"{nameof(RerollsRemainingDisplay)} on {name} needs its Bot Turn Controller field assigned in the Inspector.");
                return;
            }

            playerTurnController.OnRerollsRemainingChanged += HandleRerollsRemainingChanged;
            botTurnController.OnRerollsRemainingChanged += HandleRerollsRemainingChanged;

            if (GameplayVisibility.Instance != null)
            {
                GameplayVisibility.Instance.OnGameplayActiveChanged += HandleGameplayActiveChanged;
                HandleGameplayActiveChanged(GameplayVisibility.Instance.IsGameplayActive);
            }
        }

        private void HandleRerollsRemainingChanged(int remaining)
        {
            _lastKnownRemaining = remaining;
            RefreshText();
        }

        private void HandleGameplayActiveChanged(bool isActive)
        {
            _isGameplayActive = isActive;
            RefreshText();
        }

        private void RefreshText()
        {
            text.text = _isGameplayActive ? $"Rerolls: {_lastKnownRemaining}" : "";
        }

        private void OnDestroy()
        {
            if (playerTurnController != null)
            {
                playerTurnController.OnRerollsRemainingChanged -= HandleRerollsRemainingChanged;
            }

            if (botTurnController != null)
            {
                botTurnController.OnRerollsRemainingChanged -= HandleRerollsRemainingChanged;
            }

            if (GameplayVisibility.Instance != null)
            {
                GameplayVisibility.Instance.OnGameplayActiveChanged -= HandleGameplayActiveChanged;
            }
        }
    }
}

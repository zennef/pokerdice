using UnityEngine;
using UnityEngine.UI;

namespace PokerDice
{
    public class MainMenuController : PopupBase
    {
        [SerializeField] private Button playButton;
        [SerializeField] private GameFlowManager gameFlowManager;

        private void Start()
        {
            ValidateReferences();

            if (GameplayVisibility.Instance != null)
            {
                GameplayVisibility.Instance.NotifyOverlayShown();
            }

            if (MatchLaunchOptions.SkipMenu)
            {
                MatchLaunchOptions.SkipMenu = false;
                Hide();
            }
        }

        private void OnEnable()
        {
            playButton.onClick.AddListener(Hide);
        }

        private void OnDisable()
        {
            playButton.onClick.RemoveListener(Hide);
        }

        protected override void OnHidden()
        {
            gameFlowManager.BeginMatch();
        }

        private void ValidateReferences()
        {
            if (playButton == null)
            {
                Debug.LogError("MainMenuController: playButton is not assigned.");
            }

            if (gameFlowManager == null)
            {
                Debug.LogError("MainMenuController: gameFlowManager is not assigned.");
            }
        }
    }
}

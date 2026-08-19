using UnityEngine;
using UnityEngine.UI;

namespace PokerDice
{
    public class MainMenuController : PopupBase
    {
        [SerializeField] private Button vsBotButton;
        [SerializeField] private Button vsPlayerButton;
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
            vsBotButton.onClick.AddListener(HandleVsBotClicked);
            vsPlayerButton.onClick.AddListener(HandleVsPlayerClicked);
        }

        private void OnDisable()
        {
            vsBotButton.onClick.RemoveListener(HandleVsBotClicked);
            vsPlayerButton.onClick.RemoveListener(HandleVsPlayerClicked);
        }

        private void HandleVsBotClicked()
        {
            MatchLaunchOptions.Mode = MatchMode.VsBot;
            Hide();
        }

        private void HandleVsPlayerClicked()
        {
            MatchLaunchOptions.Mode = MatchMode.Hotseat;
            Hide();
        }

        protected override void OnHidden()
        {
            gameFlowManager.BeginMatch();
        }

        private void ValidateReferences()
        {
            if (vsBotButton == null)
            {
                Debug.LogError("MainMenuController: vsBotButton is not assigned.");
            }

            if (vsPlayerButton == null)
            {
                Debug.LogError("MainMenuController: vsPlayerButton is not assigned.");
            }

            if (gameFlowManager == null)
            {
                Debug.LogError("MainMenuController: gameFlowManager is not assigned.");
            }
        }
    }
}

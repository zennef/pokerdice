using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PokerDice
{
    public class MatchOverPopup : PopupBase
    {
        [SerializeField] private TMP_Text winnerLabel;
        [SerializeField] private TMP_Text finalScoreLabel;
        [SerializeField] private Button rematchButton;
        [SerializeField] private Button mainMenuButton;

        private void OnEnable()
        {
            rematchButton.onClick.AddListener(HandleRematchClicked);
            mainMenuButton.onClick.AddListener(HandleMainMenuClicked);
        }

        private void OnDisable()
        {
            rematchButton.onClick.RemoveListener(HandleRematchClicked);
            mainMenuButton.onClick.RemoveListener(HandleMainMenuClicked);
        }

        public void ShowResult(MatchResultData data)
        {
            winnerLabel.text = data.Outcome == MatchOutcome.PlayerWinsMatch
                ? "You Win the Match!"
                : "Bot Wins the Match";
            finalScoreLabel.text = $"{data.PlayerWins} - {data.BotWins}";

            Show();
        }

        private void HandleRematchClicked()
        {
            MatchLaunchOptions.SkipMenu = true;
            ReloadActiveScene();
        }

        private void HandleMainMenuClicked()
        {
            MatchLaunchOptions.SkipMenu = false;
            ReloadActiveScene();
        }

        private static void ReloadActiveScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        [ContextMenu("Test - Show Sample Match Result")]
        private void TestShowSampleMatchResult()
        {
            var sampleData = new MatchResultData(MatchOutcome.PlayerWinsMatch, 3, 1);

            ShowResult(sampleData);
        }
    }
}

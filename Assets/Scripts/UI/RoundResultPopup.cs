using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PokerDice
{
    public class RoundResultPopup : PopupBase
    {
        [SerializeField] private TMP_Text playerHandLabel;
        [SerializeField] private TMP_Text botHandLabel;
        [SerializeField] private TMP_Text outcomeLabel;
        [SerializeField] private Button closeButton;

        private void OnEnable()
        {
            closeButton.onClick.AddListener(Hide);
        }

        private void OnDisable()
        {
            closeButton.onClick.RemoveListener(Hide);
        }

        public void ShowResult(RoundResultData data)
        {
            playerHandLabel.text = data.PlayerHandName;
            botHandLabel.text = data.BotHandName;
            outcomeLabel.text = GetOutcomeText(data.Outcome);

            Show();
        }

        private static string GetOutcomeText(RoundOutcome outcome)
        {
            switch (outcome)
            {
                case RoundOutcome.PlayerWins:
                    return MatchLaunchOptions.Mode == MatchMode.Hotseat ? "Player 1 Wins!" : "You Win!";
                case RoundOutcome.BotWins:
                    return MatchLaunchOptions.Mode == MatchMode.Hotseat ? "Player 2 Wins!" : "Bot Wins";
                case RoundOutcome.Tie:
                    return "Tie";
                default:
                    return outcome.ToString();
            }
        }

        [ContextMenu("Test - Show Sample Result")]
        private void TestShowSampleResult()
        {
            var sampleData = new RoundResultData(
                "Full House",
                "Two Pair",
                new[] { 2, 2, 2, 5, 5 },
                new[] { 3, 3, 6, 6, 1 },
                RoundOutcome.PlayerWins);

            ShowResult(sampleData);
        }
    }
}

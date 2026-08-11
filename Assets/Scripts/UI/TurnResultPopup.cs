using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PokerDice
{
    public class TurnResultPopup : PopupBase
    {
        [SerializeField] private TMP_Text turnOwnerLabel;
        [SerializeField] private TMP_Text handNameLabel;
        [SerializeField] private Button confirmButton;

        private void OnEnable()
        {
            confirmButton.onClick.AddListener(Hide);
        }

        private void OnDisable()
        {
            confirmButton.onClick.RemoveListener(Hide);
        }

        public void ShowResult(string turnOwnerText, string handName)
        {
            turnOwnerLabel.text = turnOwnerText;
            handNameLabel.text = handName;

            Show();
        }

        [ContextMenu("Test - Show Sample Turn Result")]
        private void TestShowSampleTurnResult()
        {
            ShowResult("Bot's Turn", "Three of a Kind");
        }
    }
}

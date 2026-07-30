using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PokerDice
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class SetTextWithSlider : MonoBehaviour
    {
        private TextMeshProUGUI _text;
        private Slider _slider;

        private void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
            _slider = GetComponentInParent<Slider>();
            if (_slider == null)
            {
                Debug.LogError($"{nameof(SetTextWithSlider)} on {name} requires a Slider in a parent GameObject.");
                return;
            }
            _slider.onValueChanged.AddListener(UpdateText);
        }

        private void UpdateText(float value)
        {
            _text.text = value.ToString("F0");
        }
    }
}

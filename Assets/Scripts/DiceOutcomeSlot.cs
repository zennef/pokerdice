using System;
using UnityEngine;
using UnityEngine.UI;

namespace PokerDice
{
    public class DiceOutcomeSlot : MonoBehaviour
    {
        [SerializeField] private RollMultipleDice rollMultipleDice;
        [SerializeField] private int index;
        [SerializeField] private Toggle randomToggle;
        [SerializeField] private Slider outcomeSlider;

        private SetTextWithSlider _valueLabel;
        private bool _isHeld;

        public event Action<int> OnDieSettled;

        public bool IsRandom { get; private set; } = true;
        public int ForcedValue => outcomeSlider != null ? Mathf.RoundToInt(outcomeSlider.value) : 1;

        private void OnEnable()
        {
            if (randomToggle == null)
            {
                Debug.LogError("DiceOutcomeSlot: randomToggle is not assigned.");
            }

            if (outcomeSlider == null)
            {
                Debug.LogError("DiceOutcomeSlot: outcomeSlider is not assigned.");
            }

            _valueLabel = outcomeSlider != null
                ? outcomeSlider.GetComponentInChildren<SetTextWithSlider>()
                : null;

            if (outcomeSlider != null && _valueLabel == null)
            {
                Debug.LogError("DiceOutcomeSlot: outcomeSlider has no SetTextWithSlider in its children.");
            }

            rollMultipleDice.OnDieSettled += HandleDieSettled;

            if (randomToggle != null)
            {
                randomToggle.onValueChanged.AddListener(HandleRandomToggleChanged);
            }

            IsRandom = true;
            if (outcomeSlider != null)
            {
                outcomeSlider.interactable = false;
            }
        }

        private void OnDisable()
        {
            rollMultipleDice.OnDieSettled -= HandleDieSettled;

            if (randomToggle != null)
            {
                randomToggle.onValueChanged.RemoveListener(HandleRandomToggleChanged);
            }
        }

        public void SetOutcome(float outcome)
        {
            rollMultipleDice.SetOutcome(index, (int)outcome);
        }

        public void SetHeld(bool held)
        {
            _isHeld = held;
            UpdateInteractableStates();
        }

        private void HandleRandomToggleChanged(bool isOn)
        {
            IsRandom = isOn;
            UpdateInteractableStates();
        }

        private void UpdateInteractableStates()
        {
            if (randomToggle != null)
            {
                randomToggle.interactable = !_isHeld;
            }

            if (outcomeSlider != null)
            {
                outcomeSlider.interactable = !IsRandom && !_isHeld;
            }
        }

        public void SyncDisplayedValue(int faceValue)
        {
            if (outcomeSlider != null)
            {
                outcomeSlider.SetValueWithoutNotify(faceValue);
                _valueLabel?.Refresh();
            }
        }

        private void HandleDieSettled(int settledIndex, int faceValue)
        {
            if (settledIndex == index)
            {
                if (IsRandom)
                {
                    SyncDisplayedValue(faceValue);
                }

                OnDieSettled?.Invoke(faceValue);
            }
        }
    }
}

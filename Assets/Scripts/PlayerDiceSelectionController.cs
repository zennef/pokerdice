using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PokerDice
{
    public class PlayerDiceSelectionController : MonoBehaviour
    {
        [Serializable]
        public class DieUISlot
        {
            public Button selectButton;
            public TextMeshProUGUI faceLabel;
            public GameObject heldMarker;
            public DiceOutcomeSlot dieOutcomeSlot;
        }

        [SerializeField] private Button rollButton;
        [SerializeField] private Button skipButton;
        [SerializeField] private DieUISlot[] dieSlots = new DieUISlot[5];
        [SerializeField] private RollMultipleDice rollMultipleDice;
        [SerializeField] private Color heldColor = new Color(0.4f, 0.8f, 0.4f);
        [SerializeField] private Color normalColor = Color.white;

        private bool[] _held = new bool[5];
        private int[] _lastSettledFaces = new int[5];

        public event Action OnPlayerFinishedTurn;

        private void Start()
        {
            ValidateReferences();

            rollButton.onClick.AddListener(OnRollClicked);
            skipButton.onClick.AddListener(OnSkipClicked);

            for (int i = 0; i < dieSlots.Length; i++)
            {
                int index = i;
                dieSlots[index].selectButton.onClick.AddListener(() => OnDieButtonClicked(index));
                dieSlots[index].dieOutcomeSlot.OnDieSettled += faceValue => HandleDieSettled(index, faceValue);
            }

            rollMultipleDice.OnHandEvaluated += HandleHandEvaluated;

            ApplyInitialButtonState();
        }

        private void ValidateReferences()
        {
            if (rollButton == null)
            {
                Debug.LogError("PlayerDiceSelectionController: rollButton is not assigned.");
            }

            if (skipButton == null)
            {
                Debug.LogError("PlayerDiceSelectionController: skipButton is not assigned.");
            }

            if (rollMultipleDice == null)
            {
                Debug.LogError("PlayerDiceSelectionController: rollMultipleDice is not assigned.");
            }

            for (int i = 0; i < dieSlots.Length; i++)
            {
                var slot = dieSlots[i];
                if (slot == null)
                {
                    Debug.LogError($"PlayerDiceSelectionController: dieSlots[{i}] is not assigned.");
                    continue;
                }

                if (slot.selectButton == null)
                {
                    Debug.LogError($"PlayerDiceSelectionController: dieSlots[{i}].selectButton is not assigned.");
                }

                if (slot.faceLabel == null)
                {
                    Debug.LogError($"PlayerDiceSelectionController: dieSlots[{i}].faceLabel is not assigned.");
                }

                if (slot.heldMarker == null)
                {
                    Debug.LogError($"PlayerDiceSelectionController: dieSlots[{i}].heldMarker is not assigned.");
                }

                if (slot.dieOutcomeSlot == null)
                {
                    Debug.LogError($"PlayerDiceSelectionController: dieSlots[{i}].dieOutcomeSlot is not assigned.");
                }
            }
        }

        private void ApplyInitialButtonState()
        {
            rollButton.interactable = true;
            skipButton.interactable = false;

            for (int i = 0; i < dieSlots.Length; i++)
            {
                var slot = dieSlots[i];
                slot.selectButton.interactable = false;
                slot.heldMarker.SetActive(false);
                slot.selectButton.targetGraphic.color = normalColor;
                slot.faceLabel.text = "–";
                slot.dieOutcomeSlot.SetHeld(false);
            }
        }

        private void OnRollClicked()
        {
            rollButton.interactable = false;
            skipButton.interactable = false;
            SetAllSelectButtonsInteractable(false);

            var shouldRoll = new bool[dieSlots.Length];
            var targetFaces = new int[dieSlots.Length];
            for (int i = 0; i < dieSlots.Length; i++)
            {
                var slot = dieSlots[i];
                slot.heldMarker.SetActive(false);
                slot.selectButton.targetGraphic.color = normalColor;
                slot.dieOutcomeSlot.SetHeld(false);

                shouldRoll[i] = !_held[i];
                if (shouldRoll[i])
                {
                    slot.faceLabel.text = "–";
                    targetFaces[i] = slot.dieOutcomeSlot.IsRandom
                        ? UnityEngine.Random.Range(1, 7)
                        : slot.dieOutcomeSlot.ForcedValue;
                }
                else
                {
                    targetFaces[i] = _lastSettledFaces[i];
                }

                _held[i] = false;
            }

            rollMultipleDice.RollSubset(shouldRoll, targetFaces);
        }

        private void HandleDieSettled(int index, int faceValue)
        {
            _lastSettledFaces[index] = faceValue;
            dieSlots[index].faceLabel.text = faceValue.ToString();
        }

        private void HandleHandEvaluated(RollMultipleDice.PokerDiceHand hand)
        {
            skipButton.interactable = true;
        }

        private void OnDieButtonClicked(int index)
        {
            _held[index] = !_held[index];
            var slot = dieSlots[index];
            slot.heldMarker.SetActive(_held[index]);
            slot.selectButton.targetGraphic.color = _held[index] ? heldColor : normalColor;
            slot.dieOutcomeSlot.SetHeld(_held[index]);
            if (_held[index])
            {
                slot.dieOutcomeSlot.SyncDisplayedValue(_lastSettledFaces[index]);
            }

            rollButton.interactable = !AllHeld();
        }

        private void OnSkipClicked()
        {
            rollButton.interactable = false;
            skipButton.interactable = false;
            SetAllSelectButtonsInteractable(false);

            for (int i = 0; i < dieSlots.Length; i++)
            {
                var slot = dieSlots[i];
                slot.heldMarker.SetActive(false);
                slot.selectButton.targetGraphic.color = normalColor;
                slot.dieOutcomeSlot.SetHeld(false);
            }

            OnPlayerFinishedTurn?.Invoke();
        }

        public void ResetForNewTurn()
        {
            for (int i = 0; i < _held.Length; i++)
            {
                _held[i] = false;
            }

            ApplyInitialButtonState();
        }

        public void LockRollButton()
        {
            rollButton.interactable = false;
            SetAllSelectButtonsInteractable(false);
        }

        public void UnlockRollButton()
        {
            rollButton.interactable = true;
            SetAllSelectButtonsInteractable(true);
        }

        private bool AllHeld()
        {
            for (int i = 0; i < _held.Length; i++)
            {
                if (!_held[i])
                {
                    return false;
                }
            }

            return true;
        }

        private void SetAllSelectButtonsInteractable(bool interactable)
        {
            for (int i = 0; i < dieSlots.Length; i++)
            {
                dieSlots[i].selectButton.interactable = interactable;
            }
        }
    }
}

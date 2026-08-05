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
        [SerializeField] private int maxRerolls = 1; // bump later to allow multiple rerolls
        [SerializeField] private Color heldColor = new Color(0.4f, 0.8f, 0.4f);
        [SerializeField] private Color normalColor = Color.white;

        private bool[] _held = new bool[5];
        private int[] _lastSettledFaces = new int[5];
        private int _rerollsUsed;
        private bool _hasRolledOnce;

        public event Action OnPlayerFinishedTurn;

        public int RerollsUsed => _rerollsUsed;
        public int MaxRerolls
        {
            get => maxRerolls;
            set => maxRerolls = value;
        }

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
            }
        }

        private void OnRollClicked()
        {
            rollButton.interactable = false;
            skipButton.interactable = false;
            SetAllSelectButtonsInteractable(false);

            var targetFaces = new int[dieSlots.Length];
            for (int i = 0; i < dieSlots.Length; i++)
            {
                var slot = dieSlots[i];
                slot.heldMarker.SetActive(false);
                slot.selectButton.targetGraphic.color = normalColor;
                slot.faceLabel.text = "–";

                targetFaces[i] = _held[i] ? _lastSettledFaces[i] : UnityEngine.Random.Range(1, 7);
                _held[i] = false;
            }

            if (_hasRolledOnce)
            {
                _rerollsUsed++;
            }
            else
            {
                _hasRolledOnce = true;
            }

            rollMultipleDice.RollToTargets(targetFaces);
        }

        private void HandleDieSettled(int index, int faceValue)
        {
            _lastSettledFaces[index] = faceValue;
            dieSlots[index].faceLabel.text = faceValue.ToString();
        }

        private void HandleHandEvaluated(RollMultipleDice.PokerDiceHand hand)
        {
            skipButton.interactable = true;

            if (_rerollsUsed >= maxRerolls)
            {
                rollButton.interactable = false;
                SetAllSelectButtonsInteractable(false);
            }
            else
            {
                rollButton.interactable = true;
                SetAllSelectButtonsInteractable(true);
            }
        }

        private void OnDieButtonClicked(int index)
        {
            _held[index] = !_held[index];
            var slot = dieSlots[index];
            slot.heldMarker.SetActive(_held[index]);
            slot.selectButton.targetGraphic.color = _held[index] ? heldColor : normalColor;

            if (AllHeld())
            {
                rollButton.interactable = false;
            }
            else if (_rerollsUsed < maxRerolls)
            {
                rollButton.interactable = true;
            }
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
            }

            OnPlayerFinishedTurn?.Invoke();
        }

        public void ResetForNewTurn()
        {
            for (int i = 0; i < _held.Length; i++)
            {
                _held[i] = false;
            }

            _rerollsUsed = 0;
            _hasRolledOnce = false;

            ApplyInitialButtonState();
        }

        public void LockRollButton()
        {
            rollButton.interactable = false;
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

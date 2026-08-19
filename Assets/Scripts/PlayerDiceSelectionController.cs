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
        }

        [SerializeField] private Button rollButton;
        [SerializeField] private Button skipButton;
        [SerializeField] private DieUISlot[] dieSlots = new DieUISlot[5];
        [SerializeField] private RollMultipleDice rollMultipleDice;
        [SerializeField] private DiceRollOverride diceRollOverride;
        [SerializeField] private Color heldColor = new Color(0.4f, 0.8f, 0.4f);
        [SerializeField] private Color normalColor = Color.white;

        private bool[] _held = new bool[5];
        private int[] _lastSettledFaces = new int[5];

        private bool _cachedRollInteractable;
        private bool _cachedSkipInteractable;
        private bool[] _cachedSelectInteractable;
        private bool _hasCachedState;
        private bool isPlayerInputAllowed;
        private bool _turnFinished;

        public event Action OnPlayerFinishedTurn;

        private void OnEnable()
        {
            if (TurnAuthority.Instance == null)
            {
                Debug.LogWarning("PlayerDiceSelectionController: TurnAuthority.Instance is null in OnEnable — skipping subscription.");
                return;
            }

            TurnAuthority.Instance.OnTurnOwnerChanged += HandleTurnOwnerChanged;
            RefreshInputInteractable();
        }

        private void OnDisable()
        {
            if (TurnAuthority.Instance == null)
            {
                return;
            }

            TurnAuthority.Instance.OnTurnOwnerChanged -= HandleTurnOwnerChanged;
        }

        private void HandleTurnOwnerChanged(TurnOwner newOwner)
        {
            RefreshInputInteractable();
        }

        private bool IsPlayerTurn => MatchLaunchOptions.IsHumanControlled(TurnAuthority.Instance.CurrentOwner);

        // Stopgap cache/restore approach: a real turn-phase concept (pre-roll / selecting /
        // hand-evaluated) likely belongs in GameFlowManager once it exists, at which point this
        // caching can probably be replaced by deriving interactable state directly from that phase.
        private void RefreshInputInteractable()
        {
            bool isPlayerTurn = IsPlayerTurn;
            isPlayerInputAllowed = isPlayerTurn;

            if (_cachedSelectInteractable == null || _cachedSelectInteractable.Length != dieSlots.Length)
            {
                _cachedSelectInteractable = new bool[dieSlots.Length];
            }

            if (!isPlayerTurn)
            {
                _cachedRollInteractable = rollButton.interactable;
                _cachedSkipInteractable = skipButton.interactable;
                for (int i = 0; i < dieSlots.Length; i++)
                {
                    _cachedSelectInteractable[i] = dieSlots[i].selectButton.interactable;
                }

                _hasCachedState = true;

                rollButton.interactable = false;
                skipButton.interactable = false;
                for (int i = 0; i < dieSlots.Length; i++)
                {
                    dieSlots[i].selectButton.interactable = false;
                }
            }
            else if (_hasCachedState)
            {
                rollButton.interactable = _cachedRollInteractable;
                skipButton.interactable = _cachedSkipInteractable;
                for (int i = 0; i < dieSlots.Length; i++)
                {
                    dieSlots[i].selectButton.interactable = _cachedSelectInteractable[i];
                }
            }
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
            }

            rollMultipleDice.OnHandEvaluated += HandleHandEvaluated;
            rollMultipleDice.OnDieSettled += HandleDieSettled;

            ApplyInitialButtonState();

            if (TurnAuthority.Instance == null)
            {
                Debug.LogWarning("PlayerDiceSelectionController: TurnAuthority.Instance is null in Start — skipping initial gating.");
            }
            else
            {
                RefreshInputInteractable();
            }
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

            if (diceRollOverride == null)
            {
                Debug.LogError("PlayerDiceSelectionController: diceRollOverride is not assigned.");
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
            if (!IsPlayerTurn)
            {
                return;
            }

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

                shouldRoll[i] = !_held[i];
                if (shouldRoll[i])
                {
                    slot.faceLabel.text = "–";
                    targetFaces[i] = diceRollOverride.TryGetForcedFace(TurnOwner.Player, i, out var forced)
                        ? forced
                        : UnityEngine.Random.Range(1, 7);
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

        private void HandleHandEvaluated(PokerDiceHand hand)
        {
            if (!isPlayerInputAllowed)
            {
                return;
            }

            if (_turnFinished)
            {
                return;
            }

            skipButton.interactable = true;
        }

        public void SetDieHeldVisual(int index, bool held)
        {
            Debug.Log($"[HeldMarkerDebug] frame {Time.frameCount}: SetDieHeldVisual die {index} -> held={held}");

            var slot = dieSlots[index];
            slot.heldMarker.SetActive(held);
            slot.selectButton.targetGraphic.color = held ? heldColor : normalColor;
        }

        private void OnDieButtonClicked(int index)
        {
            if (!IsPlayerTurn)
            {
                return;
            }

            _held[index] = !_held[index];
            var slot = dieSlots[index];
            slot.heldMarker.SetActive(_held[index]);
            slot.selectButton.targetGraphic.color = _held[index] ? heldColor : normalColor;

            rollButton.interactable = !AllHeld();
        }

        private void OnSkipClicked()
        {
            FinishTurn();
        }

        public void FinishTurn()
        {
            if (!IsPlayerTurn)
            {
                return;
            }

            if (_turnFinished)
            {
                return;
            }

            _turnFinished = true;

            rollButton.interactable = false;
            skipButton.interactable = false;
            SetAllSelectButtonsInteractable(false);

            ClearAllHeldVisuals();

            OnPlayerFinishedTurn?.Invoke();
        }

        private void ClearAllHeldVisuals()
        {
            for (int i = 0; i < dieSlots.Length; i++)
            {
                var slot = dieSlots[i];
                slot.heldMarker.SetActive(false);
                slot.selectButton.targetGraphic.color = normalColor;
            }
        }

        public void ResetForNewTurn()
        {
            for (int i = 0; i < _held.Length; i++)
            {
                _held[i] = false;
            }

            _turnFinished = false;

            ClearAllHeldVisuals();
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

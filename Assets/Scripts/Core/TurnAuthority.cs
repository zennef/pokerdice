using System;
using UnityEngine;

namespace PokerDice
{
    public enum TurnOwner
    {
        Player,
        Bot
    }

    public class TurnAuthority : MonoBehaviour
    {
        public static TurnAuthority Instance { get; private set; }

        public TurnOwner CurrentOwner { get; private set; } = TurnOwner.Player;

        public event Action<TurnOwner> OnTurnOwnerChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"Duplicate TurnAuthority on '{gameObject.name}' — destroying duplicate component.");
                Destroy(this);
                return;
            }

            Instance = this;
        }

        // Does not early-out when newOwner == CurrentOwner. Once round-openers alternate,
        // consecutive rounds legitimately need to notify listeners (reroll reset, bot turn
        // kickoff) even when the raw enum value happens to match what was already set — the
        // event means "this seat should act now," not "the value changed."
        public void SetTurnOwner(TurnOwner newOwner)
        {
            CurrentOwner = newOwner;
            Debug.Log($"[TurnAuthority] Turn owner changed to: {CurrentOwner}");
            OnTurnOwnerChanged?.Invoke(CurrentOwner);
        }

#if UNITY_EDITOR
        [ContextMenu("Debug: Flip Turn Owner")]
        private void DebugFlipTurnOwner()
        {
            SetTurnOwner(CurrentOwner == TurnOwner.Player ? TurnOwner.Bot : TurnOwner.Player);
        }
#endif
    }
}

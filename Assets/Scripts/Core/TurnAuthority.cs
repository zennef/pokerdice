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

        public void SetTurnOwner(TurnOwner newOwner)
        {
            if (newOwner == CurrentOwner)
            {
                return;
            }

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

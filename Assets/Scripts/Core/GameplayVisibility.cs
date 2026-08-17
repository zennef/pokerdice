using System;
using UnityEngine;

namespace PokerDice
{
    // Intentionally a scene MonoBehaviour rather than a static field: MatchOverPopup's
    // Rematch/Main Menu buttons reload the scene while the popup is still fully shown
    // (never calling Hide()), so a static counter could get stuck non-zero across the
    // reload. A scene-instance counter is simply destroyed and recreated at 0 on every
    // reload instead, with no explicit reset needed.
    public class GameplayVisibility : MonoBehaviour
    {
        public static GameplayVisibility Instance { get; private set; }

        private int _openOverlayCount;

        public bool IsGameplayActive => _openOverlayCount == 0;

        public event Action<bool> OnGameplayActiveChanged;

        private void Awake()
        {
            Instance = this;
        }

        public void NotifyOverlayShown()
        {
            bool wasActive = _openOverlayCount == 0;
            _openOverlayCount++;

            if (wasActive)
            {
                OnGameplayActiveChanged?.Invoke(false);
            }
        }

        public void NotifyOverlayHidden()
        {
            if (_openOverlayCount > 0)
            {
                _openOverlayCount--;
            }

            if (_openOverlayCount == 0)
            {
                OnGameplayActiveChanged?.Invoke(true);
            }
        }
    }
}

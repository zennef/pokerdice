using System;
using UnityEngine;

namespace PokerDice
{
    public class DiceOutcomeSlot : MonoBehaviour
    {
        [SerializeField] private RollMultipleDice rollMultipleDice;
        [SerializeField] private int index;

        public event Action<int> OnDieSettled;

        private void OnEnable()
        {
            rollMultipleDice.OnDieSettled += HandleDieSettled;
        }

        private void OnDisable()
        {
            rollMultipleDice.OnDieSettled -= HandleDieSettled;
        }

        public void SetOutcome(float outcome)
        {
            rollMultipleDice.SetOutcome(index, (int)outcome);
        }

        private void HandleDieSettled(int settledIndex, int faceValue)
        {
            if (settledIndex == index)
            {
                OnDieSettled?.Invoke(faceValue);
            }
        }
    }
}

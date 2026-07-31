using UnityEngine;

namespace PokerDice
{
    public class DiceOutcomeSlot : MonoBehaviour
    {
        [SerializeField] private RollMultipleDice rollMultipleDice;
        [SerializeField] private int index;

        public void SetOutcome(float outcome)
        {
            rollMultipleDice.SetOutcome(index, (int)outcome);
        }
    }
}

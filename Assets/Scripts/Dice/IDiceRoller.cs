using System;

namespace PokerDice
{
    public interface IDiceRoller
    {
        event Action<int, int> OnDieSettled;
        void RollAll(int[] outcomes, Action onAllSettled);
        void RollSubset(bool[] shouldRoll, int[] outcomes, Action onAllSettled);
    }
}

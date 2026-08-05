using System;

namespace PokerDice
{
    public interface IDiceRig
    {
        event Action<int, int> OnDieSettled;
        void RollAll(int[] outcomes, Action onAllSettled);
    }
}

using System;

namespace PokerDice
{
    public interface IDiceRig
    {
        void RollAll(int[] outcomes, Action onAllSettled);
    }
}

namespace PokerDice
{
    public interface IBotHoldStrategy
    {
        bool[] DecideHolds(int[] currentFaces, int rerollsRemaining);
    }
}

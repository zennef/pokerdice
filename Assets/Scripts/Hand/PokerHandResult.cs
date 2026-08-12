using System;

namespace PokerDice
{
    public readonly struct PokerHandResult : IComparable<PokerHandResult>
    {
        public PokerDiceHand Category { get; }
        public int[] TieBreakers { get; }
        public int[] Faces { get; }

        public PokerHandResult(PokerDiceHand category, int[] tieBreakers, int[] faces)
        {
            Category = category;
            TieBreakers = tieBreakers;
            Faces = faces;
        }

        public int CompareTo(PokerHandResult other)
        {
            int categoryComparison = Category.CompareTo(other.Category);
            if (categoryComparison != 0)
            {
                return categoryComparison;
            }

            int length = Math.Min(TieBreakers.Length, other.TieBreakers.Length);
            for (int i = 0; i < length; i++)
            {
                int tieBreakerComparison = TieBreakers[i].CompareTo(other.TieBreakers[i]);
                if (tieBreakerComparison != 0)
                {
                    return tieBreakerComparison;
                }
            }

            return 0;
        }
    }
}

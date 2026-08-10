using System.Collections.Generic;
using System.Linq;

namespace PokerDice
{
    public enum PokerDiceHand
    {
        HighestNumber,
        OnePair,
        TwoPair,
        ThreeOfAKind,
        LowStraight,
        HighStraight,
        FullHouse,
        FourOfAKind,
        FiveOfAKind
    }

    public static class PokerHandEvaluator
    {
        public static PokerDiceHand Evaluate(int[] faces)
        {
            // Extract the outcome values from the array of structs
            int[] outcomes = faces;

            // Count occurrences of each die value
            var counts = outcomes.GroupBy(d => d)
                                 .ToDictionary(g => g.Key, g => g.Count());

            var groups = counts.Values.OrderByDescending(c => c).ToList();

            // Evaluate hands from highest to lowest

            if (groups.Contains(5))
            {
                return PokerDiceHand.FiveOfAKind;
            }

            if (groups.Contains(4))
            {
                return PokerDiceHand.FourOfAKind;
            }

            if (groups.SequenceEqual(new List<int> { 3, 2 }))
            {
                return PokerDiceHand.FullHouse;
            }

            if (IsHighStraight(outcomes))
            {
                return PokerDiceHand.HighStraight;
            }

            if (IsLowStraight(outcomes))
            {
                return PokerDiceHand.LowStraight;
            }

            if (groups.Contains(3))
            {
                return PokerDiceHand.ThreeOfAKind;
            }

            if (groups.Count(c => c == 2) == 2)
            {
                return PokerDiceHand.TwoPair;
            }

            if (groups.Contains(2))
            {
                return PokerDiceHand.OnePair;
            }

            return PokerDiceHand.HighestNumber;
        }

        private static bool IsHighStraight(int[] dice)
        {
            var distinctSorted = dice.Distinct().OrderBy(d => d).ToArray();
            if (distinctSorted.Length != 5)
            {
                return false;
            }

            return (distinctSorted[4] - distinctSorted[0] == 4) && (
                distinctSorted.Contains(2) &&
                distinctSorted.Contains(3) &&
                distinctSorted.Contains(4) &&
                distinctSorted.Contains(5) &&
                distinctSorted.Contains(6)
            );
        }

        private static bool IsLowStraight(int[] dice)
        {
            var distinctSorted = dice.Distinct().OrderBy(d => d).ToArray();
            if (distinctSorted.Length != 5)
            {
                return false;
            }

            return (distinctSorted[4] - distinctSorted[0] == 4) && (
                distinctSorted.Contains(1) &&
                distinctSorted.Contains(2) &&
                distinctSorted.Contains(3) &&
                distinctSorted.Contains(4) &&
                distinctSorted.Contains(5)
            );
        }
    }
}

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

        public static PokerHandResult EvaluateDetailed(int[] faces)
        {
            PokerDiceHand category = Evaluate(faces);
            int[] tieBreakers = ComputeTieBreakers(category, faces);
            return new PokerHandResult(category, tieBreakers, (int[])faces.Clone());
        }

        private static int[] ComputeTieBreakers(PokerDiceHand category, int[] faces)
        {
            var counts = faces.GroupBy(d => d)
                              .ToDictionary(g => g.Key, g => g.Count());

            switch (category)
            {
                case PokerDiceHand.FiveOfAKind:
                {
                    int quint = counts.First(kv => kv.Value == 5).Key;
                    return new[] { quint };
                }

                case PokerDiceHand.FourOfAKind:
                {
                    int quad = counts.First(kv => kv.Value == 4).Key;
                    int kicker = counts.First(kv => kv.Value == 1).Key;
                    return new[] { quad, kicker };
                }

                case PokerDiceHand.FullHouse:
                {
                    int triple = counts.First(kv => kv.Value == 3).Key;
                    int pair = counts.First(kv => kv.Value == 2).Key;
                    return new[] { triple, pair };
                }

                case PokerDiceHand.ThreeOfAKind:
                {
                    int triple = counts.First(kv => kv.Value == 3).Key;
                    var kickers = counts.Where(kv => kv.Value == 1)
                                        .Select(kv => kv.Key)
                                        .OrderByDescending(k => k);
                    return new[] { triple }.Concat(kickers).ToArray();
                }

                case PokerDiceHand.TwoPair:
                {
                    var pairs = counts.Where(kv => kv.Value == 2)
                                      .Select(kv => kv.Key)
                                      .OrderByDescending(k => k)
                                      .ToArray();
                    int kicker = counts.First(kv => kv.Value == 1).Key;
                    return new[] { pairs[0], pairs[1], kicker };
                }

                case PokerDiceHand.OnePair:
                {
                    int pair = counts.First(kv => kv.Value == 2).Key;
                    var kickers = counts.Where(kv => kv.Value == 1)
                                        .Select(kv => kv.Key)
                                        .OrderByDescending(k => k);
                    return new[] { pair }.Concat(kickers).ToArray();
                }

                case PokerDiceHand.HighStraight:
                case PokerDiceHand.LowStraight:
                case PokerDiceHand.HighestNumber:
                default:
                    return faces.OrderByDescending(f => f).ToArray();
            }
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

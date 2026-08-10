using System.Collections.Generic;

namespace PokerDice
{
    // Single-ply strategy: evaluates the expected value of only the immediate upcoming reroll.
    // It does NOT look ahead across multiple rerolls, even when rerollsRemaining > 1 — treat
    // this as a one-step heuristic, not a general multi-step solver. Recursive/multi-level
    // lookahead is intentionally out of scope for this implementation.
    public class BruteForceEvHoldStrategy : IBotHoldStrategy
    {
        public bool[] DecideHolds(int[] currentFaces, int rerollsRemaining)
        {
            int diceCount = currentFaces.Length;
            int patternCount = 1 << diceCount;

            bool[] bestHolds = null;
            long bestScoreSum = 0;
            long bestComboCount = 1;
            int bestHeldCount = -1;

            var nonHeldIndices = new List<int>(diceCount);
            int[] faces = new int[diceCount];

            for (int pattern = 0; pattern < patternCount; pattern++)
            {
                bool[] holds = new bool[diceCount];
                nonHeldIndices.Clear();

                for (int i = 0; i < diceCount; i++)
                {
                    bool held = (pattern & (1 << i)) != 0;
                    holds[i] = held;
                    faces[i] = currentFaces[i];
                    if (!held)
                    {
                        nonHeldIndices.Add(i);
                    }
                }

                int rerollCount = nonHeldIndices.Count;
                long comboCount = 1;
                for (int i = 0; i < rerollCount; i++)
                {
                    comboCount *= 6;
                }

                long scoreSum = 0;
                for (long combo = 0; combo < comboCount; combo++)
                {
                    long remaining = combo;
                    for (int j = 0; j < rerollCount; j++)
                    {
                        int digit = (int)(remaining % 6);
                        remaining /= 6;
                        faces[nonHeldIndices[j]] = digit + 1;
                    }

                    PokerDiceHand hand = PokerHandEvaluator.Evaluate(faces);
                    scoreSum += (int)hand;
                }

                int heldCount = diceCount - rerollCount;

                // Compare scoreSum/comboCount against bestScoreSum/bestComboCount without
                // floating point, via cross-multiplication, so exact ties are detected exactly.
                long lhs = scoreSum * bestComboCount;
                long rhs = bestScoreSum * comboCount;

                bool isBetter = bestHolds == null || lhs > rhs;
                bool isExactTie = bestHolds != null && lhs == rhs;
                bool prefersMoreHeld = isExactTie && heldCount > bestHeldCount;

                if (isBetter || prefersMoreHeld)
                {
                    bestHolds = holds;
                    bestScoreSum = scoreSum;
                    bestComboCount = comboCount;
                    bestHeldCount = heldCount;
                }
            }

            return bestHolds;
        }
    }
}

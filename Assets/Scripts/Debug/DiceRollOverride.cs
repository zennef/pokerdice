using System;
using UnityEngine;

namespace PokerDice
{
    [Serializable]
    public struct DiceRollOverrideEntry
    {
        public TurnOwner owner;

        // forcedFaces[i] of 1-6 forces that die's next roll for this owner; any other value
        // (0, negative, >6) means random. Values persist across rolls until manually reset —
        // there's no auto-clear.
        public int[] forcedFaces;
    }

    public class DiceRollOverride : MonoBehaviour
    {
        [SerializeField]
        private DiceRollOverrideEntry[] overrides =
        {
            new DiceRollOverrideEntry { owner = TurnOwner.Player, forcedFaces = new int[5] },
            new DiceRollOverrideEntry { owner = TurnOwner.Bot, forcedFaces = new int[5] }
        };

        public bool TryGetForcedFace(TurnOwner owner, int dieIndex, out int face)
        {
            foreach (var entry in overrides)
            {
                if (entry.owner != owner)
                {
                    continue;
                }

                if (entry.forcedFaces == null || dieIndex < 0 || dieIndex >= entry.forcedFaces.Length)
                {
                    face = 0;
                    return false;
                }

                return DiceForceUtility.TryGetForcedFace(entry.forcedFaces[dieIndex], out face);
            }

            face = 0;
            return false;
        }
    }
}

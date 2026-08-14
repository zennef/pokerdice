using UnityEngine;
using Random = UnityEngine.Random;

namespace PokerDice
{
    public static class DiceForceUtility
    {
        public static Vector3 GetRandomForce()
        {
            return new Vector3(Random.Range(-10, 10), Random.Range(10, 20), Random.Range(-10, 10));
        }

        // Shared "is this a legal forced face" rule used by both the player and bot roll paths.
        public static bool TryGetForcedFace(int rawValue, out int face)
        {
            if (rawValue >= 1 && rawValue <= 6)
            {
                face = rawValue;
                return true;
            }

            face = 0;
            return false;
        }
    }
}

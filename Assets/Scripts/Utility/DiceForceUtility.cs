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
    }
}

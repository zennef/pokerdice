using UnityEngine;

namespace PokerDice
{
    [CreateAssetMenu(fileName = "MatchSettings", menuName = "PokerDice/Match Settings")]
    public class MatchSettings : ScriptableObject
    {
        [Tooltip("Number of rerolls each player gets per round.")]
        [SerializeField] private int maxRerolls = 1;

        [Tooltip("Number of round wins required to win the match.")]
        [SerializeField] private int winThreshold = 3;

        public int MaxRerolls => maxRerolls;
        public int WinThreshold => winThreshold;

        private void OnValidate()
        {
            if (maxRerolls < 0)
            {
                maxRerolls = 0;
            }

            if (winThreshold < 1)
            {
                winThreshold = 1;
            }
        }
    }
}

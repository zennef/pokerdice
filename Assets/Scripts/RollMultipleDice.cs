using System;
using UnityEngine;

namespace PokerDice
{
    public class RollMultipleDice : MonoBehaviour
    {
        [SerializeField] private int diceCount = 5;
        [SerializeField] private MonoBehaviour diceRigSource;

        private IDiceRoller _diceRoller;
        private int[] _outcomes;

        public event Action OnRollStarted;
        public event Action<PokerDiceHand> OnHandEvaluated;
        public event Action<int, int> OnDieSettled;

        void Awake()
        {
            _diceRoller = diceRigSource as IDiceRoller;
            if (_diceRoller == null)
            {
                Debug.LogError("diceRigSource is null or does not implement IDiceRoller.");
            }
            else
            {
                _diceRoller.OnDieSettled += HandleDieSettled;
            }

            _outcomes = new int[diceCount];
            for (int i = 0; i < _outcomes.Length; i++)
            {
                _outcomes[i] = 1;
            }
        }

        public void SetOutcome(int index, int outcome)
        {
            _outcomes[index] = outcome;
        }

        public void RollAll()
        {
            OnRollStarted?.Invoke();
            _diceRoller.RollAll(_outcomes, HandleAllSettled);
        }

        public void RollToTargets(int[] targetFaces)
        {
            for (int i = 0; i < targetFaces.Length; i++)
            {
                SetOutcome(i, targetFaces[i]);
            }

            OnRollStarted?.Invoke();
            _diceRoller.RollAll(_outcomes, HandleAllSettled);
        }

        public void RollSubset(bool[] shouldRoll, int[] targetFaces)
        {
            for (int i = 0; i < targetFaces.Length; i++)
            {
                SetOutcome(i, targetFaces[i]);
            }

            OnRollStarted?.Invoke();
            _diceRoller.RollSubset(shouldRoll, _outcomes, HandleAllSettled);
        }

        private void HandleAllSettled()
        {
            OnHandEvaluated?.Invoke(EvaluateHand());
        }

        private void HandleDieSettled(int index, int faceValue)
        {
            OnDieSettled?.Invoke(index, faceValue);
        }

        public PokerDiceHand EvaluateHand() => PokerHandEvaluator.Evaluate(_outcomes);

        public PokerHandResult EvaluateDetailedHand() => PokerHandEvaluator.EvaluateDetailed(_outcomes);
    }
}

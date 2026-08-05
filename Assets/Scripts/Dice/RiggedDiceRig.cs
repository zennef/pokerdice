using System;
using PredictedDice;
using UnityEngine;
using UnityEngine.Events;

namespace PokerDice
{
    public class RiggedDiceRig : MonoBehaviour, IDiceRig
    {
        [SerializeField] private Dice[] dice;

        private int _settledCount;
        private Action _onAllSettled;
        private UnityAction<int>[] _dieSettledListeners;

        public event Action<int, int> OnDieSettled;

        private void Start()
        {
            _dieSettledListeners = new UnityAction<int>[dice.Length];
            for (int i = 0; i < dice.Length; i++)
            {
                int index = i;
                _dieSettledListeners[i] = faceValue => HandleDieSettled(index, faceValue);
                dice[i].OnRollEnd.AddListener(_dieSettledListeners[i]);
            }
        }

        private void OnDestroy()
        {
            if (_dieSettledListeners == null)
            {
                return;
            }

            for (int i = 0; i < dice.Length; i++)
            {
                if (dice[i] != null)
                {
                    dice[i].OnRollEnd.RemoveListener(_dieSettledListeners[i]);
                }
            }
        }

        public void RollAll(int[] outcomes, Action onAllSettled)
        {
            if (outcomes.Length != dice.Length)
            {
                Debug.LogError("Outcomes array length does not match dice array length.");
                return;
            }

            _settledCount = 0;
            _onAllSettled = onAllSettled;

            for (int i = 0; i < dice.Length; i++)
            {
                dice[i].RollDiceWithOutCome(GetRandomForcedRollData(outcomes[i]));
            }

            ProjectionSceneManager.Instance.Simulate();

            for (int i = 0; i < dice.Length; i++)
            {
                dice[i].PlaySimulation();
            }
        }

        private void HandleDieSettled(int index, int faceValue)
        {
            OnDieSettled?.Invoke(index, faceValue);

            _settledCount++;
            if (_settledCount >= dice.Length)
            {
                _onAllSettled?.Invoke();
            }
        }

        private RollData GetRandomForcedRollData(int outcome = RollData.RandomFace)
        {
            return new RollData
            {
                faceValue = outcome,
                force = DiceForceUtility.GetRandomForce(),
                torque = DiceForceUtility.GetRandomForce()
            };
        }
    }
}

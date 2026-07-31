using System;
using PredictedDice;
using UnityEngine;

namespace PokerDice
{
    public class RiggedDiceRig : MonoBehaviour, IDiceRig
    {
        [SerializeField] private Dice[] dice;

        private int _settledCount;
        private Action _onAllSettled;

        private void Start()
        {
            foreach (var die in dice)
            {
                die.OnRollEnd.AddListener(HandleDieSettled);
            }
        }

        private void OnDestroy()
        {
            foreach (var die in dice)
            {
                if (die != null)
                {
                    die.OnRollEnd.RemoveListener(HandleDieSettled);
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

        private void HandleDieSettled(int _)
        {
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

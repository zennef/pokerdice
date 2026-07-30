using UnityEngine;
using PokerDice;

public class GameController : MonoBehaviour
{
    [SerializeField] private RollMultipleDice rollMultipleDice;
    RollMultipleDice.PokerDiceHand hand;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (rollMultipleDice == null)
        {
            Debug.LogError($"{nameof(GameController)} on {name} needs its Roll Multiple Dice field assigned in the Inspector.");
            return;
        }

        rollMultipleDice.OnHandEvaluated += HandleHandEvaluated;
    }

    private void HandleHandEvaluated(RollMultipleDice.PokerDiceHand evaluatedHand)
    {
        hand = evaluatedHand;
        Debug.Log(hand);
    }

    private void OnDestroy()
    {
        if (rollMultipleDice != null)
        {
            rollMultipleDice.OnHandEvaluated -= HandleHandEvaluated;
        }
    }
}

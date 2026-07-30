using UnityEngine;
using PokerDice;

public class GameController : MonoBehaviour
{
    [SerializeField] private GameObject first;
    [SerializeField] private GameObject second;
    [SerializeField] private GameObject third;
    [SerializeField] private GameObject fourth;
    [SerializeField] private GameObject fifth;
    private Rigidbody rb1;
    private Rigidbody rb2;
    private Rigidbody rb3;
    private Rigidbody rb4;
    private Rigidbody rb5;
    [SerializeField] private RollMultipleDice rollMultipleDice;
    private bool isAllDiceSettled;
    RollMultipleDice.PokerDiceHand hand;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb1 = first.GetComponent<Rigidbody>();
        rb2 = second.GetComponent<Rigidbody>();
        rb3 = third.GetComponent<Rigidbody>();
        rb4 = fourth.GetComponent<Rigidbody>();
        rb5 = fifth.GetComponent<Rigidbody>();
        isAllDiceSettled = false;

        if (rollMultipleDice == null)
        {
            Debug.LogError($"{nameof(GameController)} on {name} needs its Roll Multiple Dice field assigned in the Inspector.");
            return;
        }

        rollMultipleDice.OnRollStarted += ResetForNewRoll;
    }

    public void ResetForNewRoll()
    {
        isAllDiceSettled = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(!isAllDiceSettled && rb1.IsSleeping() && rb2.IsSleeping() && rb3.IsSleeping() && rb4.IsSleeping() && rb5.IsSleeping())
        {
            isAllDiceSettled = true;
            hand = rollMultipleDice.EvaluateHand();
            Debug.Log(hand);
        }
    }
}

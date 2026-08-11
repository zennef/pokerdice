namespace PokerDice
{
    public enum RoundOutcome
    {
        PlayerWins,
        BotWins,
        Tie
    }

    public readonly struct RoundResultData
    {
        public readonly string PlayerHandName;
        public readonly string BotHandName;
        public readonly int[] PlayerDiceValues;
        public readonly int[] BotDiceValues;
        public readonly RoundOutcome Outcome;

        public RoundResultData(
            string playerHandName,
            string botHandName,
            int[] playerDiceValues,
            int[] botDiceValues,
            RoundOutcome outcome)
        {
            PlayerHandName = playerHandName;
            BotHandName = botHandName;
            PlayerDiceValues = playerDiceValues;
            BotDiceValues = botDiceValues;
            Outcome = outcome;
        }
    }
}

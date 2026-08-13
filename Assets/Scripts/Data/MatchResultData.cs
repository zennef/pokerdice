namespace PokerDice
{
    public enum MatchOutcome
    {
        PlayerWinsMatch,
        BotWinsMatch
    }

    public readonly struct MatchResultData
    {
        public readonly MatchOutcome Outcome;
        public readonly int PlayerWins;
        public readonly int BotWins;

        public MatchResultData(MatchOutcome outcome, int playerWins, int botWins)
        {
            Outcome = outcome;
            PlayerWins = playerWins;
            BotWins = botWins;
        }
    }
}

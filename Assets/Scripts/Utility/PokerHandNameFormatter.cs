using System.Text.RegularExpressions;

namespace PokerDice
{
    public static class PokerHandNameFormatter
    {
        public static string Format(PokerDiceHand hand)
        {
            return Regex.Replace(hand.ToString(), "(\\B[A-Z])", " $1");
        }
    }
}

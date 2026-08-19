namespace PokerDice
{
    public enum MatchMode
    {
        VsBot,
        Hotseat
    }

    public static class MatchLaunchOptions
    {
        // Static fields survive a SceneManager.LoadScene reload (unlike scene-instance state),
        // so this flag can carry the "skip the main menu" intent from MatchOverPopup's
        // Play Again button across the reload into MainMenuController.Start(). The same
        // mechanism means Mode survives the reload too, so a Rematch keeps the same mode
        // without re-picking it.
        public static bool SkipMenu;

        public static MatchMode Mode = MatchMode.VsBot;

        /// <summary>
        /// Centralizes the Hotseat-vs-VsBot check so gameplay scripts never compare Mode directly.
        /// </summary>
        public static bool IsHumanControlled(TurnOwner owner) => owner == TurnOwner.Player || Mode == MatchMode.Hotseat;
    }
}

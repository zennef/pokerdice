namespace PokerDice
{
    public static class MatchLaunchOptions
    {
        // Static fields survive a SceneManager.LoadScene reload (unlike scene-instance state),
        // so this flag can carry the "skip the main menu" intent from MatchOverPopup's
        // Play Again button across the reload into MainMenuController.Start().
        public static bool SkipMenu;
    }
}

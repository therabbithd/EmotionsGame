namespace EmotionsGame.Core
{
    public static class GameSceneNames
    {
        public const string MainMenu = "MainMenu";
        public const string Level1 = "Level1";
        public const string Level2 = "Level2";
        public const string Level3 = "Level3";

        public static readonly string[] Levels = { Level1, Level2, Level3 };
        public const int LevelCount = 3;

        public static string ForIndex(int index)
        {
            return Levels[UnityEngine.Mathf.Clamp(index, 0, LevelCount - 1)];
        }
    }
}

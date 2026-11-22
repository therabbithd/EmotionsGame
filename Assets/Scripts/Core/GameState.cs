using System;
using UnityEngine;

namespace EmotionsGame.Core
{
    public static class GameState
    {
        private static int currentLevelIndex;
        private static int totalScore;
        private static int levelStartScore;
        private static int lives = 3;
        private static int ammo;
        private static bool paused;
        private static bool running;

        public static event Action Changed;
        public static event Action<int> LevelChanged;
        public static event Action<int, int> LivesChanged;
        public static event Action<int> ScoreChanged;
        public static event Action<int> AmmoChanged;
        public static event Action<bool> PauseChanged;

        public static int CurrentLevelIndex => currentLevelIndex;
        public static int DisplayLevel => currentLevelIndex + 1;
        public static int TotalScore => totalScore;
        public static int LevelScore => Mathf.Max(0, totalScore - levelStartScore);
        public static int Lives => lives;
        public static int Ammo => ammo;
        public static bool Paused => paused;
        public static bool Running => running;

        public static void BeginNewRun()
        {
            currentLevelIndex = 0;
            totalScore = 0;
            levelStartScore = 0;
            lives = 3;
            ammo = 0;
            paused = false;
            running = true;
            Notify();
        }

        public static void ContinueRun(int levelIndex, int score, int savedLives)
        {
            currentLevelIndex = Mathf.Clamp(levelIndex, 0, GameSceneNames.LevelCount - 1);
            totalScore = Mathf.Max(0, score);
            levelStartScore = totalScore;
            lives = Mathf.Clamp(savedLives, 1, 3);
            ammo = 0;
            paused = false;
            running = true;
            Notify();
        }

        public static void SetLevel(int index)
        {
            currentLevelIndex = Mathf.Clamp(index, 0, GameSceneNames.LevelCount - 1);
            levelStartScore = totalScore;
            paused = false;
            running = true;
            LevelChanged?.Invoke(currentLevelIndex);
            Notify();
        }

        public static void ResetLevelScore()
        {
            totalScore = levelStartScore;
            ammo = 0;
            lives = 3;
            paused = false;
            running = true;
            Time.timeScale = 1f;
            ScoreChanged?.Invoke(totalScore);
            LivesChanged?.Invoke(lives, 3);
            AmmoChanged?.Invoke(ammo);
            Notify();
        }

        public static void AddScore(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            totalScore += amount;
            ScoreChanged?.Invoke(totalScore);
            Notify();
        }

        public static void SetLives(int value)
        {
            int next = Mathf.Clamp(value, 0, 3);
            if (next == lives)
            {
                return;
            }

            lives = next;
            LivesChanged?.Invoke(lives, 3);
            Notify();
        }

        public static void AddAmmo(int amount)
        {
            int next = Mathf.Clamp(ammo + amount, 0, 30);
            if (next == ammo)
            {
                return;
            }

            ammo = next;
            AmmoChanged?.Invoke(ammo);
            Notify();
        }

        public static bool ConsumeAmmo()
        {
            if (ammo <= 0)
            {
                return false;
            }

            ammo--;
            AmmoChanged?.Invoke(ammo);
            Notify();
            return true;
        }

        public static void SetPaused(bool value)
        {
            if (paused == value || !running)
            {
                return;
            }

            paused = value;
            Time.timeScale = value ? 0f : 1f;
            PauseChanged?.Invoke(value);
            Notify();
        }

        public static void StopRun()
        {
            running = false;
            paused = false;
            Time.timeScale = 1f;
            Notify();
        }

        public static void Notify()
        {
            Changed?.Invoke();
        }
    }
}

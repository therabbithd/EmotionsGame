using System;
using System.IO;
using UnityEngine;

namespace EmotionsGame.Save
{
    [Serializable]
    public sealed class SaveData
    {
        public int bestScore;
        public int unlockedLevel;
        public int lastLevel;
        public int lastScore;
        public int lastLives = 3;
        public int runsCompleted;
        public float sfxVolume = 0.8f;
        public string lastPlayedUtc = string.Empty;
    }

    public static class SaveService
    {
        private const string FileName = "emotions_game_save.json";
        private static SaveData data;

        public static string Path => System.IO.Path.Combine(Application.persistentDataPath, FileName);
        public static bool HasSave => data != null && data.lastPlayedUtc.Length > 0;
        public static SaveData Data => data ?? (data = LoadFromFile());

        public static SaveData Load()
        {
            data = LoadFromFile();
            return data;
        }

        public static void Save()
        {
            if (data == null)
            {
                data = new SaveData();
            }

            data.lastPlayedUtc = DateTime.UtcNow.ToString("o");
            try
            {
                File.WriteAllText(Path, JsonUtility.ToJson(data, true));
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Could not save progress: {exception.Message}");
            }
        }

        public static void RecordRunStart(int levelIndex)
        {
            SaveData current = Data;
            current.lastLevel = Mathf.Clamp(levelIndex, 0, 2);
            current.lastLives = 3;
            current.lastScore = 0;
            Save();
        }

        public static void RecordLevelComplete(int levelIndex, int score)
        {
            SaveData current = Data;
            current.unlockedLevel = Mathf.Clamp(Mathf.Max(current.unlockedLevel, levelIndex + 1), 0, 2);
            current.bestScore = Mathf.Max(current.bestScore, score);
            current.lastScore = score;
            current.lastLevel = Mathf.Clamp(levelIndex + 1, 0, 2);
            current.lastLives = 3;
            Save();
        }

        public static void RecordGameOver(int levelIndex, int score)
        {
            SaveData current = Data;
            current.bestScore = Mathf.Max(current.bestScore, score);
            current.lastScore = score;
            current.lastLevel = Mathf.Clamp(levelIndex, 0, 2);
            Save();
        }

        public static void RecordRunComplete(int score)
        {
            SaveData current = Data;
            current.runsCompleted++;
            current.bestScore = Mathf.Max(current.bestScore, score);
            current.unlockedLevel = 2;
            Save();
        }

        public static void SetSfxVolume(float volume)
        {
            Data.sfxVolume = Mathf.Clamp01(volume);
            Save();
        }

        public static void ResetAll()
        {
            data = new SaveData();
            try
            {
                if (File.Exists(Path))
                {
                    File.Delete(Path);
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Could not delete save file: {exception.Message}");
            }
        }

        private static SaveData LoadFromFile()
        {
            try
            {
                if (!File.Exists(Path))
                {
                    return new SaveData();
                }

                SaveData loaded = JsonUtility.FromJson<SaveData>(File.ReadAllText(Path));
                if (loaded == null)
                {
                    return new SaveData();
                }

                loaded.unlockedLevel = Mathf.Clamp(loaded.unlockedLevel, 0, 2);
                loaded.lastLevel = Mathf.Clamp(loaded.lastLevel, 0, 2);
                loaded.lastLives = Mathf.Clamp(loaded.lastLives, 1, 3);
                loaded.sfxVolume = Mathf.Clamp01(loaded.sfxVolume);
                return loaded;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Could not load progress, starting fresh: {exception.Message}");
                return new SaveData();
            }
        }
    }
}

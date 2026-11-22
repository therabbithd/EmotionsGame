using System;
using UnityEngine;

namespace EmotionsGame.Score
{
    /// <summary>Minimal score aggregate. Consumers subscribe to ScoreChanged.</summary>
    [DisallowMultipleComponent]
    public sealed class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        [SerializeField] private int score;

        public int Score => score;

        public event Action<int> ScoreChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            ScoreChanged?.Invoke(score);
        }

        public void AddScore(int amount)
        {
            if (amount <= 0 || Instance != this)
            {
                return;
            }

            score += amount;
            ScoreChanged?.Invoke(score);
        }

        public void ResetScore()
        {
            if (Instance != this)
            {
                return;
            }

            score = 0;
            ScoreChanged?.Invoke(score);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}

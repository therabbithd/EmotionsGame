using System;
using UnityEngine;
using EmotionsGame.Core;
using EmotionsGame.Score;

namespace EmotionsGame.Levels
{
    [DisallowMultipleComponent]
    public sealed class LevelDirector : MonoBehaviour
    {
        [SerializeField] private string displayName = "Nivel";
        [TextArea(2, 4)]
        [SerializeField] private string objective = "Derrota a los enemigos.";
        [SerializeField, Min(1)] private int targetDefeats = 8;
        [SerializeField, Min(0f)] private float timeLimit;
        [SerializeField] private bool failOnTimeout = true;

        public string DisplayName => displayName;
        public string ObjectiveText => $"{objective}\n{defeats}/{targetDefeats} derrotados";
        public int Defeats => defeats;
        public int TargetDefeats => targetDefeats;
        public float TimeRemaining => timeLimit > 0f ? Mathf.Max(0f, timeLimit - elapsed) : 0f;

        public event Action<string, float> InfoChanged;
        public event Action Completed;
        public event Action Failed;

        private int defeats;
        private float elapsed;
        private int lastScore;
        private bool running;

        public void Begin()
        {
            defeats = 0;
            elapsed = 0f;
            running = true;
            lastScore = ScoreManager.Instance != null ? ScoreManager.Instance.Score : 0;

            ScoreManager score = ScoreManager.Instance;
            if (score != null)
            {
                score.ScoreChanged += OnScoreChanged;
            }

            InfoChanged?.Invoke(ObjectiveText, TimeRemaining);
        }

        public void StopLevel()
        {
            if (!running)
            {
                return;
            }

            running = false;
            ScoreManager score = ScoreManager.Instance;
            if (score != null)
            {
                score.ScoreChanged -= OnScoreChanged;
            }
        }

        private void Update()
        {
            if (!running)
            {
                return;
            }

            if (timeLimit > 0f)
            {
                elapsed += Time.deltaTime;
                InfoChanged?.Invoke(ObjectiveText, TimeRemaining);

                if (elapsed >= timeLimit && failOnTimeout)
                {
                    running = false;
                    Failed?.Invoke();
                    return;
                }
            }

            if (defeats >= targetDefeats)
            {
                running = false;
                Completed?.Invoke();
            }
        }

        private void OnScoreChanged(int score)
        {
            if (!running || score <= lastScore)
            {
                lastScore = score;
                return;
            }

            defeats++;
            lastScore = score;
            InfoChanged?.Invoke(ObjectiveText, TimeRemaining);
        }

        private void OnDestroy()
        {
            StopLevel();
        }
    }
}

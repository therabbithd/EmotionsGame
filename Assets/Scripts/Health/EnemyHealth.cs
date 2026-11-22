using System;
using UnityEngine;
using EmotionsGame.Enemies;
using EmotionsGame.Score;

namespace EmotionsGame.Health
{
    /// <summary>
    /// Enemy health with a single-death guard, optional death feedback and score reward.
    /// Kept separate from EnemyController so pooled instances can reset cleanly.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EnemyHealth : MonoBehaviour, IDamageable
    {
        [Header("Health")]
        [SerializeField, Min(1)] private int maxHealth = 100;
        [SerializeField] private int currentHealth;
        [SerializeField, Min(0)] private int scoreValue = 10;

        [Header("Death Feedback")]
        [SerializeField] private GameObject destructionEffect;
        [SerializeField, Min(0.01f)] private float effectLifetime = 2f;
        [SerializeField] private AudioClip deathClip;

        private bool isDead;

        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;
        public int ScoreValue => scoreValue;
        public bool IsDead => isDead;

        public event Action<int> Defeated;

        private void Awake()
        {
            maxHealth = Mathf.Max(1, maxHealth);
            scoreValue = Mathf.Max(0, scoreValue);
        }

        private void OnEnable()
        {
            ResetHealth();
        }

        /// <summary>Configures the values used by ResetHealth when an instance is reused.</summary>
        public void Initialize(int health, int score)
        {
            maxHealth = Mathf.Max(1, health);
            scoreValue = Mathf.Max(0, score);
            ResetHealth();
        }

        public void ResetHealth()
        {
            isDead = false;
            currentHealth = maxHealth;
        }

        public void TakeDamage(int amount)
        {
            if (amount <= 0 || isDead)
            {
                return;
            }

            currentHealth = Mathf.Max(0, currentHealth - amount);
            if (currentHealth == 0)
            {
                Defeat();
            }
        }

        private void Defeat()
        {
            if (isDead)
            {
                return;
            }

            isDead = true;
            SpawnDestructionEffect();
            PlayDeathSound();

            // Both systems are optional: score still works with no manager in the scene.
            ScoreManager.Instance?.AddScore(scoreValue);
            Defeated?.Invoke(scoreValue);

            ReturnToPool();
        }

        private void ReturnToPool()
        {
            EnemyPool pool = EnemyPool.Instance;
            EnemyController controller = GetComponent<EnemyController>();

            if (pool != null && controller != null)
            {
                pool.Return(controller);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void SpawnDestructionEffect()
        {
            if (destructionEffect == null)
            {
                return;
            }

            GameObject effect = Instantiate(destructionEffect, transform.position, Quaternion.identity);
            Destroy(effect, effectLifetime);
        }

        private void PlayDeathSound()
        {
            if (deathClip != null)
            {
                AudioSource.PlayClipAtPoint(deathClip, transform.position);
            }
        }

        private void OnValidate()
        {
            maxHealth = Mathf.Max(1, maxHealth);
            scoreValue = Mathf.Max(0, scoreValue);
            effectLifetime = Mathf.Max(0.01f, effectLifetime);
        }
    }
}

using System;
using System.Collections;
using UnityEngine;

namespace EmotionsGame.Health
{
    /// <summary>
    /// Player lives, temporary invulnerability and blink feedback.
    /// UI and other systems subscribe to events instead of polling this component.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerHealth : MonoBehaviour, IDamageable, ILifeTarget
    {
        [Header("Lives")]
        [SerializeField, Min(1)] private int maxLives = 3;
        [SerializeField] private int currentLives;

        [Header("Invulnerability")]
        [SerializeField, Min(0f)] private float invulnerabilityDuration = 1f;
        [SerializeField, Min(0.01f)] private float blinkInterval = 0.1f;
        [SerializeField] private MeshRenderer[] blinkRenderers;

        [Header("References")]
        [SerializeField] private RollingSphere movement;

        private Coroutine invulnerabilityRoutine;
        private bool invulnerable;
        private bool dead;
        private bool renderersVisible = true;

        public int CurrentLives => currentLives;
        public int MaxLives => maxLives;
        public bool IsInvulnerable => invulnerable;
        public bool IsDead => dead;

        public event Action<int, int> LivesChanged;
        public event Action Died;

        private void Awake()
        {
            maxLives = Mathf.Max(1, maxLives);

            if (blinkRenderers == null || blinkRenderers.Length == 0)
            {
                blinkRenderers = GetComponentsInChildren<MeshRenderer>(true);
            }

            if (movement == null)
            {
                movement = GetComponent<RollingSphere>();
            }

            ResetHealth();
        }

        /// <summary>Restores full health and a clean runtime state (also for recycled instances).</summary>
        public void ResetHealth()
        {
            StopInvulnerability();
            currentLives = maxLives;
            dead = false;
            SetRenderersVisible(true);

            if (movement != null)
            {
                movement.SetMovementEnabled(true);
            }

            LivesChanged?.Invoke(currentLives, maxLives);
        }

        public void TakeDamage(int amount)
        {
            if (amount <= 0 || dead || invulnerable)
            {
                return;
            }

            currentLives = Mathf.Max(0, currentLives - amount);
            EmotionsGame.Audio.SfxService.Instance?.Play(EmotionsGame.Audio.SfxCue.Damage);
            LivesChanged?.Invoke(currentLives, maxLives);

            if (currentLives == 0)
            {
                Die();
            }
            else
            {
                invulnerabilityRoutine = StartCoroutine(InvulnerabilityRoutine());
            }
        }

        public void AddLives(int amount)
        {
            if (amount <= 0 || dead)
            {
                return;
            }

            int next = Mathf.Min(maxLives, currentLives + amount);
            if (next == currentLives)
            {
                return;
            }

            currentLives = next;
            LivesChanged?.Invoke(currentLives, maxLives);
        }

        private void Die()
        {
            dead = true;
            StopInvulnerability();
            SetRenderersVisible(true);

            // Freeze control before notifying listeners so they cannot move a dead player.
            if (movement != null)
            {
                movement.SetMovementEnabled(false);
            }

            Died?.Invoke();
        }

        private IEnumerator InvulnerabilityRoutine()
        {
            invulnerable = true;
            float elapsed = 0f;
            var wait = new WaitForSeconds(blinkInterval);

            while (elapsed < invulnerabilityDuration)
            {
                SetRenderersVisible(!renderersVisible);
                elapsed += blinkInterval;
                yield return wait;
            }

            invulnerabilityRoutine = null;
            invulnerable = false;
            SetRenderersVisible(true);
        }

        private void StopInvulnerability()
        {
            if (invulnerabilityRoutine != null)
            {
                StopCoroutine(invulnerabilityRoutine);
                invulnerabilityRoutine = null;
            }

            invulnerable = false;
            SetRenderersVisible(true);
        }

        private void SetRenderersVisible(bool visible)
        {
            renderersVisible = visible;
            if (blinkRenderers == null)
            {
                return;
            }

            foreach (MeshRenderer renderer in blinkRenderers)
            {
                if (renderer != null)
                {
                    renderer.enabled = visible;
                }
            }
        }

        private void OnDisable()
        {
            StopInvulnerability();
        }

        private void OnValidate()
        {
            maxLives = Mathf.Max(1, maxLives);
            currentLives = Mathf.Clamp(currentLives, 0, maxLives);
            invulnerabilityDuration = Mathf.Max(0f, invulnerabilityDuration);
            blinkInterval = Mathf.Max(0.01f, blinkInterval);
        }
    }
}

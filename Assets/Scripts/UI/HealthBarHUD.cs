using UnityEngine;
using UnityEngine.UI;
using EmotionsGame.Health;

namespace EmotionsGame.UI
{
    [DisallowMultipleComponent]
    public sealed class HealthBarHUD : MonoBehaviour
    {
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private Image fillImage;
        [SerializeField] private Slider slider;

        private bool subscribed;

        private void Awake()
        {
            if (playerHealth == null)
            {
                playerHealth = FindAnyObjectByType<PlayerHealth>();
            }
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            if (subscribed || playerHealth == null)
            {
                if (playerHealth != null)
                {
                    Sync(playerHealth.CurrentLives, playerHealth.MaxLives);
                }

                return;
            }

            playerHealth.LivesChanged += HandleLivesChanged;
            subscribed = true;
            Sync(playerHealth.CurrentLives, playerHealth.MaxLives);
        }

        private void Unsubscribe()
        {
            if (!subscribed || playerHealth == null)
            {
                subscribed = false;
                return;
            }

            playerHealth.LivesChanged -= HandleLivesChanged;
            subscribed = false;
        }

        private void HandleLivesChanged(int currentLives, int maxLives)
        {
            Sync(currentLives, maxLives);
        }

        private void Sync(int currentLives, int maxLives)
        {
            int safeMax = Mathf.Max(1, maxLives);
            int safeCurrent = Mathf.Clamp(currentLives, 0, safeMax);

            if (slider != null)
            {
                slider.minValue = 0f;
                slider.maxValue = safeMax;
                slider.value = safeCurrent;
            }

            if (fillImage != null)
            {
                fillImage.fillAmount = (float)safeCurrent / safeMax;
            }
        }

        private void OnValidate()
        {
            if (slider != null)
            {
                slider.minValue = 0f;
                if (slider.maxValue < 1f)
                {
                    slider.maxValue = 1f;
                }

                slider.value = Mathf.Clamp(slider.value, slider.minValue, slider.maxValue);
            }

            if (fillImage != null)
            {
                fillImage.fillAmount = Mathf.Clamp01(fillImage.fillAmount);
            }
        }
    }
}

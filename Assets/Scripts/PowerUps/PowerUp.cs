using System;
using DG.Tweening;
using UnityEngine;
using EmotionsGame.Audio;
using EmotionsGame.Core;
using EmotionsGame.Health;

namespace EmotionsGame.PowerUps
{
    /// <summary>
    /// Trigger pickup with a self-contained idle animation. Owns its two tweens so it
    /// never kills animations belonging to other components, and is safe to recycle.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PowerUp : MonoBehaviour
    {
        [Header("Pickup")]
        [SerializeField] private PowerUpType powerUpType = PowerUpType.Life;
        [SerializeField, Min(1)] private int amount = 1;
        [SerializeField] private string playerTag = "Player";

        [Header("Idle Animation (DOTween)")]
        [SerializeField] private Vector3 rotationEuler = new Vector3(0f, 360f, 0f);
        [SerializeField, Min(0.01f)] private float rotationDuration = 4f;
        [SerializeField, Min(0f)] private float bobHeight = 0.25f;
        [SerializeField, Min(0.01f)] private float bobDuration = 2f;
        [SerializeField] private Ease ease = Ease.InOutSine;
        [SerializeField] private LoopType loopType = LoopType.Restart;

        private Tween rotationTween;
        private Tween bobTween;
        private Vector3 baseLocalPosition;
        private Vector3 baseLocalEulerAngles;

        public PowerUpType Type => powerUpType;
        public int Amount => amount;

        public event Action<PowerUpType, int, GameObject> Collected;

        private void Awake()
        {
            CaptureBaseState();
        }

        private void OnEnable()
        {
            Stop();
            RestoreBaseState();

            rotationTween = transform.DOLocalRotate(
                    baseLocalEulerAngles + rotationEuler, rotationDuration, RotateMode.FastBeyond360)
                .SetEase(ease)
                .SetLoops(-1, loopType);

            // Bobbing must always return to the base height, hence Yoyo here.
            bobTween = transform.DOLocalMove(baseLocalPosition + Vector3.up * bobHeight, bobDuration)
                .SetEase(ease)
                .SetLoops(-1, LoopType.Yoyo);
        }

        private void OnDisable()
        {
            Stop();
            RestoreBaseState();
        }

        private void OnDestroy()
        {
            Stop();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!string.IsNullOrEmpty(playerTag) && !other.CompareTag(playerTag))
            {
                return;
            }

            if (powerUpType == PowerUpType.Life)
            {
                ILifeTarget target = other.GetComponentInParent<ILifeTarget>();
                if (target != null)
                {
                    target.AddLives(amount);
                    PlayerHealth health = other.GetComponentInParent<PlayerHealth>();
                    if (health != null)
                    {
                        GameState.SetLives(health.CurrentLives);
                    }
                }
            }
            else if (powerUpType == PowerUpType.Ammo)
            {
                GameState.AddAmmo(amount);
            }

            SfxService.Instance?.Play(SfxCue.Pickup);
            Collected?.Invoke(powerUpType, amount, gameObject);
            gameObject.SetActive(false);
        }

        /// <summary>Re-captures the current local transform as the animation base.</summary>
        /// <remarks>Call after a pool/chunk repositions the object and before activating it.</remarks>
        public void CaptureBaseState()
        {
            Stop();
            baseLocalPosition = transform.localPosition;
            baseLocalEulerAngles = transform.localEulerAngles;
        }

        /// <summary>Kills only this component's tweens.</summary>
        public void Stop()
        {
            rotationTween?.Kill();
            rotationTween = null;
            bobTween?.Kill();
            bobTween = null;
        }

        private void RestoreBaseState()
        {
            transform.localPosition = baseLocalPosition;
            transform.localEulerAngles = baseLocalEulerAngles;
        }

        private void OnValidate()
        {
            amount = Mathf.Max(1, amount);
            rotationDuration = Mathf.Max(0.01f, rotationDuration);
            bobDuration = Mathf.Max(0.01f, bobDuration);
            bobHeight = Mathf.Max(0f, bobHeight);
        }
    }
}

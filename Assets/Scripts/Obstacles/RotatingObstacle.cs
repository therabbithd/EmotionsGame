using DG.Tweening;
using UnityEngine;

namespace EmotionsGame.Obstacles
{
    /// <summary>
    /// Continuously spins a GameObject in local space. Use full 360-degree turns with
    /// <see cref="LoopType.Restart"/> for a seamless loop.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RotatingObstacle : ObstacleTweenBase
    {
        [Header("Local Rotation (DOTween)")]
        [SerializeField] private Vector3 rotationEuler = new Vector3(0f, 360f, 0f);
        [SerializeField, Min(0.01f)] private float duration = 4f;
        [SerializeField, Min(0f)] private float delay = 0f;
        [SerializeField] private Ease ease = Ease.Linear;
        [SerializeField] private LoopType loopType = LoopType.Restart;

        private Quaternion baseLocalRotation;
        private Vector3 baseLocalEulerAngles;

        protected override void CaptureBaseStateCore()
        {
            baseLocalRotation = transform.localRotation;
            // Quaternion -> Euler is canonicalized (270 vs -90); cache the euler values
            // so every recycle starts the tween from the exact same numbers.
            baseLocalEulerAngles = transform.localEulerAngles;
        }

        protected override void RestoreBaseStateCore()
        {
            transform.localRotation = baseLocalRotation;
            transform.localEulerAngles = baseLocalEulerAngles;
        }

        protected override Tween CreateTween()
        {
            return transform.DOLocalRotate(baseLocalEulerAngles + rotationEuler, duration, RotateMode.FastBeyond360)
                .SetDelay(delay)
                .SetEase(ease)
                .SetLoops(-1, loopType);
        }

        private void OnValidate()
        {
            duration = Mathf.Max(0.01f, duration);
            delay = Mathf.Max(0f, delay);
        }
    }
}

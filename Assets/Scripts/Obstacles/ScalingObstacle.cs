using DG.Tweening;
using UnityEngine;

namespace EmotionsGame.Obstacles
{
    /// <summary>
    /// Pulses a GameObject between its authored local scale and <see cref="targetScale"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ScalingObstacle : ObstacleTweenBase
    {
        [Header("Local Scale (DOTween)")]
        [SerializeField] private Vector3 targetScale = new Vector3(1.25f, 1.25f, 1.25f);
        [SerializeField, Min(0.01f)] private float duration = 2f;
        [SerializeField, Min(0f)] private float delay = 0f;
        [SerializeField] private Ease ease = Ease.InOutSine;
        [SerializeField] private LoopType loopType = LoopType.Yoyo;

        private Vector3 baseLocalScale;

        protected override void CaptureBaseStateCore()
        {
            baseLocalScale = transform.localScale;
        }

        protected override void RestoreBaseStateCore()
        {
            transform.localScale = baseLocalScale;
        }

        protected override Tween CreateTween()
        {
            return transform.DOScale(targetScale, duration)
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

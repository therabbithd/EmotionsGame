using DG.Tweening;
using UnityEngine;

namespace EmotionsGame.Obstacles
{
    /// <summary>
    /// Ping-pongs a GameObject between its authored local position and base + <see cref="movement"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MovingObstacle : ObstacleTweenBase
    {
        [Header("Local Movement (DOTween)")]
        [SerializeField] private Vector3 movement = new Vector3(0f, 0f, 2f);
        [SerializeField, Min(0.01f)] private float duration = 2f;
        [SerializeField, Min(0f)] private float delay = 0f;
        [SerializeField] private Ease ease = Ease.InOutSine;
        [SerializeField] private LoopType loopType = LoopType.Yoyo;

        private Vector3 baseLocalPosition;

        protected override void CaptureBaseStateCore()
        {
            baseLocalPosition = transform.localPosition;
        }

        protected override void RestoreBaseStateCore()
        {
            transform.localPosition = baseLocalPosition;
        }

        protected override Tween CreateTween()
        {
            // Local space keeps the tween independent of where a pool/chunk moves the parent.
            return transform.DOLocalMove(baseLocalPosition + movement, duration)
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

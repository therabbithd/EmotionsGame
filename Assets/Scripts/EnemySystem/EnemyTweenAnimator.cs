using DG.Tweening;
using UnityEngine;

namespace EmotionsGame.Enemies
{
    [DisallowMultipleComponent]
    public sealed class EnemyTweenAnimator : MonoBehaviour
    {
        private Tween floatTween;
        private Tween rotationTween;
        private Vector3 restingLocalPosition;

        public void Play(EnemyConfigSO config)
        {
            Stop();
            if (config == null)
            {
                return;
            }

            restingLocalPosition = transform.localPosition;
            float targetY = restingLocalPosition.y + config.FloatAmplitude;

            floatTween = transform.DOLocalMoveY(targetY, config.FloatDuration)
                .SetDelay(config.FloatDelay)
                .SetEase(config.FloatEase)
                .SetLoops(-1, LoopType.Yoyo);

            Vector3 axis = config.RotationAxis.normalized;
            Vector3 targetRotation = config.FullTurn ? axis * 360f : axis;

            rotationTween = transform.DORotate(targetRotation, config.RotationDuration, RotateMode.FastBeyond360)
                .SetDelay(config.RotationDelay)
                .SetEase(config.RotationEase)
                .SetLoops(-1, LoopType.Incremental);
        }

        public void Stop()
        {
            floatTween?.Kill();
            rotationTween?.Kill();
            floatTween = null;
            rotationTween = null;
        }

        private void OnDisable()
        {
            Stop();
        }

        private void OnDestroy()
        {
            Stop();
        }
    }
}

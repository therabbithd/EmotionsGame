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
        private Quaternion restingLocalRotation;
        private bool hasBaseState;

        public void Play(EnemyConfigSO config)
        {
            Stop();
            if (config == null)
            {
                return;
            }

            restingLocalPosition = transform.localPosition;
            restingLocalRotation = transform.localRotation;
            hasBaseState = true;
            float targetY = restingLocalPosition.y + config.FloatAmplitude;

            floatTween = transform.DOLocalMoveY(targetY, config.FloatDuration)
                .SetDelay(config.FloatDelay)
                .SetEase(config.FloatEase)
                .SetLoops(-1, LoopType.Yoyo);

            if (!HasFixedRotation())
            {
                Vector3 axis = config.RotationAxis.normalized;
                Vector3 targetRotation = config.FullTurn ? axis * 360f : axis;

                rotationTween = transform.DORotate(targetRotation, config.RotationDuration, RotateMode.FastBeyond360)
                    .SetDelay(config.RotationDelay)
                    .SetEase(config.RotationEase)
                    .SetLoops(-1, LoopType.Incremental);
            }
        }

        public void Stop()
        {
            floatTween?.Kill();
            rotationTween?.Kill();
            floatTween = null;
            rotationTween = null;
        }

        public void StopAndReset()
        {
            Stop();
            if (!hasBaseState)
            {
                return;
            }

            transform.localPosition = restingLocalPosition;
            if (!HasFixedRotation())
            {
                transform.localRotation = restingLocalRotation;
            }
        }

        private bool HasFixedRotation()
        {
            EnemyDimensionRotation dimensionRotation = GetComponent<EnemyDimensionRotation>();
            return dimensionRotation != null && dimensionRotation.enabled;
        }

        private void OnDisable()
        {
            StopAndReset();
        }

        private void OnDestroy()
        {
            Stop();
        }
    }
}

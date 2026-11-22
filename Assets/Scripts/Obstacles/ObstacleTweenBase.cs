using DG.Tweening;
using UnityEngine;

namespace EmotionsGame.Obstacles
{
    /// <summary>
    /// Base lifecycle for recyclable DOTween obstacle components.
    /// Each derived component owns a single <see cref="Tween"/>, so killing it never
    /// touches tweens created by other components on the same GameObject.
    /// </summary>
    public abstract class ObstacleTweenBase : MonoBehaviour
    {
        private Tween tween;
        private bool hasBaseState;

        protected virtual void Awake()
        {
            CaptureBaseState();
        }

        protected virtual void OnEnable()
        {
            // OnDisable restores the pose, but a manual Stop() or a re-enabled
            // component can leave residual values: always rebuild from the base.
            Stop();
            RestoreBaseState();
            tween = CreateTween();
        }

        protected virtual void OnDisable()
        {
            Stop();
            RestoreBaseState();
        }

        protected virtual void OnDestroy()
        {
            Stop();
        }

        /// <summary>
        /// Re-captures the current local transform as the design base state.
        /// Call from a pool/chunk after positioning the object and before activating it.
        /// </summary>
        public void CaptureBaseState()
        {
            Stop();
            CaptureBaseStateCore();
            hasBaseState = true;
        }

        /// <summary>Kills only this component's tween and restores the authored pose.</summary>
        public void StopAndReset()
        {
            Stop();
            RestoreBaseState();
        }

        /// <summary>Restarts this component's tween in place (useful after a runtime tweak).</summary>
        public void Restart()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            Stop();
            RestoreBaseState();
            tween = CreateTween();
        }

        /// <summary>Kills only this component's tween; other components keep theirs.</summary>
        public void Stop()
        {
            tween?.Kill();
            tween = null;
        }

        private void RestoreBaseState()
        {
            if (hasBaseState)
            {
                RestoreBaseStateCore();
            }
        }

        protected abstract void CaptureBaseStateCore();
        protected abstract void RestoreBaseStateCore();
        protected abstract Tween CreateTween();
    }
}

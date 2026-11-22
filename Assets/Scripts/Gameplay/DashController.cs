using System.Collections;
using System.Reflection;
using UnityEngine;
using EmotionsGame.Audio;
using EmotionsGame.Input;

namespace EmotionsGame.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class DashController : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float dashSpeed = 18f;
        [SerializeField, Min(1f)] private float dashAcceleration = 70f;
        [SerializeField, Min(0.05f)] private float dashDuration = 0.18f;
        [SerializeField, Min(0.1f)] private float cooldown = 1.2f;

        private RollingSphere rolling;
        private Rigidbody body;
        private FieldInfo speedField;
        private FieldInfo accelerationField;
        private float baseSpeed;
        private float baseAcceleration;
        private float nextDashTime;
        private Coroutine dashRoutine;
        private bool boosting;

        private void Awake()
        {
            rolling = GetComponent<RollingSphere>();
            body = GetComponent<Rigidbody>();

            if (rolling != null)
            {
                const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
                speedField = typeof(RollingSphere).GetField("maxSpeed", flags);
                accelerationField = typeof(RollingSphere).GetField("acceleration", flags);
                if (speedField != null) baseSpeed = Mathf.Max(1f, (float)speedField.GetValue(rolling));
                if (accelerationField != null) baseAcceleration = Mathf.Max(1f, (float)accelerationField.GetValue(rolling));
            }
        }

        public bool TryDash()
        {
            if (Time.time < nextDashTime || boosting)
            {
                return false;
            }

            nextDashTime = Time.time + cooldown;
            boosting = true;

            Vector2 input = GameInput.Instance != null ? GameInput.Instance.Move : Vector2.zero;
            Vector3 direction = transform.forward;
            if (input.sqrMagnitude > 0.01f)
            {
                direction = new Vector3(input.x, 0f, input.y).normalized;
            }

            if (speedField != null) speedField.SetValue(rolling, dashSpeed);
            if (accelerationField != null) accelerationField.SetValue(rolling, dashAcceleration);
            if (body != null && rolling == null)
            {
                body.AddForce(direction * dashSpeed, ForceMode.Impulse);
            }

            dashRoutine = StartCoroutine(DashRoutine());
            SfxService.Instance?.Play(SfxCue.Dash);
            return true;
        }

        private IEnumerator DashRoutine()
        {
            yield return new WaitForSeconds(dashDuration);
            Restore();
        }

        private void Restore()
        {
            if (dashRoutine != null)
            {
                StopCoroutine(dashRoutine);
                dashRoutine = null;
            }

            if (speedField != null && rolling != null) speedField.SetValue(rolling, baseSpeed);
            if (accelerationField != null && rolling != null) accelerationField.SetValue(rolling, baseAcceleration);
            boosting = false;
        }

        private void OnDisable()
        {
            Restore();
        }

        private void OnDestroy()
        {
            Restore();
        }
    }
}

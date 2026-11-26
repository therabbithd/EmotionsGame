using System.Collections;
using UnityEngine;

namespace EmotionsGame.Enemies
{
    [DisallowMultipleComponent]
    public sealed class EnemyMotion : MonoBehaviour
    {
        private enum Mode
        {
            Idle,
            Patrol,
            Chase,
            Retreat
        }

        private EnemyConfigSO config;
        private Transform target;
        private Coroutine routine;
        private Vector3 anchor;
        private Mode currentMode;
        private float chaseCooldownUntil;

        public void Initialize(EnemyConfigSO enemyConfig, Transform targetTransform)
        {
            config = enemyConfig;
            target = targetTransform;
            anchor = transform.position;
            chaseCooldownUntil = 0f;
            SwitchTo(Mode.Patrol);
        }

        public void SetTarget(Transform targetTransform)
        {
            target = targetTransform;
        }

        public void SetAggressive(bool aggressive)
        {
            if (config == null || currentMode == Mode.Retreat)
            {
                return;
            }

            bool shouldChase = aggressive && config.Behavior == EnemyConfigSO.ActivationBehavior.Chase &&
                               Time.time >= chaseCooldownUntil;
            if (shouldChase)
            {
                anchor = transform.position;
            }

            SwitchTo(shouldChase ? Mode.Chase : Mode.Patrol);
        }

        public void NotifyHit()
        {
            if (config == null || currentMode == Mode.Retreat)
            {
                return;
            }

            chaseCooldownUntil = Time.time + config.PostHitCooldown;
            SwitchTo(Mode.Retreat);
        }

        public void Shutdown()
        {
            if (routine != null)
            {
                StopCoroutine(routine);
                routine = null;
            }

            currentMode = Mode.Idle;
        }

        private void SwitchTo(Mode next)
        {
            if (currentMode == next)
            {
                return;
            }

            if (routine != null)
            {
                StopCoroutine(routine);
                routine = null;
            }

            currentMode = next;
            switch (next)
            {
                case Mode.Chase:
                    routine = StartCoroutine(Chase());
                    break;
                case Mode.Retreat:
                    routine = StartCoroutine(Retreat());
                    break;
                default:
                    routine = StartCoroutine(Patrol());
                    break;
            }
        }

        private IEnumerator Patrol()
        {
            while (true)
            {
                if (Time.time >= chaseCooldownUntil && target != null &&
                    config.Behavior == EnemyConfigSO.ActivationBehavior.Chase &&
                    Vector3.Distance(transform.position, target.position) <= config.DetectionRadius)
                {
                    SwitchTo(Mode.Chase);
                    yield break;
                }

                Vector2 offset = Random.insideUnitCircle * config.PatrolRadius;
                Vector3 destination = anchor + new Vector3(offset.x, 0f, offset.y);

                while (FlatDistance(transform.position, destination) > config.WaypointReachDistance)
                {
                    TranslateTowards(destination, config.PatrolSpeed);
                    yield return null;
                }

                yield return new WaitForSeconds(Random.Range(config.MinPause, config.MaxPause));
            }
        }

        private IEnumerator Chase()
        {
            while (true)
            {
                if (target != null)
                {
                    TranslateTowards(target.position, config.ChaseSpeed);
                }

                yield return null;
            }
        }

        private IEnumerator Retreat()
        {
            Vector3 fallbackDirection = anchor - transform.position;
            if (fallbackDirection.sqrMagnitude < 0.0001f)
            {
                fallbackDirection = Vector3.back;
            }
            fallbackDirection.Normalize();

            float elapsed = 0f;
            while (elapsed < config.RetreatDuration)
            {
                Vector3 direction = target != null ? transform.position - target.position : fallbackDirection;
                if (direction.sqrMagnitude < 0.0001f)
                {
                    direction = fallbackDirection;
                }

                Vector3 step = direction.normalized * (config.RetreatSpeed * Time.deltaTime);
                step.y = 0f;
                transform.position += step;

                elapsed += Time.deltaTime;
                yield return null;
            }

            SwitchTo(Mode.Patrol);
        }

        private void TranslateTowards(Vector3 destination, float speed)
        {
            Vector3 delta = destination - transform.position;
            delta.y = 0f;

            if (delta.sqrMagnitude < 0.0001f)
            {
                return;
            }

            Vector3 step = delta.normalized * (speed * Time.deltaTime);
            Vector3 position = transform.position;
            position.x += step.x;
            position.z += step.z;
            transform.position = position;
        }

        private static float FlatDistance(Vector3 from, Vector3 to)
        {
            from.y = 0f;
            to.y = 0f;
            return Vector3.Distance(from, to);
        }

        private void OnDisable()
        {
            Shutdown();
        }

        private void OnDestroy()
        {
            Shutdown();
        }
    }
}

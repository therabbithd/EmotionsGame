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
            Chase
        }

        private EnemyConfigSO config;
        private Transform target;
        private Coroutine routine;
        private Vector3 anchor;
        private Mode currentMode;

        public void Initialize(EnemyConfigSO enemyConfig, Transform targetTransform)
        {
            config = enemyConfig;
            target = targetTransform;
            anchor = transform.position;
            SwitchTo(Mode.Patrol);
        }

        public void SetAggressive(bool aggressive)
        {
            if (config == null)
            {
                return;
            }

            bool shouldChase = aggressive && config.Behavior == EnemyConfigSO.ActivationBehavior.Chase;
            if (shouldChase)
            {
                anchor = transform.position;
            }

            SwitchTo(shouldChase ? Mode.Chase : Mode.Patrol);
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
            routine = StartCoroutine(next == Mode.Chase ? Chase() : Patrol());
        }

        private IEnumerator Patrol()
        {
            while (true)
            {
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

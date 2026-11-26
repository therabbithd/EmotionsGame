using UnityEngine;

namespace EmotionsGame.Enemies
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EnemyTweenAnimator), typeof(EnemyMotion))]
    public sealed class EnemyController : MonoBehaviour, IDamageable, EmotionsGame.Health.IDamageDealerHitHandler
    {
        [SerializeField] private string targetTag = "Player";

        private EnemyTweenAnimator tweenAnimator;
        private EnemyMotion motion;
        private EnemyConfigSO config;
        private Transform target;
        private float health;
        private bool active;
        private bool aggressive;

        public int Score => config != null ? config.Score : 0;

        private void Awake()
        {
            tweenAnimator = GetComponent<EnemyTweenAnimator>();
            motion = GetComponent<EnemyMotion>();
        }

        public void Initialize(EnemyConfigSO enemyConfig, Transform assignedTarget)
        {
            config = enemyConfig;
            if (config == null)
            {
                Debug.LogError($"Enemy '{gameObject.name}' needs an EnemyConfigSO.", this);
                return;
            }

            health = config.MaxHealth;
            active = true;
            aggressive = false;
            target = TargetResolver.Resolve(assignedTarget, targetTag, ref target);

            tweenAnimator.Play(config);
            motion.Initialize(config, target);
        }

        private void Update()
        {
            if (!active || config == null)
            {
                return;
            }

            if (target == null)
            {
                target = TargetResolver.Resolve(null, targetTag, ref target);
                if (target != null)
                {
                    motion.SetTarget(target);
                }
            }

            if (target == null)
            {
                return;
            }

            float distance = Vector3.Distance(transform.position, target.position);
            if (distance <= config.AttackDistance)
            {
                motion.NotifyHit();
                return;
            }

            float detection = config.DetectionRadius;
            float release = detection * config.ReleaseRadiusMultiplier;

            if (!aggressive && distance <= detection)
            {
                aggressive = true;
                motion.SetAggressive(true);
            }
            else if (aggressive && distance > release)
            {
                aggressive = false;
                motion.SetAggressive(false);
            }
        }

        public void TakeDamage(float amount)
        {
            if (!active || amount <= 0f)
            {
                return;
            }

            health -= amount;
            if (health <= 0f)
            {
                Defeat();
            }
        }

        public void OnDealtDamage()
        {
            if (!active)
            {
                return;
            }

            motion.NotifyHit();
        }

        private void Defeat()
        {
            active = false;
            aggressive = false;
            StopRuntime();
            SpawnDestructionEffect();

            EnemyPool pool = EnemyPool.Instance;
            if (pool != null)
            {
                pool.Return(this);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void SpawnDestructionEffect()
        {
            if (config != null && config.DestructionEffect != null)
            {
                Instantiate(config.DestructionEffect, transform.position, Quaternion.identity);
            }
        }

        private void StopRuntime()
        {
            tweenAnimator.StopAndReset();
            motion.Shutdown();
        }

        private void OnDisable()
        {
            active = false;
            aggressive = false;
            StopRuntime();
        }
    }
}

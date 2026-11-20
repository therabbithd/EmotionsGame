using System.Collections;
using UnityEngine;

namespace EmotionsGame.Enemies
{
    [DisallowMultipleComponent]
    public sealed class EnemySpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private EnemyConfigSO[] enemyTypes;
        [SerializeField] private Transform assignedTarget;
        [SerializeField] private string targetTag = "Player";
        [SerializeField] private Transform[] spawnPoints;

        [Header("Activation")]
        [SerializeField, Min(0f)] private float initialDelay = 2f;
        [SerializeField, Min(0f)] private float activationDistance = 15f;

        [Header("Waves")]
        [SerializeField, Min(1)] private int enemiesPerWave = 3;
        [SerializeField, Min(0)] private int waveIncrement = 1;
        [SerializeField, Min(0f)] private float wavePause = 4f;

        [Header("Spawn Timing & Spread")]
        [SerializeField, Min(0f)] private float spawnInterval = 0.6f;
        [SerializeField, Min(0.05f)] private float minimumSpawnInterval = 0.15f;
        [SerializeField, Min(0f)] private float intervalReductionPerWave = 0.05f;
        [SerializeField, Min(0f)] private float spawnRadius = 5f;

        private Transform target;
        private Coroutine spawnRoutine;
        private int waveIndex;

        private void Start()
        {
            target = TargetResolver.Resolve(assignedTarget, targetTag, ref target);
            spawnRoutine = StartCoroutine(SpawnLoop());
        }

        private IEnumerator SpawnLoop()
        {
            yield return new WaitForSeconds(initialDelay);

            while (true)
            {
                if (target == null)
                {
                    target = TargetResolver.Resolve(null, targetTag, ref target);
                }

                if (target != null && Vector3.Distance(transform.position, target.position) <= activationDistance)
                {
                    yield return SpawnWave();
                    yield return new WaitForSeconds(wavePause);
                }
                else
                {
                    yield return null;
                }
            }
        }

        private IEnumerator SpawnWave()
        {
            int count = enemiesPerWave + waveIncrement * waveIndex;
            float interval = Mathf.Max(minimumSpawnInterval, spawnInterval - intervalReductionPerWave * waveIndex);
            waveIndex++;

            for (int i = 0; i < count; i++)
            {
                SpawnOne();
                yield return new WaitForSeconds(interval);
            }
        }

        private void SpawnOne()
        {
            if (enemyTypes == null || enemyTypes.Length == 0)
            {
                return;
            }

            EnemyPool pool = EnemyPool.Instance;
            if (pool == null)
            {
                return;
            }

            EnemyConfigSO config = enemyTypes[Random.Range(0, enemyTypes.Length)];
            EnemyController enemy = pool.Get(GetSpawnPosition(), Quaternion.identity);

            if (enemy != null)
            {
                enemy.Initialize(config, target);
            }
        }

        private Vector3 GetSpawnPosition()
        {
            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
                return point != null ? point.position : transform.position;
            }

            Vector2 offset = Random.insideUnitCircle * spawnRadius;
            return transform.position + new Vector3(offset.x, 0f, offset.y);
        }

        private void OnDisable()
        {
            StopSpawnRoutine();
        }

        private void OnDestroy()
        {
            StopSpawnRoutine();
        }

        private void StopSpawnRoutine()
        {
            if (spawnRoutine == null)
            {
                return;
            }

            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }
}

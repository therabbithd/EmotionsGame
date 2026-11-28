using DG.Tweening;
using UnityEngine;
using EmotionsGame.Enemies;

namespace EmotionsGame.Obstacles
{
    /// <summary>
    /// Block that spawns a random enemy or a friendly shield pickup when touched by the player.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LuckyBlock : MonoBehaviour
    {
        [Header("Activation")]
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private bool destroyAfterUse = true;
        [SerializeField, Min(0f)] private float destroyDelay = 0f;

        [Header("Spawn")]
        [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 1f, 0f);
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private EnemyController enemyPrefab;
        [SerializeField] private EnemyConfigSO enemyConfig;
        [SerializeField] private bool useEnemyPoolIfAvailable = true;
        [SerializeField] private GameObject friendlyEnemyPrefab;

        [Header("Visual")]
        [SerializeField] private bool playOpenAnimation = true;
        [SerializeField, Min(0.01f)] private float openPunchDuration = 0.2f;
        [SerializeField] private Vector3 openPunchScale = new Vector3(0.2f, 0.2f, 0.2f);
        [SerializeField, Min(0)] private int openPunchVibrato = 8;
        [SerializeField, Range(0f, 1f)] private float openPunchElasticity = 0.8f;

        private bool opened;
        private Tween openTween;

        private void OnTriggerEnter(Collider other)
        {
            TryOpen(other != null ? other.gameObject : null);
        }

        private void OnCollisionEnter(Collision collision)
        {
            TryOpen(collision != null ? collision.gameObject : null);
        }

        private void TryOpen(GameObject other)
        {
            if (opened || other == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(playerTag) && !other.CompareTag(playerTag))
            {
                return;
            }

            opened = true;
            PlayOpenAnimation();
            SpawnReward(other.transform);

            if (destroyAfterUse)
            {
                if (destroyDelay > 0f)
                {
                    Destroy(gameObject, destroyDelay);
                }
                else
                {
                    Destroy(gameObject);
                }
            }
            else
            {
                enabled = false;
            }
        }

        private void SpawnReward(Transform playerTransform)
        {
            int outcome = Random.Range(0, 2);
            Vector3 position = (spawnPoint != null ? spawnPoint.position : transform.position) + spawnOffset;
            Quaternion rotation = Quaternion.identity;

            if (outcome == 0)
            {
                SpawnEnemy(position, rotation, playerTransform);
            }
            else
            {
                SpawnFriendly(position, rotation, playerTransform);
            }
        }

        private void SpawnEnemy(Vector3 position, Quaternion rotation, Transform playerTransform)
        {
            EnemyController enemy = null;

            if (useEnemyPoolIfAvailable && enemyPrefab == null)
            {
                EnemyPool pool = EnemyPool.Instance;
                if (pool != null)
                {
                    enemy = pool.Get(position, rotation);
                }
            }

            if (enemy == null && enemyPrefab != null)
            {
                enemy = Instantiate(enemyPrefab, position, rotation);
            }

            if (enemy == null)
            {
                Debug.LogWarning("LuckyBlock could not spawn an enemy. Assign an enemyPrefab or provide an EnemyPool.", this);
                return;
            }

            LinkSpawnedObject(enemy.gameObject);

            if (enemyConfig != null)
            {
                enemy.Initialize(enemyConfig, playerTransform);
            }
        }

        private void SpawnFriendly(Vector3 position, Quaternion rotation, Transform playerTransform)
        {
            if (friendlyEnemyPrefab == null)
            {
                Debug.LogWarning("LuckyBlock has no friendlyEnemyPrefab assigned.", this);
                return;
            }

            GameObject friendly = Instantiate(friendlyEnemyPrefab, position, rotation);
            if (friendly == null)
            {
                return;
            }

            PowerUps.FriendlyEnemyPickup pickup = friendly.GetComponent<PowerUps.FriendlyEnemyPickup>();
            if (pickup != null)
            {
                pickup.SetTarget(playerTransform);
            }

            LinkSpawnedObject(friendly);
        }

        private void PlayOpenAnimation()
        {
            if (!playOpenAnimation)
            {
                return;
            }

            CleanupTween();
            Vector3 baseScale = transform.localScale;
            openTween = transform.DOPunchScale(openPunchScale, openPunchDuration, openPunchVibrato, openPunchElasticity)
                .SetLink(gameObject)
                .OnKill(() =>
                {
                    if (this != null && transform != null)
                    {
                        transform.localScale = baseScale;
                    }
                });
        }

        private static void LinkSpawnedObject(GameObject spawned)
        {
            if (spawned == null)
            {
                return;
            }

            // Ensure spawned DOTween animations tied to the object are cleaned up with it.
            spawned.transform.DOKill(false);
            Vector3 baseScale = spawned.transform.localScale;
            spawned.transform.localScale = baseScale * 0.01f;
            spawned.transform.DOScale(baseScale, 0.2f)
                .SetEase(Ease.OutBack)
                .SetLink(spawned)
                .OnKill(() =>
                {
                    if (spawned != null)
                    {
                        spawned.transform.localScale = baseScale;
                    }
                });
        }

        private void CleanupTween()
        {
            openTween?.Kill();
            openTween = null;
        }

        private void OnDisable()
        {
            CleanupTween();
        }

        private void OnDestroy()
        {
            CleanupTween();
        }

        private void OnValidate()
        {
            destroyDelay = Mathf.Max(0f, destroyDelay);
            openPunchDuration = Mathf.Max(0.01f, openPunchDuration);
            openPunchVibrato = Mathf.Max(0, openPunchVibrato);
            openPunchElasticity = Mathf.Clamp01(openPunchElasticity);
        }
    }
}

using DG.Tweening;
using UnityEngine;
using EmotionsGame.Audio;
using EmotionsGame.Health;

namespace EmotionsGame.PowerUps
{
    /// <summary>
    /// Friendly pickup that moves like an enemy but grants a one-hit shield on contact.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FriendlyEnemyPickup : MonoBehaviour
    {
        [Header("Shield")]
        [SerializeField, Min(1)] private int shieldCharges = 1;
        [SerializeField] private string playerTag = "Player";

        [Header("Movement")]
        [SerializeField] private bool chasePlayer = true;
        [SerializeField] private string targetTag = "Player";
        [SerializeField, Min(0f)] private float moveSpeed = 3f;
        [SerializeField, Min(0f)] private float turnSpeed = 720f;
        [SerializeField, Min(0f)] private float stopDistance = 0.1f;
        [SerializeField] private bool flattenMovement = true;

        [Header("Lifetime")]
        [SerializeField] private bool destroyOnCollect = true;
        [SerializeField, Min(0f)] private float destroyDelay = 0f;

        private Transform target;
        private bool consumed;
        private Tween moveTween;
        private Tween scaleTween;

        private void Update()
        {
            if (consumed || !chasePlayer)
            {
                return;
            }

            ResolveTarget();
            if (target == null)
            {
                return;
            }

            Vector3 destination = target.position;
            Vector3 delta = destination - transform.position;
            if (flattenMovement)
            {
                delta.y = 0f;
            }

            if (delta.sqrMagnitude <= stopDistance * stopDistance)
            {
                return;
            }

            Vector3 direction = delta.normalized;
            float step = moveSpeed * Time.deltaTime;
            Vector3 nextPosition = transform.position + direction * Mathf.Min(step, delta.magnitude);
            if (flattenMovement)
            {
                nextPosition.y = transform.position.y;
            }

            transform.position = nextPosition;

            if (direction.sqrMagnitude > 0.0001f)
            {
                Quaternion desiredRotation = Quaternion.LookRotation(direction, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, desiredRotation, turnSpeed * Time.deltaTime);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            TryCollect(other != null ? other.gameObject : null);
        }

        private void OnCollisionEnter(Collision collision)
        {
            TryCollect(collision != null ? collision.gameObject : null);
        }

        private void TryCollect(GameObject other)
        {
            if (consumed || other == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(playerTag) && !other.CompareTag(playerTag))
            {
                return;
            }

            PlayerHealth health = other.GetComponentInParent<PlayerHealth>();
            if (health == null)
            {
                return;
            }

            consumed = true;
            health.AddShield(shieldCharges);

            SfxService.Instance?.Play(SfxCue.Pickup);
            CleanupTweens();

            if (destroyOnCollect)
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
                gameObject.SetActive(false);
            }
        }

        private void ResolveTarget()
        {
            if (target != null)
            {
                return;
            }

            if (string.IsNullOrEmpty(targetTag))
            {
                return;
            }

            GameObject found = GameObject.FindGameObjectWithTag(targetTag);
            if (found != null)
            {
                target = found.transform;
            }
        }

        /// <summary>Assigns a runtime target, useful when spawned by systems that already know the player.</summary>
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        /// <summary>Kills any tweens owned by this component.</summary>
        public void CleanupTweens()
        {
            moveTween?.Kill();
            moveTween = null;
            scaleTween?.Kill();
            scaleTween = null;
        }

        private void OnDisable()
        {
            CleanupTweens();
        }

        private void OnDestroy()
        {
            CleanupTweens();
        }

        private void OnValidate()
        {
            shieldCharges = Mathf.Max(1, shieldCharges);
            moveSpeed = Mathf.Max(0f, moveSpeed);
            turnSpeed = Mathf.Max(0f, turnSpeed);
            stopDistance = Mathf.Max(0f, stopDistance);
            destroyDelay = Mathf.Max(0f, destroyDelay);
        }
    }
}

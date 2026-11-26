using UnityEngine;
using EmotionsGame.Health;

namespace EmotionsGame.Obstacles
{
    [DisallowMultipleComponent]
    public sealed class HazardObstacle : MonoBehaviour
    {
        [SerializeField, Min(1)] private int damage = 1;
        [SerializeField] private string targetTag = "Player";
        [SerializeField, Min(0f)] private float knockbackForce;
        [SerializeField] private ForceMode knockbackForceMode = ForceMode.Impulse;

        private void OnTriggerEnter(Collider other)
        {
            if (other != null)
            {
                TryApplyDamage(other.gameObject, other.ClosestPoint(transform.position));
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision == null)
            {
                return;
            }

            ContactPoint contact = collision.contactCount > 0 ? collision.GetContact(0) : default;
            Vector3 hitPoint = collision.contactCount > 0 ? contact.point : collision.transform.position;
            TryApplyDamage(collision.gameObject, hitPoint);
        }

        private void TryApplyDamage(GameObject targetObject, Vector3 hitPoint)
        {
            if (targetObject == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(targetTag) && !targetObject.CompareTag(targetTag))
            {
                return;
            }

            IDamageable damageable = targetObject.GetComponentInParent<IDamageable>();
            if (damageable == null)
            {
                return;
            }

            damageable.TakeDamage(damage);
            ApplyKnockback(targetObject, hitPoint);
        }

        private void ApplyKnockback(GameObject targetObject, Vector3 hitPoint)
        {
            if (knockbackForce <= 0f)
            {
                return;
            }

            Rigidbody targetRigidbody = targetObject.GetComponentInParent<Rigidbody>();
            if (targetRigidbody == null)
            {
                return;
            }

            Vector3 direction = targetRigidbody.worldCenterOfMass - hitPoint;
            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                direction = targetRigidbody.worldCenterOfMass - transform.position;
            }

            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                direction = Vector3.up;
            }

            targetRigidbody.AddForce(direction.normalized * knockbackForce, knockbackForceMode);
        }

        private void OnValidate()
        {
            damage = Mathf.Max(1, damage);
            knockbackForce = Mathf.Max(0f, knockbackForce);
        }
    }
}

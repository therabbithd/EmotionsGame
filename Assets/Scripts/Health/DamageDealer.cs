using UnityEngine;

namespace EmotionsGame.Health
{
    /// <summary>Reusable 3D trigger/collision damage source. Resolves the target by interface.</summary>
    [DisallowMultipleComponent]
    public sealed class DamageDealer : MonoBehaviour
    {
        [SerializeField, Min(1)] private int damage = 1;
        [SerializeField] private string targetTag = "Player";

        private void OnTriggerEnter(Collider other)
        {
            if (!string.IsNullOrEmpty(targetTag) && !other.CompareTag(targetTag))
            {
                return;
            }

            IDamageable target = other.GetComponentInParent<IDamageable>();
            if (target != null)
            {
                target.TakeDamage(damage);
            }
        }

        private void OnValidate()
        {
            damage = Mathf.Max(1, damage);
        }
    }
}

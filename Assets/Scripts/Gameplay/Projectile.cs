using UnityEngine;

namespace EmotionsGame.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class Projectile : MonoBehaviour
    {
        private Vector3 direction;
        private float speed;
        private float lifetime;
        private int damage;
        private bool initialized;

        public void Initialize(Vector3 moveDirection, float moveSpeed, float life, int hitDamage)
        {
            direction = moveDirection.sqrMagnitude > 0.001f ? moveDirection.normalized : Vector3.forward;
            speed = Mathf.Max(1f, moveSpeed);
            lifetime = Mathf.Max(0.1f, life);
            damage = Mathf.Max(1, hitDamage);
            initialized = true;
            Destroy(gameObject, lifetime);
        }

        private void Update()
        {
            if (!initialized)
            {
                return;
            }

            transform.position += direction * (speed * Time.deltaTime);
            transform.Rotate(0f, 540f * Time.deltaTime, 0f);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!initialized || other == null || other.CompareTag("Player"))
            {
                return;
            }

            if (DamageBridge.TryDealDamage(other.gameObject, damage))
            {
                Destroy(gameObject);
            }
        }
    }
}

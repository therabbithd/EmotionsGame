using UnityEngine;
using EmotionsGame.Audio;
using EmotionsGame.Core;

namespace EmotionsGame.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class AmmoController : MonoBehaviour
    {
        [SerializeField, Min(0.05f)] private float fireInterval = 0.35f;
        [SerializeField, Min(1f)] private float projectileSpeed = 16f;
        [SerializeField, Min(0.5f)] private float projectileLifetime = 2.5f;
        [SerializeField, Min(1)] private int damage = 25;
        [SerializeField, Min(0f)] private float spawnDistance = 0.8f;
        [SerializeField, Min(0.05f)] private float projectileScale = 0.28f;

        private float nextFireTime;

        public bool TryFire()
        {
            if (Time.time < nextFireTime || !GameState.ConsumeAmmo())
            {
                return false;
            }

            nextFireTime = Time.time + fireInterval;
            Fire();
            return true;
        }

        private void Fire()
        {
            Vector3 direction = transform.forward;
            Rigidbody body = GetComponent<Rigidbody>();
            if (body != null)
            {
                Vector3 horizontal = new Vector3(body.linearVelocity.x, 0f, body.linearVelocity.z);
                if (horizontal.sqrMagnitude > 0.25f)
                {
                    direction = horizontal.normalized;
                }
            }

            Vector3 spawn = transform.position + direction * spawnDistance + Vector3.up * 0.25f;
            GameObject projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectile.name = "Shot";
            projectile.transform.SetPositionAndRotation(spawn, Quaternion.LookRotation(direction));
            projectile.transform.localScale = Vector3.one * projectileScale;

            SphereCollider collider = projectile.GetComponent<SphereCollider>();
            collider.isTrigger = true;
            Collider[] playerColliders = GetComponentsInChildren<Collider>(true);
            Rigidbody projectileBody = projectile.AddComponent<Rigidbody>();
            projectileBody.isKinematic = true;
            projectileBody.useGravity = false;
            foreach (Collider playerCollider in playerColliders)
            {
                Physics.IgnoreCollision(collider, playerCollider, true);
            }

            Renderer renderer = projectile.GetComponent<Renderer>();
            Material material = Resources.Load<Material>("Materials/Projectile");
            if (material != null)
            {
                renderer.sharedMaterial = material;
            }
            else
            {
                renderer.material.color = new Color(1f, 0.85f, 0.2f);
            }

            projectile.AddComponent<Projectile>().Initialize(direction, projectileSpeed, projectileLifetime, damage);
            SfxService.Instance?.Play(SfxCue.Shoot);
        }
    }
}

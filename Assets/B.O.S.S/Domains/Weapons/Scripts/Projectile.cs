using System;
using UnityEngine;

namespace B.O.S.S.Domains.Weapons.Scripts
{
    public class Projectile : MonoBehaviour
    {
        public event Action OnDestroy;

        [Header("Particles")]
        [SerializeField, Tooltip("Particles spawned when hitting the Boss")]
        private GameObject orangeStarsParticles;
        [SerializeField, Tooltip("Particles spawned when hitting an Enemy")]
        private GameObject pinkStarsParticles;
        [SerializeField, Tooltip("Lifetime of boss-hit particles (seconds)")]
        private float bossParticlesLifetime = 0.8f;
        [SerializeField, Tooltip("Lifetime of enemy-hit particles (seconds)")]
        private float enemyParticlesLifetime = 1f;

        protected Animator Animator;
        private Rigidbody2D _rb;

        private void Awake()
        {
            Animator = GetComponent<Animator>();
            _rb = GetComponent<Rigidbody2D>();
        }

        public void Launch(Vector2 direction)
        {
            if (_rb) _rb.linearVelocity = direction;
        }

        public void Stop()
        {
            if (_rb) _rb.linearVelocity = Vector2.zero;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Boss Bullet")) return;

            if (other.CompareTag("Boss"))
                SpawnParticles(orangeStarsParticles, bossParticlesLifetime);
            else if (other.CompareTag("Enemy"))
                SpawnParticles(pinkStarsParticles, enemyParticlesLifetime);

            HandleHit(other);
        }

        protected virtual void HandleHit(Collider2D other)
        {
            EndShot();
        }

        public void EndShot()
        {
            OnDestroy?.Invoke();
            Destroy(gameObject);
        }

        private void SpawnParticles(GameObject prefab, float lifetime)
        {
            if (!prefab) return;
            var go = Instantiate(prefab, transform.position, Quaternion.identity);
            Destroy(go, lifetime);
        }
    }
}

using System;
using System.Collections;
using UnityEngine;

namespace RiseOfCathulu.Domains.Weapons.Scripts
{
    public class Projectile : MonoBehaviour
    {
        public event Action OnDestroy;

        [Header("Particles")]
        [SerializeField, Tooltip("Particles spawned when hitting the Boss")] private GameObject orangeStarsParticles;
        [SerializeField, Tooltip("Particles spawned when hitting an Enemy")] private GameObject pinkStarsParticles;
        [SerializeField, Tooltip("Lifetime of enemyPlanet-hit particles (sec)")] private float bossParticlesLifetime = 0.8f;
        [SerializeField, Tooltip("Lifetime of enemy-hit particles (sec)")] private float enemyParticlesLifetime = 1f;
        [SerializeField, Tooltip("Seconds before the projectile is destroyed")] private float lifetime = 4f; 
        [SerializeField] private float baseScale = 1.3f;
        
        protected Animator Animator;
        private Rigidbody2D _rb;
        private Coroutine _deathTimer; 

        private void Awake()
        {
            Animator = GetComponent<Animator>();
            _rb = GetComponent<Rigidbody2D>();
        }
        
        public void Initialize(float playerScale)
        {
            transform.localScale = Vector3.one * baseScale * playerScale;
        }

        public void Launch(Vector2 direction)
        {
            if (_rb) _rb.linearVelocity = direction;
            if (_deathTimer != null) StopCoroutine(_deathTimer);
            _deathTimer = StartCoroutine(LifetimeCountdown());
        }
        
        private IEnumerator LifetimeCountdown()
        {
            yield return new WaitForSeconds(lifetime);
            EndShot();
        }

        public void Stop()
        {
            if (_rb) _rb.linearVelocity = Vector2.zero;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Enemy Bullet")) return;
            if (other.CompareTag("Planet Enemy"))
                SpawnParticles(orangeStarsParticles, bossParticlesLifetime);
            else if (other.CompareTag("Enemy"))
                SpawnParticles(pinkStarsParticles, enemyParticlesLifetime);
            HandleHit(other.gameObject);
        }
        
        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.CompareTag("Enemy Bullet")) return;
            if (other.gameObject.CompareTag("Planet Enemy"))
                SpawnParticles(orangeStarsParticles, bossParticlesLifetime);
            else if (other.gameObject.CompareTag("Enemy"))
                SpawnParticles(pinkStarsParticles, enemyParticlesLifetime);
            else if (other.gameObject.CompareTag("Destructable Planet"))
                SpawnParticles(pinkStarsParticles, enemyParticlesLifetime);

            HandleHit(other.gameObject);
        }

        protected virtual void HandleHit(GameObject other)
        {
            EndShot();
        }

        public void EndShot()
        {
            if (_deathTimer != null) 
            {
                StopCoroutine(_deathTimer);
                _deathTimer = null;
            }
            OnDestroy?.Invoke();
            Destroy(gameObject);
        }

        private void SpawnParticles(GameObject prefab, float lifeTime)
        {
            if (!prefab) return;
            var go = Instantiate(prefab, transform.position, Quaternion.identity);
            Destroy(go, lifeTime);
        }
    }
}

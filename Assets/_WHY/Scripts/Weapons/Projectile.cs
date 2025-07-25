using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Weapons
{
    // Base class for all projectiles
    public class Projectile : MonoBehaviour
    {
        public event Action OnDestroy; // Event to notify when the projectile is destroyed
        private Rigidbody2D _rb;
        [SerializeField] private GameObject orangeStarsParticles;
        [SerializeField] private GameObject pinkStarsParticles;
        protected Animator Animator;
        
        private void Awake()
        {
            Animator = GetComponent<Animator>();
            _rb = GetComponent<Rigidbody2D>();
        }

        public void Launch(Vector2 direction)
        {
            _rb.linearVelocity = direction;
        }

        public void Stop()
        {
            _rb.linearVelocity = Vector2.zero;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            GameObject particles;
            if (other.gameObject.CompareTag("Boss"))
            {
                particles = Instantiate(orangeStarsParticles, transform.position, Quaternion.identity);
                Destroy(particles, 0.2f);
            }
            if (other.gameObject.CompareTag("Enemy"))
            {
                particles = Instantiate(pinkStarsParticles, transform.position, Quaternion.identity);
                Destroy(particles, 1f);
            }
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
    }
}
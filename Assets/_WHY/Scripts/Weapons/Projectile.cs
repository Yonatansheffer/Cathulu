using System;
using UnityEngine;

namespace Weapons
{
    // Base class for all projectiles
    public class Projectile : MonoBehaviour
    {
        public event Action OnDestroy; // Event to notify when the projectile is destroyed
        private Rigidbody2D _rb;
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
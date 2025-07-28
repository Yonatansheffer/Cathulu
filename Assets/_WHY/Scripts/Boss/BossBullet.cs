using System;
using UnityEngine;
using Utilities;

namespace _WHY.Scripts.Boss
{
    public class BossBullet : WHYBaseMono, IPoolable
    {
        private static readonly int Hit = Animator.StringToHash("Hit");
        private Rigidbody2D _rb;
        
        private void Start()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        public void Reset()
        {

        }
        
        private Animator _animator;
        
        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Step") || other.gameObject.CompareTag("Background")
                 || other.gameObject.CompareTag("Floor"))
            {
                _rb.linearVelocity = Vector2.zero;
                _animator.SetTrigger(Hit);
            }
            if(other.gameObject.CompareTag("Weapon"))
            {
                ReturnToPool();
            }
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.CompareTag("Step") || other.gameObject.CompareTag("Background")
                                                    || other.gameObject.CompareTag("Floor"))
            {
                _rb.linearVelocity = Vector2.zero;
                _animator.SetTrigger(Hit);
            }
            if (other.gameObject.CompareTag("Weapon"))
            {
                ReturnToPool();
            }
        }
        
        public void ReturnToPool()
        {
            BossBulletPool.Instance.Return(this);
        }
    }
}
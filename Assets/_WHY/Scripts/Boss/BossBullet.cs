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
        
        private Animator animator;
        
        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            _rb.linearVelocity = Vector2.zero;
            animator.SetTrigger(Hit);
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            _rb.linearVelocity = Vector2.zero;
            animator.SetTrigger(Hit);
        }
        
        public void ReturnToPool()
        {
            BossBulletPool.Instance.Return(this);
        }
    }
}
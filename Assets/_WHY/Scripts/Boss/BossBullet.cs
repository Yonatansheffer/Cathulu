using System;
using UnityEngine;
using Utilities;

namespace _WHY.Scripts.Boss
{
    public class BossBullet : WHYBaseMono, IPoolable
    {
        private static readonly int Hit = Animator.StringToHash("Hit");

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
            animator.SetTrigger(Hit);
        }

        public void ReturnToPool()
        {
            BossBulletPool.Instance.Return(this);
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            animator.SetTrigger(Hit);

        }
    }
}
using System;
using UnityEngine;
using Utilities;

namespace _WHY.Scripts.Boss
{
    public class BossBullet : WHYBaseMono, IPoolable
    {
        public void Reset()
        {

        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            BossBulletPool.Instance.Return(this);
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            BossBulletPool.Instance.Return(this);
        }
    }
}
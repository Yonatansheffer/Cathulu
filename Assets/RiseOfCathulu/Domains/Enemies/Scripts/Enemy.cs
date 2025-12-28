using RiseOfCathulu.Domains.Utilities.GameHandlers.Scripts;
using UnityEngine;

namespace RiseOfCathulu.Domains.Enemies.Scripts
{
    public abstract class Enemy : BossBaseMono, IPoolable 
    {
        public virtual void Reset()
        {
        }

        protected virtual void Update()
        {
            Move();
        }
        
        protected abstract void Move();

        public virtual void ToTarget(Vector2 targetPosition)
        {
        }
    }
}
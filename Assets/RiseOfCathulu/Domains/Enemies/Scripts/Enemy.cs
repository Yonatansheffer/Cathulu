using RiseOfCathulu.Domains.Utilities.GameHandlers.Scripts;
using UnityEngine;

namespace RiseOfCathulu.Domains.Enemies.Scripts
{
    public abstract class Enemy : BossBaseMono, IPoolable 
    {
        [HideInInspector] public int sizeLevel;
        
        public virtual void Reset()
        {
            sizeLevel = 0;
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
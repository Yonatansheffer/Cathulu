using _WHY.Domains.Utilities.GameHandlers.Scripts;
using UnityEngine;
using Utilities;

namespace _WHY.Domains.Enemies.Scripts
{
    public abstract class Enemy : WHYBaseMono, IPoolable 
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
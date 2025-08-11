using UnityEngine;
using Utilities; 

namespace _WHY.Scripts.Enemies
{
    public abstract class Enemy : WHYBaseMono, IPoolable 
    {
        public virtual void Reset()
        {
        }

        protected void Update()
        {
            Move();
        }
        
        protected abstract void Move();

        public virtual void ToTarget(Vector2 targetPosition)
        {
        }
    }
}
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using Utilities; // Assuming this contains MonoPool or similar

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


    }
}


/*[SerializeField] protected float moveSpeed = 3f;
protected Vector2 moveDirection;
protected bool isActive;
protected SpriteRenderer spriteRenderer;

protected virtual void Awake()
{
    Initialize(Vector2.zero, Vector2.zero);
    spriteRenderer = GetComponent<SpriteRenderer>();
}

protected virtual void Update()
{
    if (!isActive) return;
    Move();
}

protected abstract void Move();

public virtual void Initialize(Vector2 spawnPosition, Vector2 direction)
{
    transform.position = spawnPosition;
    moveDirection = direction.normalized;
    isActive = true;
    gameObject.SetActive(true);
}

public virtual void Reset()
{
    isActive = false;
}



protected virtual void Die()
{
    isActive = false;
    EnemyPool.Instance.Return(this);
}*/
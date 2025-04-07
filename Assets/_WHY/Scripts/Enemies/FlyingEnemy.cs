using System;
using UnityEngine;

namespace _WHY.Scripts.Enemies
{
    public class FlyingEnemy : Enemy
    {
         private Transform _playerTransform;   // Keep slightly above ground
        [SerializeField] private float moveSpeed = 1f;
        
        private void Awake()
        {
            _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        }
        
        protected override void Move()
        {
            transform.position = Vector3.MoveTowards(transform.position, _playerTransform.position, moveSpeed * Time.deltaTime);
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Weapon"))
            {
                FlyingEnemyPool.Instance.Return(gameObject.GetComponent<FlyingEnemy>());
            }
        }
        
    }
}
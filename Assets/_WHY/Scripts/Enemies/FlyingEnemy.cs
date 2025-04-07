using System;
using UnityEngine;

namespace _WHY.Scripts.Enemies
{
    public class FlyingEnemy : Enemy
    {
        private static readonly int MovingRight = Animator.StringToHash("MovingRight");
        private Transform _playerTransform;
        [SerializeField] private float moveSpeed = 1f;
        private Animator _animator;
        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
            _animator = GetComponent<Animator>();
            _spriteRenderer = GetComponent<SpriteRenderer>(); // Optional: for flipping sprite
        }

        protected override void Move()
        {
            Vector3 direction = _playerTransform.position - transform.position;
            _animator.SetBool(MovingRight, direction.x > 0f);
            _spriteRenderer.flipX = direction.x < 0f;
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
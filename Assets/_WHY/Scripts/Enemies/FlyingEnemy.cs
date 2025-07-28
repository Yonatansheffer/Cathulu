using System;
using GameHandlers;
using Sound;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _WHY.Scripts.Enemies
{
    public class FlyingEnemy : Enemy
    {
        private static readonly int MovingRight = Animator.StringToHash("MovingRight");
        private Transform _playerTransform;
        [SerializeField] private float moveSpeed = 1f;
        private Animator _animator;
        private SpriteRenderer _spriteRenderer;
        private bool _isRight;

        private void Awake()
        {
            _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
            _animator = GetComponent<Animator>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _isRight = Random.value > 0.5f;
        }

        protected override void Move()
        {
            Vector3 direction = (_playerTransform.position - transform.position).normalized;
            _animator.SetBool(MovingRight, direction.x > 0f);
            _spriteRenderer.flipX = direction.x < 0f;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 2f, LayerMask.GetMask("Ground"));
            if (hit.collider == null)
            {
                transform.position += direction * moveSpeed * Time.deltaTime;
            }
            else
            {
                direction.x += _isRight ?  5f: -5f;
                transform.position += direction.normalized * moveSpeed * Time.deltaTime;
            }
        }


        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Weapon"))
            {
                SoundManager.Instance.PlaySound("Explosion", transform);
                GameEvents.EnemyDestroyed?.Invoke(transform.position);
                ReturnToPool();
            }
        }
        
        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                ReturnToPool();
            }
        }

        private void ReturnToPool()
        {
            FlyingEnemyPool.Instance.Return(gameObject.GetComponent<FlyingEnemy>());
        }


    }
}
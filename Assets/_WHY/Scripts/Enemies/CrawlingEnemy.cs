using System.Collections;
using UnityEngine;

namespace _WHY.Scripts.Enemies
{
    public class CrawlingEnemy : Enemy
    {
        private static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");
        [SerializeField] private float moveSpeed = 2f;
        private Vector2 _moveDirection = Vector2.right;
        private Rigidbody2D _rb;
        private bool _isMoving;
        private Animator _animator;
        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();
            _spriteRenderer = GetComponent<SpriteRenderer>(); 
        }
        
        private new void Update()
        {
            UpdateAnimation();
            if (!_isMoving) return;
            Move();
        }
        
        private void UpdateAnimation()
        {
            _animator.SetFloat(MoveSpeed, _rb.linearVelocity.magnitude);
            _spriteRenderer.flipX = _moveDirection.x < 0f;
        }

        public override void Reset()
        {
            _isMoving = false;
        } 

        private void OnEnable()
        {
            _moveDirection = Random.value < 0.5f ? Vector2.left : Vector2.right;
        }

        protected override void Move()
        {
            _rb.linearVelocity = _moveDirection * moveSpeed;
            _spriteRenderer.flipX = _moveDirection.x > 0f;

        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Weapon"))
            {
                ReturnToPool();
            }
            if (other.CompareTag("Boundary"))
            {
                FlipDirection();
            }
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                ReturnToPool();
            }
            if (other.gameObject.CompareTag("Step")  ||other.gameObject.CompareTag("Floor"))
            {
                _isMoving = true;
            }
        }
        
        private void ReturnToPool()
        {
            CrawlingEnemyPool.Instance.Return(gameObject.GetComponent<CrawlingEnemy>());
        }

        private void FlipDirection()
        {
            _moveDirection *= -1;
        }
    }
}
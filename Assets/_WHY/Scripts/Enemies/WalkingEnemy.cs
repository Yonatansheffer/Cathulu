using System;
using GameHandlers;
using Sound;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _WHY.Scripts.Enemies
{
    public class WalkingEnemy : Enemy
    {
        private static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");
        [SerializeField] private float moveSpeed = 2f;
        private Vector2 _moveDirection = Vector2.right;
        private Rigidbody2D _rb;
        private CapsuleCollider2D _collider;
        private bool _isMoving;
        private Animator _animator;
        private SpriteRenderer _spriteRenderer;
        private Vector3 _targetPosition;
        private bool _movingToTarget;
        [SerializeField] private float fadeInDuration = 1f;
        [SerializeField] private int pointsForKill = 70;
        private float _fadeTimer;
        private bool _isFrozen = false;
        
        private void Awake()
        {
            _isMoving = false;
            _movingToTarget = false;
            _rb = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();
            _spriteRenderer = GetComponent<SpriteRenderer>(); 
            _collider = GetComponent<CapsuleCollider2D>();
        }
        
        private void OnEnable()
        {
            GameEvents.FreezeLevel += OnFreeze;
            GameEvents.UnFreezeLevel += OnUnFreeze;
        }
        
        private void OnDisable()
        {
            GameEvents.FreezeLevel -= OnFreeze;
            GameEvents.UnFreezeLevel -= OnUnFreeze;
        }
        
        private void OnFreeze()
        {
            _isFrozen = true;
            _rb.linearVelocity = Vector2.zero;
            _animator.speed = 0f; 

        }

        private void OnUnFreeze()
        {
            _isFrozen = false;
            _animator.speed = 1f;
        }

        
        public override void ToTarget(Vector2 targetPosition)
        {
            _targetPosition = targetPosition;
            _movingToTarget = true;
            _spriteRenderer.color = new Color(1f, 1f, 1f, 0.3f); // semi-transparent
            _fadeTimer = 0f;
        }

        private new void Update()
        {
            if (_isFrozen) return;
            UpdateAnimation();
            if (_movingToTarget)
            {
                _fadeTimer += Time.deltaTime;
                float alpha = Mathf.Clamp01(_fadeTimer / fadeInDuration);
                _spriteRenderer.color = new Color(1f, 1f, 1f, Mathf.Lerp(0.3f, 1f, alpha));
                MoveToTarget();
            }
            else if (_isMoving)
            {
                Move();
            }
        }

        
        private void MoveToTarget()
        {
            if (_isFrozen) return;
            var currentPosition = transform.position;
            var direction = (_targetPosition - currentPosition).normalized;
            _rb.linearVelocity = direction * moveSpeed;
            _spriteRenderer.flipX = direction.x > 0f;
        }
        
        private void UpdateAnimation()
        {
            _animator.SetFloat(MoveSpeed, _rb.linearVelocity.magnitude);
            _spriteRenderer.flipX = _moveDirection.x < 0f;
        }

        public override void Reset()
        {
            _isMoving = false;
            _rb.constraints &= ~RigidbodyConstraints2D.FreezePositionY;
            _movingToTarget = false;
        } 

        private void Start()
        {
            _moveDirection = Random.value < 0.5f ? Vector2.left : Vector2.right;
        }

        protected override void Move()
        {
            if (_isFrozen) return;

            _rb.linearVelocity = _moveDirection * moveSpeed;
            _spriteRenderer.flipX = _moveDirection.x > 0f;
        }


        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Weapon"))
            {
                ReturnToPool();
                SoundManager.Instance.PlaySound("Explosion", transform);
                GameEvents.AddPoints?.Invoke(pointsForKill);
            }
            if (other.CompareTag("Boundary"))
            {
                FlipDirection();
            }       
            if (other.CompareTag("Step Center") && _movingToTarget)
            {
                _movingToTarget = false;
                _collider.isTrigger = false;
            }
            if (other.gameObject.CompareTag("Player"))
            {
                ReturnToPool();
            }
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.CompareTag("Step") || other.gameObject.CompareTag("Floor"))
            {
                _rb.constraints |= RigidbodyConstraints2D.FreezePositionY;
                _isMoving = true;
                _collider.isTrigger = true;
            }
        }

        private void ReturnToPool()
        {
            WalkingEnemyPool.Instance.Return(gameObject.GetComponent<WalkingEnemy>());
        }

        private void FlipDirection()
        {
            _moveDirection *= -1;
        }
    }
}
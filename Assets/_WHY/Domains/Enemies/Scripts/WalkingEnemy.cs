using _WHY.Domains.Utilities.GameHandlers.Scripts;
using _WHY.Scripts.Enemies;
using Sound;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _WHY.Domains.Enemies.Scripts
{
    public class WalkingEnemy : Enemy
    {
        private static readonly int Walk = Animator.StringToHash("Walk");
        [SerializeField, Tooltip("Speed of enemy movement")] private float moveSpeed;
        [SerializeField, Tooltip("Duration of fade-in effect when moving to target")] private float fadeInDuration;
        [SerializeField, Tooltip("Points awarded for destroying this enemy")] private int pointsForKill;
        private Vector2 _moveDirection = Vector2.right;
        private Rigidbody2D _rb;
        private CapsuleCollider2D _collider;
        private bool _isMoving;
        private Animator _animator;
        private SpriteRenderer _spriteRenderer;
        private Vector3 _targetPosition;
        private bool _movingToTarget;
        private float _fadeTimer;
        private bool _isFrozen;
        private RigidbodyConstraints2D _originalConstraints;
        
        private void Start()
        {
            _moveDirection = Random.value < 0.5f ? Vector2.left : Vector2.right;
        }
        
        private void Awake()
        {
            _isMoving = false;
            _movingToTarget = true;
            _rb = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();
            _spriteRenderer = GetComponent<SpriteRenderer>(); 
            _collider = GetComponent<CapsuleCollider2D>();
        }
        
        private void OnEnable()
        {
            GameEvents.FreezeLevel += () => SetFreezeState(true);
            GameEvents.UnFreezeLevel += () => SetFreezeState(false);
        }

        private void OnDisable()
        {
            GameEvents.FreezeLevel -= () => SetFreezeState(true);
            GameEvents.UnFreezeLevel -= () => SetFreezeState(false);
        }

        private void SetFreezeState(bool freeze)
        {
            _isFrozen = freeze;
            if (_animator != null) _animator.speed = freeze ? 0f : 1f;
            if (_rb != null) _rb.constraints = freeze ? RigidbodyConstraints2D.FreezeAll : _originalConstraints;
        }
        
        public override void ToTarget(Vector2 targetPosition)
        {
            _targetPosition = targetPosition;
            if (_spriteRenderer != null) _spriteRenderer.color = new Color(1f, 1f, 1f, 0.3f);
            _fadeTimer = 0f;
        }

        private new void Update()
        {
            if (_isFrozen) return;
            if (_spriteRenderer != null) _spriteRenderer.flipX = _moveDirection.x > 0f;
            if (_movingToTarget)
            {
                _fadeTimer += Time.deltaTime;
                var alpha = Mathf.Clamp01(_fadeTimer / fadeInDuration);
                if (_spriteRenderer != null) _spriteRenderer.color = new Color(1f, 1f, 1f, Mathf.Lerp(0.3f, 1f, alpha));
                MoveToTarget();
            }
            else if (_isMoving)
            {
                Move();
            }
        }
        
        private void MoveToTarget()
        {
            if (_isFrozen || _rb == null) return;
            var currentPosition = transform.position;
            var direction = (_targetPosition - currentPosition).normalized;
            _rb.linearVelocity = direction * moveSpeed;
            if (_spriteRenderer != null) _spriteRenderer.flipX = direction.x > 0f;
        }

        public override void Reset()
        {
            if (_rb != null)
            {
                _rb.linearVelocity = Vector2.zero;
                _rb.angularVelocity = 0f;
                _rb.constraints = RigidbodyConstraints2D.None;
            }
            if (_animator != null)
            {
                _animator.speed = 1f;
                _animator.ResetTrigger(Walk);
            }
            if (_collider != null) _collider.isTrigger = true;
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = new Color(1f, 1f, 1f, 0.3f);
                _spriteRenderer.flipX = Random.value < 0.5f;
            }
            _isFrozen = false;
            _isMoving = false;
            _movingToTarget = true;
            _fadeTimer = 0f;
            _moveDirection = Random.value < 0.5f ? Vector2.left : Vector2.right;
        }
        
        protected override void Move()
        {
            if (_isFrozen || _rb == null) return;
            _rb.linearVelocity = _moveDirection * moveSpeed;
        }


        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Weapon"))
            {
                SoundManager.Instance.PlaySound("Explosion", transform);
                GameEvents.AddPoints?.Invoke(pointsForKill);
                ReturnToPool();
            }
            else if (other.CompareTag("Boundary")) _moveDirection *= -1;
            else if (other.CompareTag("Step Center") && _movingToTarget)
            {
                if (_rb != null) _rb.constraints |= RigidbodyConstraints2D.FreezePositionX;
                _movingToTarget = false;
                if (_collider != null) _collider.isTrigger = false;
            }
            else if (other.CompareTag("Player")) ReturnToPool();
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (!other.gameObject.CompareTag("Step") && !other.gameObject.CompareTag("Floor")) return;
            if (_rb != null)
            {
                _rb.constraints |= RigidbodyConstraints2D.FreezePositionY;
                _rb.constraints &= ~RigidbodyConstraints2D.FreezePositionX;
            }
            _isMoving = true;
            if (_collider != null) _collider.isTrigger = true;
            if (_animator != null) _animator.SetTrigger(Walk);
        }

        private void ReturnToPool()
        {
            GameEvents.FreezeLevel -= () => SetFreezeState(true);
            GameEvents.UnFreezeLevel -= () => SetFreezeState(false);
            WalkingEnemyPool.Instance.Return(this);
        }
    }
}
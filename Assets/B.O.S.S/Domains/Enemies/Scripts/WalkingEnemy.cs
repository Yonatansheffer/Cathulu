using _WHY.Domains.Utilities.GameHandlers.Scripts;
using _WHY.Domains.Utilities.Sound.Scripts;
using _WHY.Scripts.Enemies;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _WHY.Domains.Enemies.Scripts
{
    public class WalkingEnemy : Enemy
    {
        private static readonly int Walk = Animator.StringToHash("Walk");

        [Header("Settings")]
        [SerializeField, Tooltip("Speed of enemy movement")] private float moveSpeed = 2f;
        [SerializeField, Tooltip("Duration of fade-in effect when moving to target")] private float fadeInDuration = 1f;
        [SerializeField, Tooltip("Points awarded for destroying this enemy")] private int pointsForKill = 1;

        private Rigidbody2D _rb;
        private CapsuleCollider2D _collider;
        private Animator _animator;
        private SpriteRenderer _spriteRenderer;

        private Vector2 _moveDirection = Vector2.right;
        private Vector3 _targetPosition;
        private float _fadeTimer;

        private bool _movingToTarget;
        private bool _fallingToStep;
        private bool _isMoving;
        private bool _isFrozen;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _collider = GetComponent<CapsuleCollider2D>();

            _movingToTarget = true;
            _isMoving = false;
            _fallingToStep = false;

            if (_spriteRenderer) _spriteRenderer.color = new Color(1f, 1f, 1f, 0.3f);
        }

        private void Start()
        {
            _moveDirection = Random.value < 0.5f ? Vector2.left : Vector2.right;
            UpdateConstraints();
        }

        private void OnEnable()
        {
            GameEvents.FreezeLevel += OnFreezeLevel;
            GameEvents.UnFreezeLevel += OnUnfreezeLevel;
        }

        private void OnDisable()
        {
            GameEvents.FreezeLevel -= OnFreezeLevel;
            GameEvents.UnFreezeLevel -= OnUnfreezeLevel;
        }

        private void OnFreezeLevel() => SetFreezeState(true);
        private void OnUnfreezeLevel() => SetFreezeState(false);

        private new void Update()
        {
            if (_isFrozen) return;

            if (_spriteRenderer && !_movingToTarget)
                _spriteRenderer.flipX = _moveDirection.x > 0f;

            if (_movingToTarget)
            {
                _fadeTimer += Time.deltaTime;
                var alpha = Mathf.Clamp01(fadeInDuration > 0f ? _fadeTimer / fadeInDuration : 1f);
                if (_spriteRenderer)
                    _spriteRenderer.color = new Color(1f, 1f, 1f, Mathf.Lerp(0.3f, 1f, alpha));
                MoveToTarget();
            }
            else if (_isMoving)
            {
                Move();
            }
        }

        public override void ToTarget(Vector2 targetPosition)
        {
            _targetPosition = targetPosition;
            _movingToTarget = true;
            _fallingToStep = false;
            _isMoving = false;

            if (_spriteRenderer)
                _spriteRenderer.color = new Color(1f, 1f, 1f, 0.3f);

            _fadeTimer = 0f;

            if (_rb)
                _rb.linearVelocity = Vector2.zero;

            UpdateConstraints();
        }

        public override void Reset()
        {
            if (_rb)
            {
                _rb.linearVelocity = Vector2.zero;
                _rb.angularVelocity = 0f;
            }

            if (_animator)
            {
                _animator.speed = 1f;
                _animator.ResetTrigger(Walk);
            }

            if (_collider) _collider.isTrigger = true;

            if (_spriteRenderer)
            {
                _spriteRenderer.color = new Color(1f, 1f, 1f, 0.3f);
                _spriteRenderer.flipX = Random.value < 0.5f;
            }

            _isFrozen = false;
            _isMoving = false;
            _fallingToStep = false;
            _movingToTarget = true;

            _fadeTimer = 0f;
            _moveDirection = Random.value < 0.5f ? Vector2.left : Vector2.right;

            UpdateConstraints();
        }

        protected override void Move()
        {
            if (_isFrozen || !_rb) return;
            _rb.linearVelocity = _moveDirection * moveSpeed;
        }

        private void MoveToTarget()
        {
            if (_isFrozen || !_rb) return;

            var current = transform.position;
            var dir = (_targetPosition - current).normalized;

            _rb.linearVelocity = dir * moveSpeed;

            if (_spriteRenderer) _spriteRenderer.flipX = dir.x > 0f;

            const float reachEps = 0.05f;
            if (Vector2.Distance(current, _targetPosition) <= reachEps)
            {
                _movingToTarget = false;
                _fallingToStep = true;
                _isMoving = false;

                _rb.linearVelocity = Vector2.zero;
                UpdateConstraints();
            }
        }

        private void UpdateConstraints()
        {
            if (!_rb) return;

            if (_isFrozen)
            {
                _rb.constraints = RigidbodyConstraints2D.FreezeAll;
                if (_collider) _collider.isTrigger = true;
                return;
            }

            var constraints = RigidbodyConstraints2D.FreezeRotation;

            if (_movingToTarget)
            {
                if (_collider) _collider.isTrigger = true;
            }
            else if (_fallingToStep)
            {
                constraints |= RigidbodyConstraints2D.FreezePositionX;
                if (_collider) _collider.isTrigger = false;
            }
            else if (_isMoving)
            {
                constraints |= RigidbodyConstraints2D.FreezePositionY;
                if (_collider) _collider.isTrigger = true;
            }
            else
            {
                if (_collider) _collider.isTrigger = false;
            }

            _rb.constraints = constraints;
        }

        private void SetFreezeState(bool freeze)
        {
            _isFrozen = freeze;

            if (_animator)
                _animator.speed = freeze ? 0f : 1f;

            if (_rb && freeze)
                _rb.linearVelocity = Vector2.zero;

            UpdateConstraints();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Weapon"))
            {
                SoundManager.Instance.PlaySound("Explosion", transform);
                GameEvents.AddPoints?.Invoke(pointsForKill);
                ReturnToPool();
            }
            else if (other.CompareTag("Boundary"))
            {
                _moveDirection *= -1;
            }
            else if (other.CompareTag("Step Center") && _movingToTarget)
            {
                _movingToTarget = false;
                _fallingToStep = true;
                _isMoving = false;

                if (_rb) _rb.linearVelocity = Vector2.zero;
                UpdateConstraints();
            }
            else if (other.CompareTag("Player"))
            {
                ReturnToPool();
            }
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (!other.gameObject.CompareTag("Step") && !other.gameObject.CompareTag("Floor")) return;

            _isMoving = true;
            _fallingToStep = false;
            _movingToTarget = false;

            if (_rb) _rb.linearVelocity = Vector2.zero;
            UpdateConstraints();

            if (_animator) _animator.SetTrigger(Walk);
        }

        private void ReturnToPool()
        {
            GameEvents.FreezeLevel -= OnFreezeLevel;
            GameEvents.UnFreezeLevel -= OnUnfreezeLevel;
            WalkingEnemyPool.Instance.Return(this);
        }
    }
}

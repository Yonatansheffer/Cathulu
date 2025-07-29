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
        private Vector3 _lastYCheckPosition;
        private float _stuckTime;
        [SerializeField] private float stuckCheckInterval = 0.5f;
        [SerializeField] private float stuckThreshold = 1f;
        [SerializeField] private float stuckMoveBoost = 10f;
        private bool _isFrozen = false;
        private Rigidbody2D _rb;
        [SerializeField] private float bigEnemySize = 9f;
        [SerializeField] private float bigEnemySpeed = 5f;
        
        private void Awake()
        {
            _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
            _animator = GetComponent<Animator>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _isRight = Random.value > 0.5f;
            stuckMoveBoost = _isRight? 10f : -10f;
            _rb = GetComponent<Rigidbody2D>();
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
        
        public void SetBigEnemy()
        {
            moveSpeed = bigEnemySpeed;
            transform.localScale = new Vector3(bigEnemySize, bigEnemySize, 1f);
        }
        
        private void OnFreeze()
        {
            _rb.linearVelocity = Vector2.zero;
            _isFrozen = true;
            _animator.speed = 0f; 

        }

        private void OnUnFreeze()
        {
            _isFrozen = false;
            _animator.speed = 1f;
        }

        protected override void Move()
        {
            if (_isFrozen) return;
            Vector3 direction = (_playerTransform.position - transform.position).normalized;
            _animator.SetBool(MovingRight, direction.x > 0f);
            _spriteRenderer.flipX = direction.x < 0f;
            /*if (Mathf.Abs(transform.position.y - _lastYCheckPosition.y) < stuckThreshold)
            {
                _stuckTime += Time.deltaTime;
                if (_stuckTime > stuckCheckInterval)
                {
                    direction.x += stuckMoveBoost * Mathf.Sign(direction.x);
                    _stuckTime = 0f; 
                }
            }
            else
            {
                _stuckTime = 0f;
            }*/
            _lastYCheckPosition = transform.position;
            transform.position += direction * moveSpeed * Time.deltaTime;           
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
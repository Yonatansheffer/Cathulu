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
        private bool _isFrozen = false;
        private Rigidbody2D _rb;
        [SerializeField] private float bigEnemySize = 9f;
        [SerializeField] private float bigEnemySpeed = 5f;
        [SerializeField] private int pointsForKill = 70;

        
        private void Awake()
        {
            _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
            _animator = GetComponent<Animator>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
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
            if (_isFrozen || _playerTransform == null) return;

            // === WEAK PLAYER ATTRACTION (WEIGHTED) ===
            Vector3 toPlayer = _playerTransform.position - transform.position;
            Vector3 playerDir = toPlayer.normalized;
            float playerAttractionWeight = 0.5f;

            // === SMOOTH RANDOM MOVEMENT VIA PERLIN NOISE ===
            float timeOffset = Time.time * 0.3f + GetInstanceID(); // Unique per enemy
            float noiseX = Mathf.PerlinNoise(timeOffset, 0f) - 0.5f;
            float noiseY = Mathf.PerlinNoise(0f, timeOffset) - 0.5f;
            Vector3 randomDir = new Vector3(noiseX, noiseY, 0f).normalized;

            // === COMBINE RANDOMNESS AND PLAYER ATTRACTION ===
            Vector3 finalDir = (randomDir * (1f - playerAttractionWeight) + playerDir * playerAttractionWeight).normalized;

            float detectionDistance = 4f;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, finalDir, detectionDistance, LayerMask.GetMask("Ground"));

            if (hit.collider != null)
            {
                // Calculate avoidance direction (perpendicular to the surface)
                Vector2 obstacleNormal = hit.normal;
                finalDir = (finalDir + (Vector3)obstacleNormal * 4f).normalized;
            }

            // === MOVE ENEMY ===
            transform.position += finalDir * moveSpeed * Time.deltaTime;

            // === FLIP SPRITE BASED ON MOVEMENT ===
            _animator.SetBool(MovingRight, finalDir.x > 0f);
            _spriteRenderer.flipX = finalDir.x < 0f;
        }

        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Weapon"))
            {
                SoundManager.Instance.PlaySound("Explosion", transform);
                GameEvents.AddPoints?.Invoke(pointsForKill);
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
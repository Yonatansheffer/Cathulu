using GameHandlers;
using Sound;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Random = UnityEngine.Random;

namespace _WHY.Scripts.Enemies
{
    public class FlyingEnemy : Enemy
    {
        private static readonly int MovingRight = Animator.StringToHash("MovingRight");
        private Transform _playerTransform;
        [SerializeField] private float moveSpeed;
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

        public override void Reset()
        {
            /*// Restore size & speed to defaults
            transform.localScale = Vector3.one;
            moveSpeed = 1f; // or whatever your original normal speed is*/

            // Reset physics
            if (_rb != null)
            {
                _rb.linearVelocity = Vector2.zero;
                _rb.angularVelocity = 0f;
            }

            /*// Reset animation
            if (_animator != null)
            {
                _animator.speed = 1f;
                _animator.SetBool(MovingRight, false);
            }*/
            _isFrozen = false;
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
            float playerAttractionWeight = 0.55f;

            // === SMOOTH RANDOM MOVEMENT VIA PERLIN NOISE ===
            float timeOffset = Time.time * 0.3f + GetInstanceID(); // Unique per enemy
            float noiseX = Mathf.PerlinNoise(timeOffset, 0f) - 0.5f;
            float noiseY = Mathf.PerlinNoise(0f, timeOffset) - 0.5f;
            Vector3 randomDir = new Vector3(noiseX, noiseY, 0f).normalized;

            // === COMBINE RANDOMNESS AND PLAYER ATTRACTION ===
            Vector3 finalDir = (randomDir * (1f - playerAttractionWeight) + playerDir * playerAttractionWeight).normalized;

            float detectionDistance = 4f;
            LayerMask groundMask = LayerMask.GetMask("Ground");

            RaycastHit2D hit = Physics2D.Raycast(transform.position, finalDir, detectionDistance, groundMask);

            if (hit.collider != null)
            {
                // Try to find a clear side
                Vector2 obstacleNormal = hit.normal;
    
                // Perpendicular directions to the normal
                Vector2 perp1 = new Vector2(-obstacleNormal.y, obstacleNormal.x); // left
                Vector2 perp2 = new Vector2(obstacleNormal.y, -obstacleNormal.x); // right

                bool leftClear = !Physics2D.Raycast(transform.position, perp1, 1f, groundMask);
                bool rightClear = !Physics2D.Raycast(transform.position, perp2, 1f, groundMask);

                Vector2 avoidDir;
                if (leftClear && !rightClear)
                    avoidDir = perp1;
                else if (rightClear && !leftClear)
                    avoidDir = perp2;
                else if (leftClear && rightClear)
                    // Pick the side with more clearance
                    avoidDir = Physics2D.Raycast(transform.position + (Vector3)perp1 * 1f, perp1, detectionDistance, groundMask)
                        ? perp2 : perp1;
                else
                    avoidDir = obstacleNormal; // No side clear, just back off

                // Blend avoidance direction into current direction for smoothness
                finalDir = Vector2.Lerp(finalDir, avoidDir, 2f).normalized;
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
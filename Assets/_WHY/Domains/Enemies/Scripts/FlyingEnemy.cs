using _WHY.Domains.Utilities.GameHandlers.Scripts;
using Sound;
using UnityEngine;

namespace _WHY.Domains.Enemies.Scripts 
{
    public class FlyingEnemy : Enemy
    {
        private static readonly int MovingRight = Animator.StringToHash("MovingRight");
        private Transform _playerTransform;
        [SerializeField, Tooltip("Speed of enemy movement")] private float moveSpeed;
        [SerializeField, Tooltip("Scale for big enemy variant")] private float bigEnemySize;
        [SerializeField, Tooltip("Speed for big enemy variant")] private float bigEnemySpeed;
        [SerializeField, Tooltip("Points awarded for destroying this enemy")] private int pointsForKill;
        [SerializeField, Tooltip("Weight of player attraction (0-1)")] private float playerAttractionWeight = 0.55f;
        [SerializeField, Tooltip("Time scale for Perlin noise randomness")] private float noiseTimeScale = 0.3f;
        [SerializeField, Tooltip("Distance for obstacle detection raycast")] private float detectionDistance = 4f;
        [SerializeField, Tooltip("Distance for side clearance raycast")] private float sideClearanceDistance = 1f;
        [SerializeField, Tooltip("Weight for blending avoidance direction")] private float avoidanceLerpWeight = 2f;

        private Animator _animator;
        private SpriteRenderer _spriteRenderer;
        private bool _isFrozen;
        private Rigidbody2D _rb;

        private void Awake()
        {
            _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
            _animator = GetComponent<Animator>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _rb = GetComponent<Rigidbody2D>();
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
        
        public void SetBigEnemy()
        {
            moveSpeed = bigEnemySpeed;
            transform.localScale = new Vector3(bigEnemySize, bigEnemySize, 1f);
        }       
        
        private void SetFreezeState(bool freeze)
        {
            _isFrozen = freeze;
            if(_animator) _animator.speed = freeze ? 0f : 1f;
            if (freeze && _rb != null)
            {
                _rb.linearVelocity = Vector2.zero;
            }
        }

        public override void Reset()
        {
            _animator.speed = 1f;
            if (_rb != null)
            {
                _rb.linearVelocity = Vector2.zero;
                _rb.angularVelocity = 0f;
            }
            _isFrozen = false;
        }
        
        
        protected override void Move()
        {
            if (_isFrozen || _playerTransform == null) return;
            var finalDir = CalculateDirection();
            finalDir = HandleObstacleAvoidance(finalDir);
            transform.position += finalDir * moveSpeed * Time.deltaTime;
            _animator.SetBool(MovingRight, finalDir.x > 0f);
            _spriteRenderer.flipX = finalDir.x < 0f;
        }

        private Vector3 CalculateDirection()
        {
            var toPlayer = _playerTransform.position - transform.position;
            var playerDir = toPlayer.normalized;
            var timeOffset = Time.time * noiseTimeScale + GetInstanceID();
            var noiseX = Mathf.PerlinNoise(timeOffset, 0f) - 0.5f;
            var noiseY = Mathf.PerlinNoise(0f, timeOffset) - 0.5f;
            var randomDir = new Vector3(noiseX, noiseY, 0f).normalized;
            return (randomDir * (1f - playerAttractionWeight) + playerDir * playerAttractionWeight).normalized;
        }

        private Vector3 HandleObstacleAvoidance(Vector3 moveDir)
        {
            LayerMask groundMask = LayerMask.GetMask("Ground");
            var hit = Physics2D.Raycast(
                transform.position, moveDir, detectionDistance, groundMask);
            if (hit.collider == null) return moveDir;
            var obstacleNormal = hit.normal;
            var perp1 = new Vector2(-obstacleNormal.y, obstacleNormal.x);
            var perp2 = new Vector2(obstacleNormal.y, -obstacleNormal.x); 
            var leftClear = !Physics2D.Raycast(
                transform.position, perp1, sideClearanceDistance, groundMask);
            var rightClear = !Physics2D.Raycast(
                transform.position, perp2, sideClearanceDistance, groundMask);
            Vector2 avoidDir;
            if (leftClear && !rightClear)
                avoidDir = perp1;
            else if (rightClear && !leftClear)
                avoidDir = perp2;
            else if (leftClear)
                avoidDir = Physics2D.Raycast(
                    transform.position + (Vector3)perp1 * sideClearanceDistance,
                    perp1, detectionDistance, groundMask)
                    ? perp2 : perp1;
            else
                avoidDir = obstacleNormal;
            return Vector2.Lerp(moveDir, avoidDir, avoidanceLerpWeight).normalized;
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Weapon")) return;
            SoundManager.Instance.PlaySound("Explosion", transform);
            GameEvents.AddPoints?.Invoke(pointsForKill);
            GameEvents.EnemyDestroyed?.Invoke(transform.position);
            ReturnToPool();
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
            GameEvents.FreezeLevel -= () => SetFreezeState(true);
            GameEvents.UnFreezeLevel -= () => SetFreezeState(false);
            FlyingEnemyPool.Instance.Return(gameObject.GetComponent<FlyingEnemy>());
        }
    }
}
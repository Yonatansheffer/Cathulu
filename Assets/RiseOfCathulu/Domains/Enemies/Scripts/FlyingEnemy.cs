using RiseOfCathulu.Domains.Utilities.GameHandlers.Scripts;
using RiseOfCathulu.Domains.Utilities.Sound.Scripts;
using UnityEngine;

namespace RiseOfCathulu.Domains.Enemies.Scripts
{
    public class FlyingEnemy : Enemy
    {
        private static readonly int MovingRight = Animator.StringToHash("MovingRight");

        [Header("Movement & Scaling")]
        [SerializeField, Tooltip("Speed of enemy movement")] private float moveSpeed = 3f;
        [SerializeField, Tooltip("Weight of player attraction (0-1)")] private float playerAttractionWeight = 0.55f;
        [SerializeField, Tooltip("Time scale for Perlin noise randomness")] private float noiseTimeScale = 0.3f;

        [Header("Scoring")]
        [SerializeField, Tooltip("Points awarded for destroying this enemy")] private int pointsForKill = 1;

        [Header("Avoidance")]
        [SerializeField, Tooltip("Distance for obstacle detection raycast")] private float detectionDistance = 4f;
        [SerializeField, Tooltip("Distance for side clearance raycast")] private float sideClearanceDistance = 1f;
        [SerializeField, Tooltip("Weight for blending avoidance direction")] private float avoidanceLerpWeight = 2f;

        [Header("Facing Filter")]
        [SerializeField, Tooltip("Min horizontal speed  to allow facing flip")] private float minFlipSpeed = 0.4f;
        [SerializeField, Tooltip("Horizontal X threshold to trigger flip")] private float flipHysteresis = 0.18f;
        [SerializeField, Tooltip("Min time between flips")] private float flipCooldown = 0.25f;
        
        [Header("Tether Settings")]
        private Transform _tetherTransform;
        private float _maxTetherDistance;
        private bool _isTethered = false;
        
        private Transform _playerTransform;
        private Animator _animator;
        private SpriteRenderer _spriteRenderer;
        private Rigidbody2D _rb;
        private bool _isFrozen;
        private int _facing = 1;
        private float _lastFlipTime = -999f;
        private Color _baseColor;
        
        public void SetTether(Transform center, float maxDistance)
        {
            _tetherTransform = center;
            _maxTetherDistance = maxDistance;
            _isTethered = true;
        }
        
        public void InitializeLevel(int level, GrowthConfig config)
        {
            sizeLevel = level;
            transform.localScale = Vector3.one * config.GetScale(level);
            moveSpeed = config.GetSpeed(level);
        }

        public void SetMoveSpeed(float speed) => moveSpeed = speed;
        
        public override void Reset()
        {
            // Reset state for pooling
            _isTethered = false;
            _tetherTransform = null;
            _isFrozen = false;
            _facing = 1;
            _lastFlipTime = -999f;
            
            // Reset visuals/physics
            transform.localScale = Vector3.one; 
            moveSpeed = 3f; 
            if (_animator) 
            {
                _animator.speed = 1f;
                _animator.SetBool(MovingRight, true);
            }
            if (_rb)
            {
                _rb.linearVelocity = Vector2.zero;
                _rb.angularVelocity = 0f;
            }
            if (_spriteRenderer) _spriteRenderer.flipX = false;
            var eatable = GetComponent<EnemyEatable>();
            if (eatable != null) eatable.ResetEatableState();
        }

        private void Awake()
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            _playerTransform = playerObj ? playerObj.transform : null;
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

        private void OnFreeze() => SetFreezeState(true);
        private void OnUnFreeze() => SetFreezeState(false);

        private void SetFreezeState(bool freeze)
        {
            _isFrozen = freeze;
            if (_animator) _animator.speed = freeze ? 0f : 1f;
            if (freeze && _rb) _rb.linearVelocity = Vector2.zero;
        }

        protected override void Move()
        {
            if (_isFrozen || !_playerTransform) return;
            var finalDir = CalculateDirection();
            finalDir = ApplyTetherConstraint(finalDir);
            finalDir = HandleObstacleAvoidance(finalDir);
            transform.position += finalDir * (moveSpeed * Time.deltaTime);
            UpdateFacing(finalDir);
        }
        
        private Vector3 ApplyTetherConstraint(Vector3 moveDir)
        {
            if (!_isTethered || _tetherTransform == null) return moveDir;

            Vector3 toCenter = _tetherTransform.position - transform.position;
            float currentDistance = toCenter.magnitude;

            // If we are at the edge and moving further away
            if (currentDistance > _maxTetherDistance)
            {
                float dot = Vector3.Dot(moveDir, toCenter.normalized);
                
                // If dot is negative, we are moving AWAY from the center
                if (dot < 0)
                {
                    // Blend the movement direction with a strong pull towards the center
                    // The further out they are, the harder they get pulled back
                    float pullIntensity = Mathf.Clamp01((currentDistance - _maxTetherDistance) / 2f);
                    return Vector3.Lerp(moveDir, toCenter.normalized, pullIntensity + 0.5f).normalized;
                }
            }

            return moveDir;
        }

        private void UpdateFacing(Vector3 dir)
        {
            var horizontalSpeed = Mathf.Abs(dir.x) * moveSpeed;
            if (horizontalSpeed < minFlipSpeed) return;
            if (Time.time - _lastFlipTime < flipCooldown) return;

            if (_facing < 0 && dir.x >  flipHysteresis) { _facing = 1; _lastFlipTime = Time.time; }
            if (_facing > 0 && dir.x < -flipHysteresis) { _facing = -1; _lastFlipTime = Time.time; }

            if (_animator) _animator.SetBool(MovingRight, _facing > 0);
            if (_spriteRenderer) _spriteRenderer.flipX = _facing < 0;
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
            var groundMask = LayerMask.GetMask("Ground");
            var hit = Physics2D.Raycast(transform.position, moveDir, detectionDistance, groundMask);
            if (!hit.collider) return moveDir;

            var n = hit.normal;
            var perp1 = new Vector2(-n.y, n.x);
            var perp2 = new Vector2(n.y, -n.x);

            var leftClear = !Physics2D.Raycast(transform.position, perp1, sideClearanceDistance, groundMask);
            var rightClear = !Physics2D.Raycast(transform.position, perp2, sideClearanceDistance, groundMask);

            Vector2 avoidDir;
            if (leftClear && !rightClear) avoidDir = perp1;
            else if (rightClear && !leftClear) avoidDir = perp2;
            else if (leftClear)
                avoidDir = Physics2D.Raycast(transform.position + (Vector3)perp1 * sideClearanceDistance, perp1, detectionDistance, groundMask) ? perp2 : perp1;
            else
                avoidDir = n;

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

        private void ReturnToPool()
        {
            GameEvents.FreezeLevel -= OnFreeze;
            GameEvents.UnFreezeLevel -= OnUnFreeze;
            FlyingEnemyPool.Instance.Return(GetComponent<FlyingEnemy>());
        }
        
        public void ReturnToPoolExternally()
        {
            ReturnToPool();
        }

    }
}

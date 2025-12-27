using B.O.S.S.Domains.Utilities.GameHandlers.Scripts;
using B.O.S.S.Domains.Utilities.Sound.Scripts;
using UnityEngine;

namespace B.O.S.S.Domains.Enemies.Scripts
{
    public class FlyingEnemy : Enemy
    {
        private static readonly int MovingRight = Animator.StringToHash("MovingRight");

        [Header("Movement")]
        [SerializeField, Tooltip("Speed of enemy movement")] private float moveSpeed = 3f;
        [SerializeField, Tooltip("Weight of player attraction (0-1)")] private float playerAttractionWeight = 0.55f;
        [SerializeField, Tooltip("Time scale for Perlin noise randomness")] private float noiseTimeScale = 0.3f;

        [Header("Big Variant")]
        [SerializeField, Tooltip("Scale for big enemy variant")] private float bigEnemySize = 1.8f;
        [SerializeField, Tooltip("Speed for big enemy variant")] private float bigEnemySpeed = 2.2f;

        [Header("Scoring")]
        [SerializeField, Tooltip("Points awarded for destroying this enemy")] private int pointsForKill = 1;

        [Header("Avoidance")]
        [SerializeField, Tooltip("Distance for obstacle detection raycast")] private float detectionDistance = 4f;
        [SerializeField, Tooltip("Distance for side clearance raycast")] private float sideClearanceDistance = 1f;
        [SerializeField, Tooltip("Weight for blending avoidance direction")] private float avoidanceLerpWeight = 2f;

        [Header("Facing Filter")]
        [SerializeField, Tooltip("Min horizontal speed required to allow facing flip")] private float minFlipSpeed = 0.4f;
        [SerializeField, Tooltip("Horizontal X threshold (hysteresis) to trigger flip")] private float flipHysteresis = 0.18f;
        [SerializeField, Tooltip("Min time between flips")] private float flipCooldown = 0.25f;

        private Transform _playerTransform;
        private Animator _animator;
        private SpriteRenderer _spriteRenderer;
        private Rigidbody2D _rb;
        private bool _isFrozen;

        private int _facing = 1;
        private float _lastFlipTime = -999f;
        
        private float _basePlayerAttractionWeight;
        private Color _baseColor;
        

        private void Awake()
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            _playerTransform = playerObj ? playerObj.transform : null;
            _animator = GetComponent<Animator>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _rb = GetComponent<Rigidbody2D>();
            
            _basePlayerAttractionWeight = playerAttractionWeight;
            if (_spriteRenderer)
                _baseColor = _spriteRenderer.color;
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

        private void OnFreeze() => SetFreezeState(true);
        private void OnUnFreeze() => SetFreezeState(false);

        private void SetFreezeState(bool freeze)
        {
            _isFrozen = freeze;
            if (_animator) _animator.speed = freeze ? 0f : 1f;
            if (freeze && _rb) _rb.linearVelocity = Vector2.zero;
        }

        public override void Reset()
        {
            if (_animator) _animator.speed = 1f;
            if (_rb)
            {
                _rb.linearVelocity = Vector2.zero;
                _rb.angularVelocity = 0f;
            }
            _isFrozen = false;
            _facing = 1;
            _lastFlipTime = -999f;
            if (_animator) _animator.SetBool(MovingRight, true);
            if (_spriteRenderer) _spriteRenderer.flipX = false;
            var eatable = GetComponent<EnemyEatable>();
            if (eatable != null)
            {
                eatable.ResetEatableState();
            }
        }

        protected override void Move()
        {
            if (_isFrozen || !_playerTransform) return;

            var finalDir = CalculateDirection();
            finalDir = HandleObstacleAvoidance(finalDir);

            transform.position += finalDir * (moveSpeed * Time.deltaTime);

            UpdateFacing(finalDir);
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

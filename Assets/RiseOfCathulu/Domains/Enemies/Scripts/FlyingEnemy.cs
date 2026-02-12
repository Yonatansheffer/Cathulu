using RiseOfCathulu.Domains.Player.Scripts;
using RiseOfCathulu.Domains.Utilities.GameHandlers.Scripts;
using RiseOfCathulu.Domains.Utilities.Sound.Scripts;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RiseOfCathulu.Domains.Enemies.Scripts
{
    public class FlyingEnemy : MonoBehaviour, IPoolable
    {
        private static readonly int MovingRight = Animator.StringToHash("MovingRight");
        public int sizeLevel;

        [Header("Movement")]
        [SerializeField, Tooltip("Speed of enemy movement")] private float moveSpeed = 3f;

        
        [Header("Player Attraction")]
        [SerializeField, Tooltip("Minimum of player attraction (0-1)")] private float minPlayerAttraction = 0.55f;
        [SerializeField, Tooltip("Maximum of player attraction (0-1)")] private float maxPlayerAttraction = 0.9f;
        [SerializeField, Tooltip("Time scale for Perlin noise randomness")] private float noiseTimeScale = 0.3f;
        [SerializeField] private float eatableSoftRadiusFactor = 0.75f; 

        private PlayerSize _player;
        private float _playerAttractionWeight;
        
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
        
        [Header("Particles")]
        [SerializeField, Tooltip("Stars particle prefab on death")] private GameObject orangeStarsParticles;
        [SerializeField, Tooltip("Stars particle size")] private float particlesSize;
        
        [Header("Tether Settings")]
        private Transform _tetherTransform;
        private float _maxTetherDistance;
        private float _eatableMaxTetherDistance;
        private bool _isTethered;
        private float _baseScale;
        private Transform _playerTransform;
        private Animator _animator;
        private SpriteRenderer _spriteRenderer;
        private Rigidbody2D _rb;
        private bool _isFrozen;
        private float _baseMoveSpeed;
        private int _facing = 1;
        private float _lastFlipTime = -999f;
        private float _currentAttractionSign = 1f; 
        private bool _isCurrentlyEatable;
        private EnemySpawning _ownerSpawner;
        
        public bool IsEatable => _isCurrentlyEatable;
        
        public void SetTether(Transform center, float maxDistance, float eatableMaxDistance)
        {
            _tetherTransform = center;
            _maxTetherDistance = maxDistance;
            _eatableMaxTetherDistance = eatableMaxDistance;
            _isTethered = true;
        }
        
        public void SetOwnerSpawner(EnemySpawning spawner)
        {
            _ownerSpawner = spawner;
        }
        
        public void InitializeLevel(int level, GrowthConfig config)
        {
            sizeLevel = level;
            _baseScale = config.GetScale(level);
            transform.localScale = Vector3.one * _baseScale;
            if (_ownerSpawner != null && _ownerSpawner.isOpening) transform.localScale *= 1.9f;
            float playerSpeed = config.GetMaxSpeed(_player.CurrentSizeLevel);
            float speedMultiplier = Random.Range(0.1f, 0.40f);
            if (_ownerSpawner != null && _ownerSpawner.isOpening) speedMultiplier /= 2.3f;
            _baseMoveSpeed = playerSpeed * speedMultiplier;
            moveSpeed = playerSpeed * speedMultiplier;
            _playerAttractionWeight = Random.Range(minPlayerAttraction,maxPlayerAttraction);
            if (_ownerSpawner != null && _ownerSpawner.isOpening) _playerAttractionWeight = 0.85f;
            _currentAttractionSign = 1f;
        }   

        public void SetMoveSpeed(float speed) => moveSpeed = speed;
        
        public void Reset()
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
        }

        private void Awake()
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) _player = playerObj.GetComponent<PlayerSize>();
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
        
        private void Update()
        {
            CheckTooSmall();
            UpdateEatable();
            Move();
        }
        
        private void CheckTooSmall()
        {
            if (_player == null) return;
            if (_player.CurrentSizeLevel > sizeLevel +5)
            {
                ReturnToPool();
            }
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private void UpdateEatable()
        {
            bool nowEatable = _player.CurrentSizeLevel >= sizeLevel;

            // Transition: NOT eatable → eatable
            if (nowEatable && !_isCurrentlyEatable)
            {
                _isCurrentlyEatable = true;
                _currentAttractionSign = -1f;
                moveSpeed = _baseMoveSpeed * 0.82f;
                _ownerSpawner?.NotifyEnemyBecameEatable();
            }
            // Transition: eatable → NOT eatable
            else if (!nowEatable && _isCurrentlyEatable)
            {
                _isCurrentlyEatable = false;
                _currentAttractionSign = 1f;
                moveSpeed = _baseMoveSpeed;
                _ownerSpawner?.NotifyEnemyStoppedBeingEatable();
                return;
            }

            // 🔴 Continuous rule enforcement (THIS WAS MISSING)
            if (_isCurrentlyEatable && _ownerSpawner != null
                                    && !_ownerSpawner.CanBecomeEatable() && IsInsideEatableTether())
            {
                ReturnToPool();
            }
        }
        
        private bool IsInsideEatableTether()
        {
            if (!_isTethered || _tetherTransform == null)
                return false;

            float distance = Vector3.Distance(transform.position, _tetherTransform.position);
            return distance <= _eatableMaxTetherDistance;
        }


        private void Move()
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
        if (!_isTethered || _tetherTransform == null)
            return moveDir;

        Vector3 toCenter = _tetherTransform.position - transform.position;
        float currentDistance = toCenter.magnitude;

        Vector3 toCenterDir = toCenter.normalized;
        Vector3 fromCenterDir = -toCenterDir;

        // -----------------------------
        // EATABLE BEHAVIOR (SOFT ZONE)
        // -----------------------------
        if (_isCurrentlyEatable)
        {
            float softRadius = _eatableMaxTetherDistance * eatableSoftRadiusFactor;

            // Inside soft zone → no constraint at all
            if (currentDistance < softRadius)
                return moveDir;

            // Between soft zone and hard leash → gentle pull inward
            if (currentDistance < _eatableMaxTetherDistance)
            {
                float t = Mathf.InverseLerp(
                    softRadius,
                    _eatableMaxTetherDistance,
                    currentDistance
                );

                return Vector3.Lerp(
                    moveDir,
                    toCenterDir,
                    t * 0.6f
                ).normalized;
            }

            // Outside hard leash → strong pull
            float hardPull = Mathf.Clamp01(
                (currentDistance - _eatableMaxTetherDistance) / 2f
            );

            return Vector3.Lerp(
                moveDir,
                toCenterDir,
                hardPull + 0.6f
            ).normalized;
        }

        // --------------------------------
        // NON-EATABLE (ORIGINAL BEHAVIOR)
        // --------------------------------
        if (currentDistance < _eatableMaxTetherDistance)
        {
            float pushStrength = Mathf.Clamp01(
                (_eatableMaxTetherDistance - currentDistance) / 2f
            );

            return Vector3.Lerp(
                moveDir,
                fromCenterDir,
                pushStrength + 0.5f
            ).normalized;
        }

        if (currentDistance > _maxTetherDistance)
        {
            float pullStrength = Mathf.Clamp01(
                (currentDistance - _maxTetherDistance) / 2f
            );

            return Vector3.Lerp(
                moveDir,
                toCenterDir,
                pullStrength + 0.5f
            ).normalized;
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
            // --- Random (Perlin noise) direction ---
            float timeOffset = Time.time * noiseTimeScale + GetInstanceID();
            Vector3 randomDir = new Vector3(
                Mathf.PerlinNoise(timeOffset, 0f) - 0.5f,
                Mathf.PerlinNoise(0f, timeOffset) - 0.5f,
                0f
            ).normalized;

            if (!_isTethered || _tetherTransform == null || !_playerTransform)
                return randomDir;

            // --- Awareness zone check ---
            float playerToCenterDist = Vector3.Distance(
                _playerTransform.position,
                _tetherTransform.position
            );

            bool playerInZone =
                playerToCenterDist >= _eatableMaxTetherDistance &&
                playerToCenterDist <= _maxTetherDistance;

            if (!playerInZone)
                return randomDir;

            // --- Attraction / Repulsion (SIGNED) ---
            Vector3 toPlayerDir =
                (_playerTransform.position - transform.position).normalized;

            float signedAttraction =
                _playerAttractionWeight * _currentAttractionSign;

            return (
                randomDir * (1f - Mathf.Abs(signedAttraction)) +
                toPlayerDir * signedAttraction
            ).normalized;
        }


        private Vector3 HandleObstacleAvoidance(Vector3 moveDir)
        {
            var groundMask = LayerMask.GetMask("Ground");
            var hit = Physics2D.Raycast(transform.position, moveDir, detectionDistance, groundMask);
            if (!hit.collider) return moveDir;
            var n = hit.normal;
            var perp1 = new Vector2(-n.y, n.x);
            var perp2 = new Vector2(n.y, -n.x);
            var leftClear = !Physics2D.Raycast(transform.position, 
                perp1, sideClearanceDistance, groundMask);
            var rightClear = !Physics2D.Raycast(transform.position,
                perp2, sideClearanceDistance, groundMask);
            Vector2 avoidDir;
            if (leftClear && !rightClear) avoidDir = perp1;
            else if (rightClear && !leftClear) avoidDir = perp2;
            else if (leftClear)
                avoidDir = Physics2D.Raycast(transform.position + (Vector3)perp1 * sideClearanceDistance,
                    perp1, detectionDistance, groundMask) ? perp2 : perp1;
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
            var particles = Instantiate(orangeStarsParticles, transform.position, Quaternion.identity);
            particles.transform.localScale *= _baseScale * particlesSize;
            Destroy(particles, 2f);
            ReturnToPool();
        }
        
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!collision.gameObject.CompareTag("Player")) return;
            ReturnToPool();
        }

        private void ReturnToPool()
        {
            _ownerSpawner?.NotifyEnemyReturned(_isCurrentlyEatable);
            _isCurrentlyEatable = false;
            _ownerSpawner = null;
            GameEvents.FreezeLevel -= OnFreeze;
            GameEvents.UnFreezeLevel -= OnUnFreeze;
            FlyingEnemyPool.Instance.Return(this);
        }
    }
}

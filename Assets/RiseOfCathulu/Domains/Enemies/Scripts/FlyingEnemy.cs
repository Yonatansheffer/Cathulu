using System;
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
        [SerializeField, Tooltip("Randomness factor for speed")] private float speedVariation = 0.4f;
        
        [Header("Player Attraction")]
        private PlayerSize _player;
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
        private float _eatableMaxTetherDistance;
        private bool _isTethered = false;
        
        private Transform _playerTransform;
        private Animator _animator;
        private SpriteRenderer _spriteRenderer;
        private Rigidbody2D _rb;
        private bool _isFrozen;
        private int _facing = 1;
        private float _lastFlipTime = -999f;
        private float _currentAttractionSign = 1f; // 1 = Attract, -1 = Repel
        private bool _isCurrentlyEatable = false;
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

            // Set absolute scale directly
            float targetScale = config.GetScale(level);
            transform.localScale = Vector3.one * targetScale;

            // Get the base speed from config
            float baseSpeed = config.GetSpeed(level);

            // Calculate a random multiplier (e.g., if variation is 0.15, range is 0.85 to 1.15)
            float randomMultiplier = Random.Range(1f - speedVariation, 1f + speedVariation);
            // Inside InitializeLevel
            if (_animator != null)
            {
                // If they move 20% faster, their animation plays 20% faster
                _animator.speed = randomMultiplier; 
            }
    
            // Assign the unique speed
            moveSpeed = baseSpeed * randomMultiplier;
            
            playerAttractionWeight = Random.Range(0.5f, 1.0f);
    
            // Reset sign to positive (hunting) by default
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
            UpdateEatable();
            Move();
        }

        private void UpdateEatable()
        {
            bool nowEatable = _player.CurrentSizeLevel >= sizeLevel;

            if (_isCurrentlyEatable != nowEatable)
            {
                _isCurrentlyEatable = nowEatable;
                _currentAttractionSign = _isCurrentlyEatable ? -1f : 1f;
            }
        }

        
        private bool IsPlayerWithinTetherRange()
        {
            if (!_isTethered || _tetherTransform == null || !_playerTransform)
                return true; 

            float activeMaxDistance = _isCurrentlyEatable ? _eatableMaxTetherDistance : _maxTetherDistance;
            float playerDistance = Vector3.Distance(_playerTransform.position, _tetherTransform.position);
            return playerDistance <= activeMaxDistance;
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
            if (!_isTethered || _tetherTransform == null) return moveDir;

            float activeMaxDistance = _isCurrentlyEatable
                ? _eatableMaxTetherDistance
                : _maxTetherDistance;

            Vector3 toCenter = _tetherTransform.position - transform.position;
            float currentDistance = toCenter.magnitude;

            if (currentDistance > activeMaxDistance)
            {
                float dot = Vector3.Dot(moveDir, toCenter.normalized);
                if (dot < 0)
                {
                    float pullIntensity = Mathf.Clamp01((currentDistance - activeMaxDistance) / 2f);

                    return Vector3.Lerp(moveDir, toCenter.normalized, pullIntensity + 0.5f).normalized; }
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
            var modifiedPlayerDir = playerDir * _currentAttractionSign;

            // --- Random (Perlin noise) direction ---
            var timeOffset = Time.time * noiseTimeScale + GetInstanceID();
            float noiseX = Mathf.PerlinNoise(timeOffset, 0f) - 0.5f;
            float noiseY = Mathf.PerlinNoise(0f, timeOffset) - 0.5f;
            var randomDir = new Vector3(noiseX, noiseY, 0f).normalized;

            // --- Decide attraction strength based on player distance ---
            float effectiveAttractionWeight = 0f;

            if (_isTethered && _tetherTransform != null)
            {
                float activeMaxDistance =
                    _isCurrentlyEatable ? _eatableMaxTetherDistance : _maxTetherDistance;

                float dist = Vector3.Distance(
                    _playerTransform.position,
                    _tetherTransform.position
                );

                // Smooth fade-in of attention
                float t = Mathf.InverseLerp(
                    activeMaxDistance * 1.3f, // fully random when far
                    activeMaxDistance,        // full attention when inside
                    dist
                );

                effectiveAttractionWeight = playerAttractionWeight * t;
            }

            return (
                randomDir * (1f - effectiveAttractionWeight) +
                modifiedPlayerDir * effectiveAttractionWeight
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
        
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!collision.gameObject.CompareTag("Player")) return;
            GameEvents.ChangePlayerSize?.Invoke(_isCurrentlyEatable? 1:-1);
            ReturnToPool();
        }

        private void ReturnToPool()
        {
            _ownerSpawner?.NotifyEnemyReturned();
            GameEvents.FreezeLevel -= OnFreeze;
            GameEvents.UnFreezeLevel -= OnUnFreeze;
            FlyingEnemyPool.Instance.Return(GetComponent<FlyingEnemy>());
        }
        
    }
}

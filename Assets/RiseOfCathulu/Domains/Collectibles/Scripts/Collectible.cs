using System.Collections;
using RiseOfCathulu.Domains.Player.Scripts;
using RiseOfCathulu.Domains.Utilities.GameHandlers.Scripts;
using UnityEngine;

namespace RiseOfCathulu.Domains.Collectibles.Scripts
{
    [RequireComponent(typeof(SpriteRenderer))]
    public abstract class Collectible : MonoBehaviour
    {
        [SerializeField, Tooltip("Speed at which the collectible falls")] protected float fallSpeed = 150f;
        [SerializeField, Tooltip("Speed at which the collectible falls")] protected float inPlanetFallSpeed = 2f;
        [SerializeField, Tooltip("Time before destroyed after hitting floor")] protected float timeForDestroy = 20f;
        [SerializeField, Tooltip("Duration of blinking effect before destruction")] private float blinkDuration = 3f;
        [SerializeField, Tooltip("Interval between blinks")] private float blinkInterval = 0.1f;
        
        [Header("Level & Scaling")]
        [SerializeField, Tooltip("Std deviation for normal distribution")] private float levelStdDev = 1.2f;
        [SerializeField, Tooltip("Min level offset from player")] private int minLevelOffset = -2;
        [SerializeField, Tooltip("Max level offset from player")] private int maxLevelOffset = 2;
        [SerializeField, Tooltip("Growth config scriptable object")] private GrowthConfig growthConfig;
        private int _collectibleLevel;
        private Vector3 _fallDirection;
        private PlayerSize _playerSize;
        private SpriteRenderer _spriteRenderer;
        private bool _isInPlanet;
        
        protected virtual void Awake()
        {
            _playerSize = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerSize>();    
            _spriteRenderer = GetComponent<SpriteRenderer>();InitializeScaleFromPlayer();
            InitializeScaleFromPlayer();
        }
    
        protected virtual void Update()
        {
            if (_isInPlanet)
                transform.position += _fallDirection * (inPlanetFallSpeed * Time.deltaTime);
            else
                transform.Translate(Vector3.down * (fallSpeed * Time.deltaTime));
        }
        
        private void InitializeScaleFromPlayer()
        {
            if (_playerSize == null) return;
            transform.localScale = Vector3.one * growthConfig.GetScale(_playerSize.CurrentSizeLevel);
        }
        
        public void InitializeFallTowardsPlanet(Transform target, float radius)
        {
            _isInPlanet = true;
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            transform.position = target.position + (Vector3)(randomDir * radius);
            _fallDirection = (target.position - transform.position).normalized;
        }
        

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Planet"))
            {
                StopMovement();
                StartCoroutine(StartDestroyTimer());
            }
            else if (other.CompareTag("Player"))
            {
                HandlePickup();
            }
        }
        
        protected abstract void HandlePickup();
        
        private IEnumerator StartDestroyTimer()
        {
            yield return new WaitForSeconds(timeForDestroy);
            StartCoroutine(BlinkRoutine());
            yield return new WaitForSeconds(blinkDuration);
            Destroy(gameObject);
        }

        private IEnumerator BlinkRoutine()
        {
            var endTime = Time.time + blinkDuration;
            while (Time.time < endTime)
            {
                _spriteRenderer.enabled = !_spriteRenderer.enabled;
                yield return new WaitForSeconds(blinkInterval);
            }
            _spriteRenderer.enabled = true;
        }

        public void StopMovement()
        {
            fallSpeed = 0;
            inPlanetFallSpeed = 0;
        }
    }
}
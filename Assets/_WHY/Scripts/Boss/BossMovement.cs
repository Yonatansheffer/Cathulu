using System.Collections;
using GameHandlers;
using Sound;
using UnityEngine;

namespace _WHY.Scripts.Boss
{
    public class BossMovement : WHYBaseMono
    {
        [Header("Floating Movement")]
        [SerializeField] private float moveRadius = 3f;
        [SerializeField] private float initialMoveSpeed = 1f;
        [SerializeField] private float shootingMoveSpeed = 1f;
        [SerializeField] private float idleTiltAngle = 15f;
        [SerializeField] private float tiltSpeed = 2f;

        [Header("X Movement Range")]
        [SerializeField] private float minX = -5f;
        [SerializeField] private float maxX = 5f;
        
        [SerializeField] private GameObject orangeStarsParticles;

        private Vector3 _startPosition;
        private Vector3 _targetPosition;
        private float _moveSpeed;
        private bool _movingRight = true;
        private bool _isFrozen = false;

        private void Start()
        {
            _moveSpeed = initialMoveSpeed;
            _startPosition = transform.position;
            PickNewTargetPosition();
        }

        private void OnEnable()
        {
            GameEvents.FreezeLevel += OnFreeze;
            GameEvents.UnFreezeLevel += OnUnFreeze;
            GameEvents.BossDestroyed += DestroyMovement;
        }

        private void OnDisable()
        {
            GameEvents.FreezeLevel -= OnFreeze;
            GameEvents.UnFreezeLevel -= OnUnFreeze;
            GameEvents.BossDestroyed -= DestroyMovement;
        }
        
        private void DestroyMovement()
        {
            StartCoroutine(ShakeAndDestroy());
        }

        private IEnumerator ShakeAndDestroy()
        {
            yield return new WaitForSeconds(1f);
            transform.rotation = Quaternion.identity;
            float duration = 4f;
            float elapsed = 0f;
            float startTilt = 30f; // start more intense
            float endTilt = 7f;    // end with a small shake
            float frequency = 50f; // much faster
            while (elapsed < duration)
            {
                SoundManager.Instance.PlaySound("Boss Damage", transform);
                float currentTilt = Mathf.Lerp(startTilt, endTilt, elapsed / duration);
                float angle = Mathf.Sin(Time.time * frequency) * currentTilt;
                transform.rotation = Quaternion.Euler(0f, 0f, angle);
                elapsed += Time.deltaTime;
                yield return null;
            }
            transform.rotation = Quaternion.identity;
            var particles = Instantiate(orangeStarsParticles, transform.position, Quaternion.identity);
            particles.transform.localScale *= 3.5f; // doubles the size
            Destroy(particles, 2f);
            GameEvents.BossEndedDeath?.Invoke();
            SoundManager.Instance.PlaySound("Explosion", transform);
            Destroy(gameObject);
        }



        private void Update()
        {
            if (_isFrozen) return;

            if (BossShooting.GetBossState() != BossShooting.BossState.Idle)
            {
                _moveSpeed = shootingMoveSpeed;
                HandleShootingMovement();
                return;
            }

            _moveSpeed = initialMoveSpeed;
            HandleFloatingMovement();
            HandleIdleTilt();
        }

        private void HandleFloatingMovement()
        {
            transform.position = Vector3.MoveTowards(transform.position,
                _targetPosition, _moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, _targetPosition) < 0.1f)
            {
                PickNewTargetPosition();
            }
        }

        private void PickNewTargetPosition()
        {
            var randomX = Random.Range(minX, maxX);
            var randomYOffset = Random.Range(-moveRadius, moveRadius);
            var newY = _startPosition.y + randomYOffset;
            _targetPosition = new Vector3(randomX, newY, _startPosition.z);
        }

        private void HandleShootingMovement()
        {
            var pos = transform.position;
            var direction = _movingRight ? 1f : -1f;
            pos.x += direction * _moveSpeed * Time.deltaTime;

            if (pos.x >= maxX)
            {
                pos.x = maxX;
                _movingRight = false;
            }
            else if (pos.x <= minX)
            {
                pos.x = minX;
                _movingRight = true;
            }

            transform.position = pos;
        }

        private void HandleIdleTilt()
        {
            var angle = Mathf.Sin(Time.time * tiltSpeed) * idleTiltAngle;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        private void OnFreeze() => _isFrozen = true;

        private void OnUnFreeze() => _isFrozen = false;
    }
}

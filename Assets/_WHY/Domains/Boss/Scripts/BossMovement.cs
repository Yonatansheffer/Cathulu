using System.Collections;
using _WHY.Domains.Utilities.GameHandlers.Scripts;
using _WHY.Domains.Utilities.Sound.Scripts;
using UnityEngine;

namespace _WHY.Domains.Boss.Scripts
{
    public class BossMovement : WHYBaseMono
    {
        [Header("Floating Movement")]
        [SerializeField, Tooltip("Radius for vertical floating around start position")]
        private float moveRadius = 3f;
        [SerializeField, Tooltip("Horizontal speed while idle")]
        private float initialMoveSpeed = 1f;
        [SerializeField, Tooltip("Horizontal speed while shooting")]
        private float shootingMoveSpeed = 1f;
        [SerializeField, Tooltip("Max tilt angle while idle")]
        private float idleTiltAngle = 15f;
        [SerializeField, Tooltip("Tilt oscillation speed while idle")]
        private float tiltSpeed = 2f;

        [Header("X Movement Range")]
        [SerializeField, Tooltip("Minimum X position")]
        private float minX = -5f;
        [SerializeField, Tooltip("Maximum X position")]
        private float maxX = 5f;

        [Header("FX")]
        [SerializeField, Tooltip("Stars particle prefab on death")]
        private GameObject orangeStarsParticles;

        private Vector3 _startPosition;
        private Vector3 _targetPosition;
        private float _moveSpeed;
        private bool _movingRight = true;
        private bool _isFrozen;

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
            var duration = 2.7f;
            var elapsed = 0f;
            var startTilt = 40f;
            var endTilt = 7f;
            var frequency = 45f;
            while (elapsed < duration)
            {
                SoundManager.Instance.PlaySound("Boss Damage", transform);
                var currentTilt = Mathf.Lerp(startTilt, endTilt, elapsed / duration);
                var angle = Mathf.Sin(Time.time * frequency) * currentTilt;
                transform.rotation = Quaternion.Euler(0f, 0f, angle);
                elapsed += Time.deltaTime;
                yield return null;
            }
            transform.rotation = Quaternion.identity;
            var particles = Instantiate(orangeStarsParticles, transform.position, Quaternion.identity);
            particles.transform.localScale *= 3.5f;
            Destroy(particles, 2f);
            GameEvents.BossEndedDeath?.Invoke();
            SoundManager.Instance.PlaySound("Explosion", transform);
            Destroy(gameObject);
        }

        private void Update()
        {
            if (_isFrozen) return;

            var state = BossShooting.GetBossState();
            if (state != BossShooting.BossState.Idle)
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
            transform.position = Vector3.MoveTowards(
                transform.position, _targetPosition, _moveSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, _targetPosition) < 0.1f) PickNewTargetPosition();
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

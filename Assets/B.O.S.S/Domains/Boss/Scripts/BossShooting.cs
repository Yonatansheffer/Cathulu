using System.Collections;
using _WHY.Domains.Boss.Scripts;
using B.O.S.S.Domains.Utilities.GameHandlers.Scripts;
using B.O.S.S.Domains.Utilities.Sound.Scripts;
using UnityEngine;

namespace B.O.S.S.Domains.Boss.Scripts
{
    public class BossShooting : BossBaseMono
    {
        public enum BossState { Idle, RotatingIn, Shooting, RotatingBack, Death }

        [Header("Rotation & Shooting")]
        [SerializeField, Tooltip("Rotation speed while aiming")] private float rotationSpeed = 30f;
        [SerializeField, Tooltip("Bullets per second")] private float bulletsPerSecond = 5f;
        [SerializeField, Tooltip("Bullet launch force")] private float bulletForce = 20f;
        [SerializeField, Tooltip("Shooting phase duration")] private float shootingDuration = 8f;
        [SerializeField, Tooltip("Delay before shooting starts")] private float startShootDelay = 0.8f;

        [Header("Player Target")]
        [SerializeField, Tooltip("Player GameObject to target")] private GameObject player;

        [Header("Ball Shot")]
        [SerializeField, Tooltip("Distance threshold for ball shot")] private float ballShootDistance = 120f;
        [SerializeField, Tooltip("Cooldown for ball shot")] private float ballShootCooldown = 3f;

        private static BossState _currentState = BossState.Idle;
        private float _shootTimer;
        private float _shootingElapsed;
        private float _accumulatedRotation;
        private float _lastBallShootTime = -999f;
        private bool _isFrozen;

        public static BossState GetBossState() => _currentState;

        private void Start()
        {
            _currentState = BossState.Idle;
        }

        private void OnEnable()
        {
            GameEvents.BossShoots += OnBossShoots;
            GameEvents.BossDestroyed += OnBossDestroyed;
            GameEvents.FreezeLevel += OnFreeze;
            GameEvents.UnFreezeLevel += OnUnFreeze;
        }

        private void OnDisable()
        {
            GameEvents.BossShoots -= OnBossShoots;
            GameEvents.BossDestroyed -= OnBossDestroyed;
            GameEvents.FreezeLevel -= OnFreeze;
            GameEvents.UnFreezeLevel -= OnUnFreeze;
        }

        private void OnFreeze() => _isFrozen = true;
        private void OnUnFreeze() => _isFrozen = false;

        private void OnBossDestroyed()
        {
            _currentState = BossState.Death;
            _isFrozen = true;
        }

        private void OnBossShoots()
        {
            if (_currentState != BossState.Idle) return;
            StartCoroutine(BeginShootAfterDelay(startShootDelay));
        }

        private IEnumerator BeginShootAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            SoundManager.Instance.PlaySound("Boss Shoot", transform);
            ResetShootingState();
            _currentState = BossState.RotatingIn;
        }

        private void Update()
        {
            if (_isFrozen) return;

            CheckProximityAttack();

            switch (_currentState)
            {
                case BossState.RotatingIn:
                    HandleRotatingIn();
                    break;
                case BossState.Shooting:
                    HandleShooting();
                    break;
                case BossState.RotatingBack:
                    HandleRotatingBack();
                    break;
            }
        }

        private void HandleRotatingIn()
        {
            var step = rotationSpeed * Time.deltaTime;
            transform.Rotate(0f, 0f, -step);
            _accumulatedRotation += step;

            if (_accumulatedRotation >= 90f)
            {
                transform.rotation = Quaternion.Euler(0f, 0f, -90f);
                ResetShootingState();
                _currentState = BossState.Shooting;
            }
        }

        private void HandleShooting()
        {
            _shootingElapsed += Time.deltaTime;
            _shootTimer += Time.deltaTime;

            var interval = 1f / bulletsPerSecond;
            while (_shootTimer >= interval)
            {
                Shoot();
                _shootTimer -= interval;
            }

            if (_shootingElapsed >= shootingDuration)
                _currentState = BossState.RotatingBack;
        }

        private void HandleRotatingBack()
        {
            var step = rotationSpeed * Time.deltaTime;
            var z = Mathf.MoveTowardsAngle(transform.eulerAngles.z, 0f, step);
            transform.rotation = Quaternion.Euler(0f, 0f, z);

            if (Mathf.Approximately(z, 0f))
                _currentState = BossState.Idle;
        }

        private void CheckProximityAttack()
        {
            if (player == null) return;

            var distance = Vector2.Distance(transform.position, player.transform.position);
            if (distance > ballShootDistance) return;

            var cd = distance <= ballShootDistance * 0.5f ? ballShootCooldown * 0.5f : ballShootCooldown;
            if (Time.time - _lastBallShootTime >= cd)
            {
                ShootBallBullet();
                _lastBallShootTime = Time.time;
            }
        }

        private void Shoot()
        {
            var bullet = BossBulletPool.Instance.Get();
            bullet.transform.position = transform.position;
            var rb = bullet.GetComponent<Rigidbody2D>();
            rb.linearVelocity = (Vector2)transform.right * bulletForce;
        }

        private void ShootBallBullet()
        {
            var bullet = BossBallBulletPool.Instance.Get();
            bullet.transform.position = transform.position;
            var dir = (player.transform.position - transform.position).normalized;
            var rb = bullet.GetComponent<Rigidbody2D>();
            rb.linearVelocity = dir * bulletForce;
            SoundManager.Instance.PlaySound("Boss Bullet", transform);
        }

        private void ResetShootingState()
        {
            _shootTimer = 0f;
            _shootingElapsed = 0f;
            _accumulatedRotation = 0f;
        }
    }
}

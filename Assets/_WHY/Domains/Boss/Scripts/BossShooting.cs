using System.Collections;
using _WHY.Domains.Utilities.GameHandlers.Scripts;
using Sound;
using UnityEngine;

namespace _WHY.Domains.Boss.Scripts
{
    public class BossShooting : WHYBaseMono
    {
        public enum BossState { Idle, RotatingIn, Shooting, RotatingBack, Death }

        [Header("Shooting")]
        [SerializeField] private float rotationSpeed = 30f;
        [SerializeField] private float bulletsPerSecond = 5f;
        [SerializeField] private float bulletForce = 20f;
        [SerializeField] private GameObject player;
        [SerializeField] private float shootingDuration = 8f; 
        [SerializeField] private float ballShootDistance = 120f; 
        [SerializeField] private float ballShootCooldown = 3f; 
        private float _shootTimer;
        private float _totalRotation;
        private float _shootingElapsed;
        private bool _isFrozen;
        private float _lastBallShootTime = -999f; 
        private static BossState _currentState = BossState.Idle;

        public static BossState GetBossState() => _currentState;
        
        private void Start()
        {
            _currentState = BossState.Idle;
        }

        private void OnEnable()
        {
            GameEvents.BossShoots += StartShooting;
            GameEvents.BossDestroyed += UpdateDestroyedState;
            GameEvents.FreezeLevel += () => _isFrozen = true;
            GameEvents.UnFreezeLevel += () => _isFrozen = false;
        }

        private void OnDisable()
        {
            GameEvents.BossShoots -= StartShooting;
            GameEvents.FreezeLevel -= () => _isFrozen = true;
            GameEvents.BossDestroyed -= UpdateDestroyedState;
            GameEvents.UnFreezeLevel -= () => _isFrozen = false;
        }
        
        private void UpdateDestroyedState()
        {
            _currentState = BossState.Death;
            _isFrozen = true;
        }

       
        private void Update()
        {
            if (_isFrozen) return;
            CheckProximityAttack(); 
            switch (_currentState)
            {
                case BossState.RotatingIn:
                    RotateIn();
                    break;
                case BossState.Shooting:
                    _shootingElapsed += Time.deltaTime;
                    ShootingRoutine();
                    if (_shootingElapsed >= shootingDuration)
                        _currentState = BossState.RotatingBack;
                    break;
                case BossState.RotatingBack:
                    RotateBackToZero();
                    break;
            }
        }

        private void CheckProximityAttack()
        {
            if (player == null) return;
            var distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
            // If close enough and cooldown passed
            if (!(distanceToPlayer <= ballShootDistance) ||
                !(Time.time - _lastBallShootTime >= ballShootCooldown)) return;
            ShootBallBullet();
            _lastBallShootTime = Time.time;
        }

        private void StartShooting()
        {
            if (_currentState != BossState.Idle) return;
            StartCoroutine(StartShootingWithDelay(0.8f));
        }

        private IEnumerator StartShootingWithDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            SoundManager.Instance.PlaySound("Boss Shoot", transform);
            _shootTimer = 0f;
            _totalRotation = 0f;
            transform.rotation = Quaternion.identity;
            _currentState = BossState.RotatingIn;
        }

        private void RotateIn()
        {
            var rotateStep = rotationSpeed * Time.deltaTime;
            transform.Rotate(0f, 0f, -rotateStep); 
            _totalRotation += rotateStep;
            if (!(_totalRotation >= 90f)) return;
            transform.rotation = Quaternion.Euler(0f, 0f, -90f);
            _totalRotation = 0f;
            _shootingElapsed = 0f;
            _shootTimer = 0f;
            _currentState = BossState.Shooting;
        }

        private void RotateBackToZero()
        {
            var currentZ = transform.eulerAngles.z;
            var rotateStep = rotationSpeed * Time.deltaTime;
            var newZ = Mathf.MoveTowardsAngle(currentZ, 0f, rotateStep);
            transform.rotation = Quaternion.Euler(0f, 0f, newZ);
            if (Mathf.Approximately(newZ, 0f)) _currentState = BossState.Idle;
        }


        private void ShootingRoutine()
        {
            _shootTimer += Time.deltaTime;
            var shootInterval = 1f / bulletsPerSecond;
            while (_shootTimer >= shootInterval)
            {
                Shoot();
                _shootTimer -= shootInterval;
            }
        }

        private void Shoot()
        {
            var bullet = BossBulletPool.Instance.Get();
            bullet.transform.position = transform.position;
            Vector2 shootDirection = transform.right;
            var rb = bullet.GetComponent<Rigidbody2D>();
            rb.linearVelocity = shootDirection * bulletForce;
        }

        private void ShootBallBullet()
        {
            SoundManager.Instance.PlaySound("Boss Bullet", transform);
            var bullet = BossBallBulletPool.Instance.Get();
            bullet.transform.position = transform.position;
            Vector2 shootDirection = (player.transform.position - transform.position).normalized;
            var rb = bullet.GetComponent<Rigidbody2D>();
            rb.linearVelocity = shootDirection * bulletForce;
        }
    }
}

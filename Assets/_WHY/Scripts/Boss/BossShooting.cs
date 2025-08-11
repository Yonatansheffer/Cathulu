using GameHandlers;
using UnityEngine;
using System.Collections;
using Sound;
namespace _WHY.Scripts.Boss
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
        [SerializeField] private float ballShootDistance = 30f; // Distance to trigger ball shot
        [SerializeField] private float ballShootCooldown = 2f; // Seconds
        private float shootTimer = 0f;
        private float totalRotation = 0f;
        private float _shootingElapsed = 0f;
        private bool _isFrozen = false;
       
        private float _lastBallShootTime = -999f; // Track cooldown
        private static BossState _currentState = BossState.Idle;

        public static BossState GetBossState() => _currentState;
        
        private void Start()
        {
            _currentState = BossState.Idle;
        }

        private void OnEnable()
        {
            GameEvents.BossShoots += StartShooting;
            GameEvents.ShootBallBullet += ShootBallBullet;
            GameEvents.BossDestroyed += UpdateDestroyedState;
            GameEvents.FreezeLevel += () => _isFrozen = true;
            GameEvents.UnFreezeLevel += () => _isFrozen = false;
        }

        private void OnDisable()
        {
            GameEvents.BossShoots -= StartShooting;
            GameEvents.ShootBallBullet -= ShootBallBullet;
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

            float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);

            // If close enough and cooldown passed
            if (distanceToPlayer <= ballShootDistance && 
                Time.time - _lastBallShootTime >= ballShootCooldown)
            {
                ShootBallBullet();
                _lastBallShootTime = Time.time;
            }
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
            shootTimer = 0f;
            totalRotation = 0f;
            transform.rotation = Quaternion.identity;
            _currentState = BossState.RotatingIn;
        }

        private void RotateIn()
        {
            float rotateStep = rotationSpeed * Time.deltaTime;
            transform.Rotate(0f, 0f, -rotateStep); 
            totalRotation += rotateStep;

            if (totalRotation >= 90f)
            {
                transform.rotation = Quaternion.Euler(0f, 0f, -90f);
                totalRotation = 0f;
                _shootingElapsed = 0f;
                shootTimer = 0f;
                _currentState = BossState.Shooting;
            }
        }

        private void RotateBackToZero()
        {
            float currentZ = transform.eulerAngles.z;
            float rotateStep = rotationSpeed * Time.deltaTime;
            float newZ = Mathf.MoveTowardsAngle(currentZ, 0f, rotateStep);
            transform.rotation = Quaternion.Euler(0f, 0f, newZ);

            if (Mathf.Approximately(newZ, 0f))
                _currentState = BossState.Idle;
        }


        private void ShootingRoutine()
        {
            shootTimer += Time.deltaTime;
            float shootInterval = 1f / bulletsPerSecond;

            while (shootTimer >= shootInterval)
            {
                Shoot();
                shootTimer -= shootInterval;
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

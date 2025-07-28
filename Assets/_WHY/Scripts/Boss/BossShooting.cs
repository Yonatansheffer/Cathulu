using _WHY.Scripts.Enemies;
using GameHandlers;
using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

namespace _WHY.Scripts.Boss
{
    public class BossShooting : WHYBaseMono
    {
        [Header("Enemy Spawning")]
        [SerializeField] private float minSpawnForce = 4f;
        [SerializeField] private float maxSpawnForce = 40f;
        [SerializeField] private Vector3 spawnOffset = new Vector3(0.8f, 0f, 0f);
        [SerializeField] private Transform[] enemyTargetPositions;

        [Header("Shooting")]
        [SerializeField] private float rotationSpeed = 30f;
        [SerializeField] private float bulletsPerSecond = 5f;
        [SerializeField] private float bulletForce = 20f;

        [Header("Floating Movement")]
        [SerializeField] private float moveRadius = 3f;
        [SerializeField] private float moveSpeed = 1f;
        [SerializeField] private float idleTiltAngle = 15f;
        [SerializeField] private float tiltSpeed = 2f;
        
        
        [SerializeField] private GameObject player;
        private float shootTimer = 0f;
        private bool isShooting = false;
        private float totalRotation = 0f;
        private float lastAngle = 0f;
        private float rotationDirection = 1f; // 1 for clockwise, -1 for counterclockwise


        private Vector3 startPosition;
        private Vector3 targetPosition;

        private void Start()
        {
            startPosition = transform.position;
            PickNewTargetPosition();
        }

        private void OnEnable()
        {
            GameEvents.BossShoots += StartShooting;
            GameEvents.SpawnAllEnemies += OnSpawnAllEnemies;
            GameEvents.BossLivesChanged += BossHealthWaves;
        }

        private void OnDisable()
        {
            GameEvents.SpawnAllEnemies -= OnSpawnAllEnemies;
            GameEvents.BossLivesChanged -= BossHealthWaves;
            GameEvents.BossShoots -= StartShooting;
        }
        
        private void Update()
        {
            if (isShooting)
            {
                HandleShootingRotation();
                ShootingRoutine();
            }
            else
            {
                HandleIdleTilt();
                HandleFloatingMovement();
            }
        }
        
        private void StartShooting()
        {
            if (isShooting) return;
            rotationDirection = Random.value > 0.5f ? 1f : -1f;
            StartCoroutine(StartShootingWithDelay(0.8f));
        }

        private IEnumerator StartShootingWithDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            isShooting = true;
            shootTimer = 0f;
            totalRotation = 0f;
            lastAngle = transform.eulerAngles.z;
            transform.rotation = Quaternion.identity;
        }

        private void HandleShootingRotation()
        {
            float currentAngle = transform.eulerAngles.z;
            float deltaRotation = Mathf.DeltaAngle(lastAngle, currentAngle);
            totalRotation += Mathf.Abs(deltaRotation);
            lastAngle = currentAngle;
            if (totalRotation >= 400f)
            {
                isShooting = false;
                return;
            }
            transform.Rotate(0f, 0f, rotationSpeed * rotationDirection * Time.deltaTime);
        }


        private void ShootingRoutine()
        {
            shootTimer += Time.deltaTime;
            float shootInterval = 1f / bulletsPerSecond;

            while (shootTimer >= shootInterval)
            {
                Shoot(true);
                shootTimer -= shootInterval;
            }
        }

        private void Shoot(bool isRoutine)
        {
            var bullet = BossBulletPool.Instance.Get();
            bullet.transform.position = transform.position;
            Vector2 shootDirection = isRoutine?
                transform.right: (player.transform.position - transform.position).normalized;
            var rb = bullet.GetComponent<Rigidbody2D>();
            rb.linearVelocity = shootDirection * bulletForce;
        }
        
        private void HandleFloatingMovement()
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                PickNewTargetPosition();
            }
        }

        private void PickNewTargetPosition()
        {
            var randomOffset = Random.insideUnitCircle * moveRadius;
            targetPosition = startPosition + new Vector3(randomOffset.x, randomOffset.y, 0f);
        }

        private void HandleIdleTilt()
        {
            float angle = Mathf.Sin(Time.time * tiltSpeed) * idleTiltAngle;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
        
        public void EnemySpawn()
        {
            Shoot(false);
            var isFlyingEnemy = Random.value > 0.5f;
            Enemy spawnedEnemy = isFlyingEnemy ? FlyingEnemyPool.Instance.Get() : WalkingEnemyPool.Instance.Get();
            spawnedEnemy.transform.position = transform.position + spawnOffset;
            ApplyRandomForce(spawnedEnemy);
            if (isFlyingEnemy) return;
            var targetTransform = enemyTargetPositions[Random.Range(0, enemyTargetPositions.Length)];
            var enemyTargetPosition = targetTransform.parent != null
                ? targetTransform.parent.TransformPoint(targetTransform.localPosition)
                : targetTransform.position;
            spawnedEnemy.ToTarget(enemyTargetPosition);
        }


        private void ApplyRandomForce(Enemy enemy)
        {
            var rb = enemy.GetComponent<Rigidbody2D>();
            var direction = Random.insideUnitCircle.normalized;
            var force = Random.Range(minSpawnForce, maxSpawnForce);
            rb.AddForce(direction * force, ForceMode2D.Impulse);
        }

        private void BossHealthWaves(int currentHealth)
        {
            switch (currentHealth)
            {
                case 15:
                    SpawnFlyingEnemies(3);
                    GameEvents.EnemySpawned?.Invoke();
                    break;
                case 10:
                    GameEvents.SpawnAllEnemies?.Invoke();
                    break;
                case 7:
                    SpawnFlyingEnemies(5);
                    GameEvents.EnemySpawned?.Invoke();
                    break;
                case 5:
                    SpawnFlyingEnemies(5);
                    GameEvents.EnemySpawned?.Invoke();
                    break;
                case 3:
                    GameEvents.SpawnAllEnemies?.Invoke();
                    break;
                case 1:
                    SpawnFlyingEnemies(8);
                    GameEvents.EnemySpawned?.Invoke();
                    break;
            }
        } 
        
        private void OnSpawnAllEnemies()
        {
            foreach (var targetTransform in enemyTargetPositions)
            {
                var walkingEnemy = WalkingEnemyPool.Instance.Get();
                walkingEnemy.transform.position = transform.position + spawnOffset;
                ApplyRandomForce(walkingEnemy);
                var target = targetTransform.parent != null
                    ? targetTransform.parent.TransformPoint(targetTransform.localPosition)
                    : targetTransform.position;
                walkingEnemy.ToTarget(target);
            }
            SpawnFlyingEnemies(5);
        }

        private void SpawnFlyingEnemies(int amount)
        {
            for (var i = 0; i < amount; i++)
            {
                var flyingEnemy = FlyingEnemyPool.Instance.Get();
                flyingEnemy.transform.position = transform.position + spawnOffset;
                ApplyRandomForce(flyingEnemy);
            }
        }
    }
}

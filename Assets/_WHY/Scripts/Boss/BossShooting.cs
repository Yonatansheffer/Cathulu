using System;
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

        [Header("Shooting")]
        [SerializeField] private float rotationSpeed = 30f;
        [SerializeField] private float bulletsPerSecond = 5f;
        [SerializeField] private float bulletForce = 20f;

        [Header("Floating Movement")]
        [SerializeField] private float moveRadius = 3f;
        [SerializeField] private float moveSpeed = 1f;
        [SerializeField] private float idleTiltAngle = 15f;
        [SerializeField] private float tiltSpeed = 2f;

        private float shootTimer = 0f;
        private bool isShooting = false;
        private float totalRotation = 0f;
        private float lastAngle = 0f;

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
            GameEvents.ToSpawnEnemy += EnemySpawn;
        }

        private void OnDisable()
        {
            GameEvents.BossShoots -= StartShooting;
            GameEvents.ToSpawnEnemy -= EnemySpawn;
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

            if (totalRotation >= 360f)
            {
                isShooting = false;
                return;
            }
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
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
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
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
            Vector2 randomOffset = Random.insideUnitCircle * moveRadius;
            targetPosition = startPosition + new Vector3(randomOffset.x, randomOffset.y, 0f);
        }

        private void HandleIdleTilt()
        {
            float angle = Mathf.Sin(Time.time * tiltSpeed) * idleTiltAngle;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
        
        private void EnemySpawn(bool isFlyingEnemy)
        {
            Enemy spawnedEnemy;
            if (isFlyingEnemy)
            {
                spawnedEnemy = FlyingEnemyPool.Instance.Get();
            }
            else
            {
                spawnedEnemy = WalkingEnemyPool.Instance.Get();
            }

            spawnedEnemy.transform.position = transform.position + spawnOffset;
            ApplyRandomForce(spawnedEnemy);
        }

        private void ApplyRandomForce(Enemy enemy)
        {
            Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            float force = Random.Range(minSpawnForce, maxSpawnForce);
            rb.AddForce(randomDirection * force, ForceMode2D.Impulse);
        }
    }
}

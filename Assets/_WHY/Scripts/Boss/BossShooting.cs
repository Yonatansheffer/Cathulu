using _WHY.Scripts.Enemies;
using GameHandlers;
using UnityEngine;
using System.Collections;

namespace _WHY.Scripts.Boss
{
    public class BossShooting : WHYBaseMono
    {
        [Header("Enemy Spawning")]
        [SerializeField] private float minSpawnForce = 4f;
        [SerializeField] private float maxSpawnForce = 60f;
        [SerializeField] private Vector3 spawnOffset = new Vector3(0.8f, 0f, 0f);

        [Header("Shooting")]
        [SerializeField] private float rotationSpeed = 30f;    
        [SerializeField] private float bulletsPerSecond = 5f;
        [SerializeField] private float bulletForce = 20f;
        [SerializeField] private float shootingDuration = 8f;     

        private float shootTimer = 0f;
        private bool isShooting = false;

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
            if (!isShooting) return;
            if(transform.rotation.z is > 355f and < 360f)
            {
                isShooting = false;
            }
            ShootingRoutine();
        }

        private void ShootingRoutine()
        {
            // Rotate boss
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);


            // Shoot bullets at a fixed interval
            shootTimer += Time.deltaTime;
            float shootInterval = 1f / bulletsPerSecond;
            while (shootTimer >= shootInterval)
            {
                Shoot();
                shootTimer -= shootInterval;
            }
        }

        private void StartShooting()
        {
            if (isShooting) return;

            isShooting = true;
            shootTimer = 0f;
        }

        private void Shoot()
        {
            var bullet = BossBulletPool.Instance.Get();
            bullet.transform.position = transform.position;
            Vector2 shootDirection = transform.right;
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            rb.linearVelocity = shootDirection * bulletForce;
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
                spawnedEnemy = CrawlingEnemyPool.Instance.Get();
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

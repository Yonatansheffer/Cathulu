using _WHY.Scripts.Enemies;
using GameHandlers;
using UnityEngine;

namespace _WHY.Scripts.Boss
{
    public class BossShooting :WHYBaseMono
    {
        [SerializeField] private float minSpawnForce = 4f;
        [SerializeField] private float maxSpawnForce = 10f;
        [SerializeField] private Vector3 spawnOffset = new Vector3(0.8f, 0f, 0f);
        [SerializeField] private float rotationSpeed = 30f; // degrees per second

        
        private void OnEnable()
        {
            GameEvents.BossShoots += Shoot;
            GameEvents.ToSpawnEnemy += EnemySpawn;
        }
        
        private void OnDisable()
        {
            GameEvents.ToSpawnEnemy -= EnemySpawn;
            GameEvents.BossShoots -= Shoot;
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
            Vector2 randomDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-0.5f, 1f)).normalized;
            float force = Random.Range(minSpawnForce, maxSpawnForce);
            rb.AddForce(randomDirection * force, ForceMode2D.Impulse);
        }

        private void Shoot()
        {
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        }
        
    }
}
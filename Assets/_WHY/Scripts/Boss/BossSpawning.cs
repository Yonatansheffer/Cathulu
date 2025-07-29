using _WHY.Scripts.Enemies;
using GameHandlers;
using UnityEngine;

namespace _WHY.Scripts.Boss
{
    public class BossSpawning : WHYBaseMono
    {
        [Header("Enemy Spawning")]
        [SerializeField] private float minSpawnForce = 4f;
        [SerializeField] private float maxSpawnForce = 40f;
        [SerializeField] private Vector3 spawnOffset = new Vector3(0.8f, 0f, 0f);
        [SerializeField] private float chanceOfBigEnemy = 9f;
        [SerializeField] private Transform[] enemyTargetPositions;
        
        private void OnEnable()
        {
            GameEvents.SpawnAllWalkingEnemies += SpawnAllWalkingEnemies;
            GameEvents.BossLivesChanged += BossHealthWaves;
        }

        private void OnDisable()
        {
            GameEvents.SpawnAllWalkingEnemies -= SpawnAllWalkingEnemies;
            GameEvents.BossLivesChanged -= BossHealthWaves;
        }
        public void EnemySpawnRoutine()
        {
            GameEvents.ShootBallBullet?.Invoke();
            if (Random.value > 0.5f)
            {
                SpawnFlyingEnemies(1);
                return;
            }
            var targetTransform = enemyTargetPositions[Random.Range(0, enemyTargetPositions.Length)];
            SpawnWalkingEnemy(targetTransform);
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
                    GameEvents.SpawnAllWalkingEnemies?.Invoke();
                    SpawnFlyingEnemies(3);
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
                    GameEvents.SpawnAllWalkingEnemies?.Invoke();
                    SpawnFlyingEnemies(5);
                    break;
                case 1:
                    SpawnFlyingEnemies(8);
                    GameEvents.EnemySpawned?.Invoke();
                    break;
            }
        } 
        
        private void SpawnAllWalkingEnemies()
        {
            foreach (var targetTransform in enemyTargetPositions)
            {
                SpawnWalkingEnemy(targetTransform);
            }
        }

        private void SpawnWalkingEnemy(Transform targetTransform)
        {
            var walkingEnemy = WalkingEnemyPool.Instance.Get();
            walkingEnemy.transform.position = transform.position + spawnOffset;
            ApplyRandomForce(walkingEnemy);
            var target = targetTransform.parent != null
                ? targetTransform.parent.TransformPoint(targetTransform.localPosition)
                : targetTransform.position;
            walkingEnemy.ToTarget(target);
        }

        private void SpawnFlyingEnemies(int amount)
        {
            for (var i = 0; i < amount; i++)
            {
                var flyingEnemy = FlyingEnemyPool.Instance.Get();
                if (Random.Range(0, 100) < chanceOfBigEnemy)
                {
                    flyingEnemy.SetBigEnemy();
                }
                flyingEnemy.transform.position = transform.position + spawnOffset;
                ApplyRandomForce(flyingEnemy);
            }
        }
    }
}
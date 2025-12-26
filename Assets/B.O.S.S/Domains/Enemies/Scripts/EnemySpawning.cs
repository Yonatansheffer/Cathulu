using System.Collections;
using B.O.S.S.Domains.Enemies.Scripts;
using B.O.S.S.Domains.Utilities.GameHandlers.Scripts;
using UnityEngine;

namespace B.O.S.S.Domains.Boss.Scripts
{
    public class EnemySpawning : BossBaseMono
    {
        [Header("Enemy Spawning")]
        [SerializeField, Tooltip("Minimum impulse force applied to spawned enemies")]
        private float minSpawnForce = 4f;
        [SerializeField, Tooltip("Maximum impulse force applied to spawned enemies")]
        private float maxSpawnForce = 40f;
        [SerializeField, Tooltip("Chance (0-100) for a flying enemy to be big")]
        private float chanceOfBigEnemy = 9f;
        [SerializeField, Tooltip("Walking enemies target positions")]
        private Transform[] enemyTargetPositions;
        [SerializeField] private Collider2D gameAreaCollider;


        private void OnEnable()
        {
            /*GameEvents.SpawnAllWalkingEnemies += SpawnAllWalkingEnemies;
            GameEvents.BossLivesChanged += BossHealthWaves;*/
            GameEvents.ToSpawnEnemy += EnemySpawnRoutine;
        }

        private void OnDisable()
        {
            /*GameEvents.SpawnAllWalkingEnemies -= SpawnAllWalkingEnemies;
            GameEvents.BossLivesChanged -= BossHealthWaves;*/
            GameEvents.ToSpawnEnemy -= EnemySpawnRoutine;
        }

        private void EnemySpawnRoutine()
        {
            StartCoroutine(EnemySpawn());
        }

        private IEnumerator EnemySpawn()
        {
            yield return new WaitForSeconds(0.5f);
            if (true || Random.value > 0.5f)
            {
                SpawnFlyingEnemies(1);
                yield break;
            }
            if (enemyTargetPositions == null || enemyTargetPositions.Length == 0) yield break;
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


        private void SpawnWalkingEnemy(Transform targetTransform)
        {
            var walkingEnemy = WalkingEnemyPool.Instance.Get();
            walkingEnemy.transform.position = GetRandomPointInCollider();
            ApplyRandomForce(walkingEnemy);
            var target = targetTransform.parent
                ? targetTransform.parent.TransformPoint(targetTransform.localPosition)
                : targetTransform.position;
            walkingEnemy.ToTarget(target);
        }

        private void SpawnFlyingEnemies(int amount)
        {
            for (var i = 0; i < amount; i++)
            {
                var flyingEnemy = FlyingEnemyPool.Instance.Get();
                if (Random.Range(0, 100) < chanceOfBigEnemy) flyingEnemy.SetBigEnemy();
                flyingEnemy.transform.position = GetRandomPointInCollider();
                ApplyRandomForce(flyingEnemy);
            }
        }
        
        private Vector2 GetRandomPointInCollider()
        {
            Bounds bounds = gameAreaCollider.bounds;
            for (int i = 0; i < 10; i++) // safety loop
            {
                float x = Random.Range(bounds.min.x, bounds.max.x);
                float y = Random.Range(bounds.min.y, bounds.max.y);
                Vector2 point = new Vector2(x, y);

                if (gameAreaCollider.OverlapPoint(point))
                    return point;
            }
            return gameAreaCollider.bounds.center;  // fallback (should almost never happen)
        }

    }
}



/*private void BossHealthWaves(int currentHealth)
{
    switch (currentHealth)
    {
        case 15:
            SpawnFlyingEnemies(3);
            GameEvents.EnemySpawned?.Invoke();
            break;
        case 10:
            GameEvents.SpawnAllWalkingEnemies?.Invoke();
            SpawnFlyingEnemies(4);
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
    if (enemyTargetPositions == null || enemyTargetPositions.Length == 0) return;
    foreach (var targetTransform in enemyTargetPositions) SpawnWalkingEnemy(targetTransform);
}*/
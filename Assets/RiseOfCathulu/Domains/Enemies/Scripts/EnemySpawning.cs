using System.Collections;
using RiseOfCathulu.Domains.Player.Scripts;
using RiseOfCathulu.Domains.Utilities.GameHandlers.Scripts;
using UnityEngine;

namespace RiseOfCathulu.Domains.Enemies.Scripts
{
    public class EnemySpawning : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GrowthConfig growthConfig;
        private PlayerSize _playerSize;
        
        [Header("Enemy Spawning")]
        [SerializeField, Tooltip("Minimum impulse force applied to spawned enemies")] private float minSpawnForce = 4f;
        [SerializeField, Tooltip("Maximum impulse force applied to spawned enemies")] private float maxSpawnForce = 40f;
        [SerializeField, Tooltip("Offset from the spawn position")] private Vector3 spawnOffset = new(0.8f, 0f, 0f);
        [SerializeField, Tooltip("Maximum distance of enemy from planet")] private float maxDistanceFromPlanet = 10f;
        [SerializeField, Tooltip("Maximum distance of eatable enemy from planet")]
        private float eatableMaxDistanceFromPlanet = 10f;
        
        [Header("Spawn Limits")]
        [SerializeField] private int maxActiveEnemies = 10;
        [SerializeField] private int maxEatableEnemies = 3;
        private int _currentEatableEnemies = 0;
        private int _currentActiveEnemies = 0;

        [Header("Normal Distribution (Leveling)")]
        [SerializeField, Tooltip("1=Tight range, 3=High variety")] private float levelStandardDeviation = 1.5f;
        [SerializeField, Tooltip("Shift average enemy level (-1=slightly easier)")] private int levelOffset = 0;
        
                
        [Header("Debug Settings")]
        [SerializeField] private bool showDebugOverlay = true;
        [SerializeField] private Color debugCircleColor = new Color(1, 0, 0, 0.2f);
        private int _lastSpawnedLevel;
        private float _lastSpawnedScale;

        public bool CanBecomeEatable()
        {
            return _currentEatableEnemies < maxEatableEnemies;
        } 

        public void NotifyEnemyReturned(bool wasEatable)
        {
            _currentActiveEnemies = Mathf.Max(0, _currentActiveEnemies - 1);

            if (wasEatable)
                _currentEatableEnemies = Mathf.Max(0, _currentEatableEnemies - 1);
        }

        public void NotifyEnemyBecameEatable()
        {
            _currentEatableEnemies++;
        }

        public void NotifyEnemyStoppedBeingEatable()
        {
            _currentEatableEnemies = Mathf.Max(0, _currentEatableEnemies - 1);
        }


        private void Awake()
        {
            _playerSize = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerSize>();
        }
        
        private void OnEnable()
        {
            GameEvents.ToSpawnEnemy += EnemySpawnRoutine;
            GameEvents.PlanetEnemyDestroyed += StopSpawning;
            
        }

        private void OnDisable()
        {
            GameEvents.ToSpawnEnemy -= EnemySpawnRoutine;
            GameEvents.PlanetEnemyDestroyed -= StopSpawning;
        }
        
        private void StopSpawning(Transform destroyedPlanet)
        {
            if (transform.parent != destroyedPlanet) return;
            StopAllCoroutines();
        }
        
        private int GetForcedEatableLevel()
        {
            // Smallest level that is eatable by the player
            // (equal size is eatable in your logic)
            return Mathf.Clamp(
                _playerSize.CurrentSizeLevel,
                growthConfig.minLevel,
                growthConfig.maxLevel
            );
        }

        private void EnemySpawnRoutine() => StartCoroutine(EnemySpawn());

        private IEnumerator EnemySpawn()
        {
            yield return new WaitForSeconds(0.5f);
            SpawnFlyingEnemies(1);
        }

        private void SpawnFlyingEnemies(int amount)
        {
            for (var i = 0; i < amount; i++)
            {
                if (_currentActiveEnemies >= maxActiveEnemies)
                    break;
                
                var flyingEnemy = FlyingEnemyPool.Instance.Get();
                var enemy = flyingEnemy.GetComponent<FlyingEnemy>();
                int spawnedLevel = GetNormalDistributedLevel();;
                enemy.InitializeLevel(spawnedLevel, growthConfig);
                enemy.SetTether(transform.parent, maxDistanceFromPlanet, eatableMaxDistanceFromPlanet);
                flyingEnemy.transform.position = transform.position + spawnOffset;
                ApplyRandomForce(flyingEnemy);
                enemy.SetOwnerSpawner(this);
                _currentActiveEnemies++;
                _lastSpawnedLevel = spawnedLevel;
                _lastSpawnedScale = flyingEnemy.transform.localScale.x; 
            }
        }
        
        private int GetNormalDistributedLevel()
        {
            int meanLevel = _playerSize.CurrentSizeLevel + levelOffset;
            return LevelDistribution.GetNormalDistributedLevel(
                meanLevel,
                levelStandardDeviation,
                growthConfig.minLevel,
                growthConfig.maxLevel
            );
        }


        private void ApplyRandomForce(FlyingEnemy enemy)
        {
            var rb = enemy.GetComponent<Rigidbody2D>();
            if (rb) {
                var direction = Random.insideUnitCircle.normalized;
                var force = Random.Range(minSpawnForce, maxSpawnForce);
                rb.AddForce(direction * force, ForceMode2D.Impulse);
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Visualize the Tether Range
            Gizmos.color = debugCircleColor;
            if (transform.parent != null)
            {
                Gizmos.DrawWireSphere(transform.parent.position, maxDistanceFromPlanet);
                Gizmos.DrawWireSphere(transform.parent.position, eatableMaxDistanceFromPlanet);
            }

            // Visualize the Spawn Offset point
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + spawnOffset);
            Gizmos.DrawSphere(transform.position + spawnOffset, 0.2f);
        }
        private void OnGUI()
        {
            if (!showDebugOverlay) return;
            GUI.Box(new Rect(10, 10, 250, 110), "Spawner Debug Tool");
            GUI.Label(new Rect(20, 30, 230, 20), $"Player Level: {_playerSize.CurrentSizeLevel}");
            GUI.Label(new Rect(20, 50, 230, 20), $"Target Mean Level: {_playerSize.CurrentSizeLevel + levelOffset}");
            GUI.Label(new Rect(20, 70, 230, 20), $"Last Enemy Level: {_lastSpawnedLevel}");
            GUI.Label(new Rect(20, 90, 230, 20), $"Last Enemy Scale: {_lastSpawnedScale:F2}");
        }
    }
}

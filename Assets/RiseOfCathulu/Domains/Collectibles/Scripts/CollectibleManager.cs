using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RiseOfCathulu.Domains.Utilities.GameHandlers.Scripts;
using RiseOfCathulu.Domains.Weapons.Scripts;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RiseOfCathulu.Domains.Collectibles.Scripts
{
    public class CollectibleManager : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField, Tooltip("Global weapon settings (for default weapon etc.)")] private WeaponSettings settings;
        [SerializeField, Tooltip("Prefabs for power-up collectibles")] private GameObject[] powerUpCollectibles;
        [SerializeField, Tooltip("Prefabs for point collectibles")] private GameObject[] pointCollectibles;

        [Header("Spawning")]
        [SerializeField, Tooltip("Interval between automatic collectible drops")] private float dropInterval = 6f;
        [SerializeField, Tooltip("Chance to drop on enemy destruction (0-1)")] private float dropChance = 0.35f;
        [SerializeField, Tooltip("Chance (0-100) for power-up ")] private float powerUpToPointPercentRatio = 35f;
        
        [Header("Planets")]
        [SerializeField] private CircleCollider2D[] gravityAreas;
        
        private readonly List<Collectible> _activeCollectibles = new();
        private WeaponType _activeWeapon;
        private bool _isShieldActive;
        private int _currentPlayerHealth;
        private int _initialPlayerHealth;
        private Coroutine _spawnRoutine;

        private void Awake()
        {
            _currentPlayerHealth = _initialPlayerHealth;
            _activeWeapon =  settings.defaultWeapon;
        }

        private void Start()
        {
            DestroyAllCollectibles();
            StartSpawningCollectibles();
        }

        private void OnEnable()
        {
            GameEvents.ShieldUpdated += UpdateShield;
            GameEvents.WeaponCollected += UpdateWeapon;
            GameEvents.PlayerDefeated += StopCollectiblesMovement;
            GameEvents.PlayerLostLife += UpdatePlayerHealth;
            GameEvents.EnemyDestroyed += DropCollectible;
            GameEvents.RestartLevel += StartSpawningCollectibles;
        }

        private void OnDisable()
        {
            GameEvents.ShieldUpdated -= UpdateShield;
            GameEvents.WeaponCollected -= UpdateWeapon;
            GameEvents.PlayerDefeated -= StopCollectiblesMovement;
            GameEvents.PlayerLostLife -= UpdatePlayerHealth;
            GameEvents.EnemyDestroyed -= DropCollectible;
            GameEvents.RestartLevel -= StartSpawningCollectibles;

            if (_spawnRoutine != null)
            {
                StopCoroutine(_spawnRoutine);
                _spawnRoutine = null;
            }
        }

        private void UpdatePlayerHealth(int health) => _currentPlayerHealth = health;
        private void UpdateWeapon(WeaponType weaponType) => _activeWeapon = weaponType;
        private void UpdateShield(bool isActive) => _isShieldActive = isActive;

        private void StartSpawningCollectibles()
        {
            if (_spawnRoutine != null) StopCoroutine(_spawnRoutine);
            _spawnRoutine = StartCoroutine(SpawnCollectiblesRoutine());
        }

        private IEnumerator SpawnCollectiblesRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(dropInterval);
                DropCollectible(Vector3.zero);
            }
        }

        private void DropCollectible(Vector3 position)
        {
            if (Random.value > dropChance) return;
            var roll = Random.Range(0f, 100f);
            var isRandomPlanet = position == Vector3.zero;
            if (roll < powerUpToPointPercentRatio) 
                DropPowerUpCollectible(isRandomPlanet);
            else 
                DropPointCollectible(isRandomPlanet);
        }

        private void DropPowerUpCollectible(bool isRandomPlanet)
        {
            if (powerUpCollectibles == null || powerUpCollectibles.Length == 0) return;
            var selected = powerUpCollectibles[Random.Range(0, powerUpCollectibles.Length)];
            if (IsRedundantCollectible(selected)) return;
            var spawned = Instantiate(selected, Vector3.zero, Quaternion.identity);
            var collectible = spawned.GetComponent<Collectible>();
            if (collectible != null)
            {
                if (isRandomPlanet)
                {
                    var gravityArea = gravityAreas[Random.Range(0, gravityAreas.Length)];
                    collectible.InitializeFallTowardsPlanet(gravityArea.transform,
                        gravityArea.radius * gravityArea.transform.lossyScale.x
                    );
                }
                _activeCollectibles.Add(collectible);
            }
        }

        private void DropPointCollectible(bool isRandomPlanet)
        {
            if (pointCollectibles == null || pointCollectibles.Length == 0) return;
            var selected = pointCollectibles[Random.Range(0, pointCollectibles.Length)];
            var spawned = Instantiate(selected, Vector3.zero, Quaternion.identity);
            var collectible = spawned.GetComponent<Collectible>();
            if (collectible != null)
            {
                if (isRandomPlanet)
                {
                    var gravityArea = gravityAreas[Random.Range(0, gravityAreas.Length)];
                    collectible.InitializeFallTowardsPlanet(gravityArea.transform,
                        gravityArea.radius * gravityArea.transform.lossyScale.x
                    );
                }
                _activeCollectibles.Add(collectible);
            }
        }

        private bool IsRedundantCollectible(GameObject prefab)
        {
            if (prefab.TryGetComponent(out WeaponCollectible weapon))
                return weapon.GetWeaponType() == _activeWeapon;

            if (prefab.TryGetComponent(out ShieldCollectible _))
                return _isShieldActive;

            if (prefab.TryGetComponent(out SizeCollectible _))
                return _currentPlayerHealth >= _initialPlayerHealth;

            return false;
        }

        private void StopCollectiblesMovement()
        {
            if (_spawnRoutine != null)
            {
                StopCoroutine(_spawnRoutine);
                _spawnRoutine = null;
            }
            foreach (var c in _activeCollectibles.Where(c => c != null))
            {
                c.StopMovement();
                var animator = c.GetComponent<Animator>();
                if (animator != null)
                {
                    animator.speed = 0f;
                }
            }
        }

        private void DestroyAllCollectibles()
        {
            foreach (var c in _activeCollectibles.Where(c => c != null))
            {
                Destroy(c.gameObject);
            }
            _activeCollectibles.Clear();
        }
    }
}

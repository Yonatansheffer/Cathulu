using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _WHY.Domains.Player.Scripts;
using _WHY.Domains.Utilities.GameHandlers.Scripts;
using _WHY.Domains.Weapons.Scripts;
using UnityEngine;
using Weapons;
using Random = UnityEngine.Random;

namespace _WHY.Domains.Collectibles.Scripts
{
    public class CollectibleManager : MonoBehaviour
    {
        [SerializeField] private WeaponSettings settings;
        [SerializeField, Tooltip("Prefabs for power-up collectibles")] private GameObject[] powerUpCollectibles;
        [SerializeField, Tooltip("Prefabs for point collectibles")] private GameObject[] pointCollectibles;
        [SerializeField, Tooltip("Interval between automatic collectible drops")] private float dropInterval;
        [SerializeField, Tooltip("Possible spawn positions for collectibles")] private Transform[] positionsForDrop;
        [SerializeField, Tooltip("Chance to drop collectible on enemy destruction (0-1)")] private float dropChance ;
        [SerializeField, Tooltip("Chance for power-up vs. point collectible (0-100)")]
        private float powerUpToPointPercentRatio;
        [SerializeField, Tooltip("Random X offset range for spawn position")]
        private Vector2 randomXOffsetRange;
        [SerializeField, Tooltip("Y offset for collectible spawn position")] private float yOffset;
        
        private List<Collectible> _activeCollectibles;
        private WeaponType _activeWeapon;
        private bool _isShieldActive;
        private int _currentPlayerHealth;

        private void Awake()
        {
            _currentPlayerHealth = PlayerHealth.GetInitialPlayerHealth();
            _activeCollectibles = new List<Collectible>();
            _activeWeapon = settings.defaultWeapon;
        }

        private void Start()
        {
            StartSpawningCollectibles();
        }

        private void OnEnable()
        {
            GameEvents.ShieldUpdated += UpdateShield;
            GameEvents.WeaponCollected += UpdateWeapon;
            GameEvents.BeginGameLoop += DestroyAllCollectibles;
            GameEvents.PlayerDefeated += StopCollectiblesMovement;
            GameEvents.PlayerLivesChanged += UpdatePlayerHealth;
            GameEvents.EnemyDestroyed += DropCollectible;
            GameEvents.RestartLevel += StartSpawningCollectibles;
        }

        private void OnDisable()
        {
            GameEvents.ShieldUpdated -= UpdateShield;
            GameEvents.WeaponCollected -= UpdateWeapon;
            GameEvents.BeginGameLoop -= DestroyAllCollectibles;
            GameEvents.PlayerDefeated -= StopCollectiblesMovement;
            GameEvents.PlayerLivesChanged -= UpdatePlayerHealth;
            GameEvents.EnemyDestroyed -= DropCollectible;
            GameEvents.RestartLevel -= StartSpawningCollectibles;
        }

        private void UpdatePlayerHealth(int health)
        {
            _currentPlayerHealth = health;
        }

        private void UpdateWeapon(WeaponType weaponType)
        {
            _activeWeapon = weaponType;
        }

        private void UpdateShield(bool isActive)
        {
            _isShieldActive = isActive;
        }

        private void DropCollectible(Vector3 position)
        {
            if (Random.value > dropChance)
            {
                return;
            }

            Vector3 spawnPosition = position == Vector3.zero
                ? GetRandomSpawnPosition()
                : position;

            if (Random.Range(0, 100) > powerUpToPointPercentRatio)
            {
                DropPointCollectible(spawnPosition);
            }
            else
            {
                DropPowerUpCollectible(spawnPosition);
            }
        }

        private Vector3 GetRandomSpawnPosition()
        {
            var randomPosition = positionsForDrop[Random.Range(0, positionsForDrop.Length)].position;
            return randomPosition + new Vector3(Random.Range(randomXOffsetRange.x, randomXOffsetRange.y), yOffset, 0f);
        }

        private void DropPowerUpCollectible(Vector3 position)
        {
            var selected = powerUpCollectibles[Random.Range(0, powerUpCollectibles.Length)];
            if (IsRedundantCollectible(selected))
            {
                return;
            }

            var spawned = Instantiate(selected, position, Quaternion.identity);
            var collectible = spawned.GetComponent<Collectible>();
            if (collectible != null)
            {
                _activeCollectibles.Add(collectible);
            }
        }

        private bool IsRedundantCollectible(GameObject collectible)
        {
            if (collectible.TryGetComponent(out WeaponCollectible weapon))
            {
                return weapon.GetWeaponType() == _activeWeapon;
            }
            if (collectible.TryGetComponent(out ShieldCollectible _))
            {
                return _isShieldActive;
            }
            if (collectible.TryGetComponent(out LifeCollectible _))
            {
                return _currentPlayerHealth >= PlayerHealth.GetInitialPlayerHealth();
            }
            return false;
        }

        private void StartSpawningCollectibles()
        {
            StartCoroutine(SpawnCollectiblesRoutine());
        }

        private IEnumerator SpawnCollectiblesRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(dropInterval);
                DropCollectible(Vector3.zero);
            }
        }

        private void DropPointCollectible(Vector3 position)
        {
            var selectedPoint = pointCollectibles[Random.Range(0, pointCollectibles.Length)];
            var pointCollectibleObject = Instantiate(selectedPoint, position, Quaternion.identity);
            var collectible = pointCollectibleObject.GetComponent<Collectible>();
            if (collectible != null)
            {
                _activeCollectibles.Add(collectible);
            }
        }

        private void StopCollectiblesMovement()
        {
            StopAllCoroutines();
            foreach (var collectible in _activeCollectibles)
            {
                if (collectible != null)
                {
                    collectible.StopMovement();
                }
            }
        }

        private void DestroyAllCollectibles()
        {
            foreach (var collectible in _activeCollectibles.Where(collectible => collectible != null))
            {
                Destroy(collectible.gameObject);
            }

            _activeCollectibles.Clear();
        }
    }
}
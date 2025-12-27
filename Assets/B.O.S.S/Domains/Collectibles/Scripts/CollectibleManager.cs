using System.Collections;
using System.Collections.Generic;
using System.Linq;
using B.O.S.S.Domains.Player.Scripts;
using B.O.S.S.Domains.Utilities.GameHandlers.Scripts;
using B.O.S.S.Domains.Weapons.Scripts;
using UnityEngine;
using Random = UnityEngine.Random;

namespace B.O.S.S.Domains.Collectibles.Scripts
{
    public class CollectibleManager : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField, Tooltip("Global weapon settings (for default weapon etc.)")]
        private WeaponSettings settings;
        [SerializeField, Tooltip("Prefabs for power-up collectibles")]
        private GameObject[] powerUpCollectibles;
        [SerializeField, Tooltip("Prefabs for point collectibles")]
        private GameObject[] pointCollectibles;

        [Header("Spawning")]
        [SerializeField, Tooltip("Interval between automatic collectible drops")]
        private float dropInterval = 6f;
        [SerializeField, Tooltip("Possible spawn positions for collectibles")]
        private Transform[] positionsForDrop;
        [SerializeField, Tooltip("Chance to drop collectible on enemy destruction (0-1)")]
        private float dropChance = 0.35f;
        [SerializeField, Tooltip("Chance (0-100) that a drop is a power-up (else points)")]
        private float powerUpToPointPercentRatio = 35f;
        [SerializeField, Tooltip("Random X offset range for spawn position")]
        private Vector2 randomXOffsetRange = new(-0.75f, 0.75f);
        [SerializeField, Tooltip("Y offset for collectible spawn position")]
        private float yOffset = 0.75f;

        private readonly List<Collectible> _activeCollectibles = new();
        private WeaponType _activeWeapon;
        private bool _isShieldActive;
        private int _currentPlayerHealth;
        private int _initialPlayerHealth;
        private Coroutine _spawnRoutine;

        private void Awake()
        {
            //_initialPlayerHealth = PlayerSize.CurrentSizeLevel();
            _currentPlayerHealth = _initialPlayerHealth;
            _activeWeapon =  settings.defaultWeapon;
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
            GameEvents.PlayerLostLife += UpdatePlayerHealth;
            GameEvents.EnemyDestroyed += DropCollectible;
            GameEvents.RestartLevel += StartSpawningCollectibles;
        }

        private void OnDisable()
        {
            GameEvents.ShieldUpdated -= UpdateShield;
            GameEvents.WeaponCollected -= UpdateWeapon;
            GameEvents.BeginGameLoop -= DestroyAllCollectibles;
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

            var spawnPos = position == Vector3.zero ? GetRandomSpawnPosition() : position;
            var roll = Random.Range(0f, 100f);
            if (roll < powerUpToPointPercentRatio) DropPowerUpCollectible(spawnPos);
            else DropPointCollectible(spawnPos);
        }

        private Vector3 GetRandomSpawnPosition()
        {
            if (positionsForDrop == null || positionsForDrop.Length == 0) return Vector3.zero;
            var anchor = positionsForDrop[Random.Range(0, positionsForDrop.Length)].position;
            var dx = Random.Range(randomXOffsetRange.x, randomXOffsetRange.y);
            return new Vector3(anchor.x + dx, anchor.y + yOffset, 0f);
        }

        private void DropPowerUpCollectible(Vector3 position)
        {
            if (powerUpCollectibles == null || powerUpCollectibles.Length == 0) return;
            var selected = powerUpCollectibles[Random.Range(0, powerUpCollectibles.Length)];
            if (IsRedundantCollectible(selected)) return;
            var spawned = Instantiate(selected, position, Quaternion.identity);
            var collectible = spawned.GetComponent<Collectible>();
            if (collectible != null) _activeCollectibles.Add(collectible);
        }

        private void DropPointCollectible(Vector3 position)
        {
            if (pointCollectibles == null || pointCollectibles.Length == 0) return;
            var selected = pointCollectibles[Random.Range(0, pointCollectibles.Length)];
            var spawned = Instantiate(selected, position, Quaternion.identity);
            var collectible = spawned.GetComponent<Collectible>();
            if (collectible != null) _activeCollectibles.Add(collectible);
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

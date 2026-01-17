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
        
        private readonly Dictionary<Transform, Coroutine> _planetSpawnRoutines = new();
        private readonly List<Collectible> _activeCollectibles = new();
        private WeaponType _activeWeapon;
        private bool _isShieldActive;

        private void Awake()
        {
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
            GameEvents.RestartLevel += StartSpawningCollectibles;
            GameEvents.PlanetDestroyed += StopSpawningForPlanet;
        }

        private void OnDisable()
        {
            GameEvents.ShieldUpdated -= UpdateShield;
            GameEvents.WeaponCollected -= UpdateWeapon;
            GameEvents.PlayerDefeated -= StopCollectiblesMovement;
            GameEvents.RestartLevel -= StartSpawningCollectibles;
            GameEvents.PlanetDestroyed -= StopSpawningForPlanet;
        }

        private void UpdateWeapon(WeaponType weaponType) => _activeWeapon = weaponType;
        private void UpdateShield(bool isActive) => _isShieldActive = isActive;

        private void StartSpawningCollectibles()
        {
            StopAllPlanetRoutines();
            foreach (var gravityArea in gravityAreas)
            {
                if (gravityArea == null) continue;
                var planet = gravityArea.transform;
                var routine = StartCoroutine(SpawnCollectiblesForPlanet(gravityArea));
                _planetSpawnRoutines[planet] = routine;
            }
        }
        
        private IEnumerator SpawnCollectiblesForPlanet(CircleCollider2D gravityArea)
        {
            var planetTransform = gravityArea.transform;
            float radius = gravityArea.radius * gravityArea.transform.lossyScale.x;
            while (true)
            {
                yield return new WaitForSeconds(dropInterval);

                if (gravityArea == null || planetTransform == null)
                    yield break;

                DropCollectibleAtPlanet(planetTransform, radius);
            }
        }
        
        private void StopSpawningForPlanet(Transform destroyedPlanet)
        {
            if (!_planetSpawnRoutines.TryGetValue(destroyedPlanet, out var routine))
                return;

            StopCoroutine(routine);
            _planetSpawnRoutines.Remove(destroyedPlanet);
        }

        
        private void DropCollectibleAtPlanet(Transform planet, float radius)
        {
            if (Random.value > dropChance) return;

            float roll = Random.Range(0f, 100f);

            if (roll < powerUpToPointPercentRatio)
                DropPowerUpCollectible(planet, radius);
            else
                DropPointCollectible(planet, radius);
        }
        
        private void DropPowerUpCollectible(Transform planet, float radius)
        {
            if (powerUpCollectibles == null || powerUpCollectibles.Length == 0) return;

            var selected = powerUpCollectibles[Random.Range(0, powerUpCollectibles.Length)];
            if (IsRedundantCollectible(selected)) return;

            var spawned = Instantiate(selected, Vector3.zero, Quaternion.identity);
            var collectible = spawned.GetComponent<Collectible>();

            if (collectible != null)
            {
                collectible.InitializeFallTowardsPlanet(planet, radius);
                _activeCollectibles.Add(collectible);
            }
        }


        private void DropPointCollectible(Transform planet, float radius)
        {
            if (pointCollectibles == null || pointCollectibles.Length == 0) return;

            var selected = pointCollectibles[Random.Range(0, pointCollectibles.Length)];
            var spawned = Instantiate(selected, Vector3.zero, Quaternion.identity);
            var collectible = spawned.GetComponent<Collectible>();

            if (collectible != null)
            {
                collectible.InitializeFallTowardsPlanet(planet, radius);
                _activeCollectibles.Add(collectible);
            }
        }


        private bool IsRedundantCollectible(GameObject prefab)
        {
            if (prefab.TryGetComponent(out WeaponCollectible weapon))
                return weapon.GetWeaponType() == _activeWeapon;

            if (prefab.TryGetComponent(out ShieldCollectible _))
                return _isShieldActive;

            return false;
        }

        private void StopCollectiblesMovement()
        {
            StopAllPlanetRoutines(); 
            foreach (var c in _activeCollectibles.Where(c => c != null))
            {
                c.StopMovement();
                var animator = c.GetComponent<Animator>();
                if (animator != null)
                    animator.speed = 0f;
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
        
        
        private void StopAllPlanetRoutines()
        {
            foreach (var routine in _planetSpawnRoutines.Values)
            {
                if (routine != null)
                    StopCoroutine(routine);
            }
            _planetSpawnRoutines.Clear();
        }

    }
}

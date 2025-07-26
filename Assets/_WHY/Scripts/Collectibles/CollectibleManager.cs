using System;
using System.Collections;
using System.Collections.Generic;
using GameHandlers;
using UnityEngine;
using UnityEngine.Serialization;
using Weapons;
using Random = UnityEngine.Random;

namespace Collectibles
{
    public class CollectibleManager : MonoBehaviour
    {
        [SerializeField] private WeaponSettings settings;
        [SerializeField] private GameObject[] powerUpCollectibles;
        [SerializeField] private GameObject[] pointCollectibles;
        [SerializeField] private float powerUptoPointPercentRatio = 17f; // Interval between possible power up drops
        [SerializeField] private float dropInterval = 5f; 
        private List<Collectible> _activeCollectibles; // List of all active collectibles
        [SerializeField] private Transform[] positionsForDrop;
        [SerializeField] private float yOffset = -0.5f; // Offset for the collectible spawn position
        private WeaponType _activeWeapon;
        private bool _isShieldActive;
        private void Awake()
        {
            _activeCollectibles = new List<Collectible>();
        }

        private void Start()
        {
            StartDropCoroutine();
        }

        private void OnEnable()
        {
            GameEvents.ShieldUpdated += UpdateShield;
            GameEvents.WeaponCollected += UpdateWeapon;
            GameEvents.BeginGameLoop += DestroyAllCollectibles;
            GameEvents.FreezeLevel += StopCollectiblesMovement;
            GameEvents.EnemyDestroyed += DropCollectible;
            GameEvents.RestartLevel += StartDropCoroutine;
            GameEvents.ReadyStage += DestroyAllCollectibles;
        }

        private void OnDisable()
        {
            GameEvents.ShieldUpdated -= UpdateShield;
            GameEvents.WeaponCollected -= UpdateWeapon;
            GameEvents.BeginGameLoop -= DestroyAllCollectibles;
            GameEvents.FreezeLevel -= StopCollectiblesMovement;
            GameEvents.EnemyDestroyed -= DropCollectible;
            GameEvents.RestartLevel -= StartDropCoroutine;
            GameEvents.ReadyStage -= DestroyAllCollectibles;
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
            if (Random.value > 0.5f)
            {
                return;
            }
            if (position == Vector3.zero)
            {
                position = positionsForDrop[Random.Range(0, positionsForDrop.Length)].position
                           + new Vector3(0f, yOffset, 0f);
            }
            if (Random.Range(0, 100) > powerUptoPointPercentRatio)
            {
                DropPointCollectible(position);
            }
            else
            {
                DropPowerUpCollectible(position);
            }
        }

        private void DropPowerUpCollectible(Vector3 position)
        {
            var selectedPowerUp = powerUpCollectibles[Random.Range(0, powerUpCollectibles.Length)];
            var weaponCollectible = selectedPowerUp.GetComponent<WeaponCollectible>();
            if (weaponCollectible != null)
            {
                if(weaponCollectible.GetWeaponType() == _activeWeapon)
                {
                    return;
                }
            }
            var shieldCollectible = selectedPowerUp.GetComponent<ShieldCollectible>();
            if (shieldCollectible != null && _isShieldActive)
            {
                return; 
            }
            Vector3 spawnPosition = position;
            var powerUpCollectibleObject = Instantiate(selectedPowerUp, spawnPosition, Quaternion.identity);
            _activeCollectibles.Add(powerUpCollectibleObject.GetComponent<Collectible>());
        }
        
        private void StartDropCoroutine()
        {
            StartCoroutine(DropCoroutine());
        }
    
        private IEnumerator DropCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(dropInterval); // Wait for the specified interval
                DropCollectible(Vector3.zero);
            }
        }
    
        private void DropPointCollectible(Vector3 position)
        {
            var selectedPoint = pointCollectibles[Random.Range(0, pointCollectibles.Length)]; 
            var pointCollectibleObject = Instantiate(selectedPoint, position, Quaternion.identity);
            _activeCollectibles.Add(pointCollectibleObject.GetComponent<Collectible>());
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
            if(_activeCollectibles.Count == 0)
                return;
            foreach (var collectible in _activeCollectibles)
            {
                if (collectible != null)
                {
                    Destroy(collectible.gameObject);
                }
            }
            _activeCollectibles.Clear();
        }
    }
}
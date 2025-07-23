using System;
using System.Collections;
using System.Collections.Generic;
using GameHandlers;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Collectibles
{
    public class CollectibleManager : MonoBehaviour
    {
        [SerializeField] private GameObject[] powerUpCollectibles;
        [SerializeField] private GameObject[] foodCollectibles;
        [SerializeField] private float powerUptoFoodPercentRatio = 17f; // Interval between possible power up drops
        [SerializeField] private float dropInterval = 5f; // Interval between possible food collectible drops*/
        private List<Collectible> _activeCollectibles; // List of all active collectibles
        [SerializeField] private Transform[] positionsForDrop;
        [SerializeField] private float yOffset = -0.5f; // Offset for the collectible spawn position
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
            GameEvents.BeginGameLoop += DestroyAllCollectibles;
            GameEvents.FreezeLevel += StopCollectiblesMovement;
            GameEvents.EnemyDestroyed += DropCollectible;
            GameEvents.RestartLevel += StartDropCoroutine;
            GameEvents.ReadyStage += DestroyAllCollectibles;
        }

        private void OnDisable()
        {
            GameEvents.BeginGameLoop -= DestroyAllCollectibles;
            GameEvents.FreezeLevel -= StopCollectiblesMovement;
            GameEvents.EnemyDestroyed -= DropCollectible;
            GameEvents.RestartLevel -= StartDropCoroutine;
            GameEvents.ReadyStage -= DestroyAllCollectibles;
        }
        
        private void DropCollectible(Vector3 position)
        {
            if (Random.value > 0.5f)
            {
                return;
            }
            if (position == Vector3.zero)
            {
                position = positionsForDrop[Random.Range(0, positionsForDrop.Length)].position;
            }
            if (Random.Range(0, 100) > powerUptoFoodPercentRatio)
            {
                DropFoodCollectible(position);
            }
            else
            {
                DropPowerUpCollectible(position);
            }
        }

        private void DropPowerUpCollectible(Vector3 position)
        {
            var selectedPowerUp = powerUpCollectibles[Random.Range(0, powerUpCollectibles.Length)];
            Vector3 spawnPosition = position + new Vector3(0f, yOffset, 0f);
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
    
        private void DropFoodCollectible(Vector3 position)
        {
            var selectedFood = foodCollectibles[Random.Range(0, foodCollectibles.Length)]; 
            var foodCollectibleObject = Instantiate(selectedFood, position, Quaternion.identity);
            _activeCollectibles.Add(foodCollectibleObject.GetComponent<Collectible>());
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
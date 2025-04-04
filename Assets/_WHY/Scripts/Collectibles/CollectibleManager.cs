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
        //[SerializeField] private GameObject[] foodCollectibles;
        [SerializeField] private float powerUpDropChance = 30f; // Chance of dropping a power up collectible
        /*[SerializeField] private float foodDropChance = 10f; // Chance of dropping a food collectible
        [SerializeField] private float foodDropInterval = 5f; // Interval between possible food collectible drops*/
        private List<Collectible> _activeCollectibles; // List of all active collectibles

        private void Awake()
        {
            _activeCollectibles = new List<Collectible>();
        }

        private void OnEnable()
        {
            GameEvents.BeginGamePlay += DestroyAllCollectibles;
            GameEvents.FreezeStage += StopCollectiblesMovement;
            //GameEvents.StartStage += StartDropFoodCoroutine;
            GameEvents.StartGame += DestroyAllCollectibles;
            GameEvents.ReadyStage += DestroyAllCollectibles;
            GameEvents.GameOver += DestroyAllCollectibles;
        }

        private void OnDisable()
        {
            GameEvents.BeginGamePlay -= DestroyAllCollectibles;
            GameEvents.FreezeStage -= StopCollectiblesMovement;
            //GameEvents.StartStage -= StartDropFoodCoroutine;
            GameEvents.ReadyStage -= DestroyAllCollectibles;
            GameEvents.GameOver -= DestroyAllCollectibles;
            GameEvents.StartGame -= DestroyAllCollectibles;
        }

        private void DropPowerUpCollectible(Transform t, int numHits)
        {
            if (numHits != 4 && Random.Range(0, 100) < powerUpDropChance)
            {
                var selectedPowerUp = powerUpCollectibles[Random.Range(0, powerUpCollectibles.Length)];
                var powerUpCollectibleObject = Instantiate(selectedPowerUp, t.position, Quaternion.identity);
                _activeCollectibles.Add(powerUpCollectibleObject.GetComponent<Collectible>());
            }
        }
        
        /*private void StartDropFoodCoroutine()
        {
            StartCoroutine(FoodDropCoroutine());
        }
    
        private IEnumerator FoodDropCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(foodDropInterval); // Wait for the specified interval
                DropFoodCollectible();
            }
        }
    
        private void DropFoodCollectible()
        {
            if (Random.Range(0, 100) < foodDropChance)
            {
                var selectedFood = foodCollectibles[Random.Range(0, foodCollectibles.Length)]; 
                var randomX = Random.Range(ceilingXStart, ceilingXEnd);
                var foodCollectibleObject =
                    Instantiate(selectedFood, new Vector3(randomX, ceilingHeight, 0), Quaternion.identity);
                _activeCollectibles.Add(foodCollectibleObject.GetComponent<Collectible>());
            }
        }*/
    
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
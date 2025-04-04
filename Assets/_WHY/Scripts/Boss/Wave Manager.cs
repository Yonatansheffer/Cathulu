using System.Collections;
using UnityEngine;
using GameHandlers;

namespace _WHY.Scripts.Boss
{
    public class WaveManager : MonoBehaviour
    {
        [SerializeField] private float startDelay = 2f;
        [SerializeField] private float initialInterval = 5f;
        [SerializeField] private float minInterval = 1f;
        [SerializeField] private float intervalDecreaseRate = 0.1f;
        private float currentInterval;

        private void Start()
        {
            currentInterval = initialInterval;
            StartCoroutine(HandleWaves());
        }

        private IEnumerator HandleWaves()
        {
            yield return new WaitForSeconds(startDelay);

            while (true)
            {
                // Trigger enemy spawn
                bool spawnFlying = Random.value > 0.5f;
                GameEvents.ToSpawnEnemy?.Invoke(spawnFlying);

                // Trigger boss shooting
                GameEvents.BossShoots?.Invoke();

                // Wait before next wave
                yield return new WaitForSeconds(currentInterval);

                // Speed up
                currentInterval = Mathf.Max(minInterval, currentInterval - intervalDecreaseRate);
            }
        }
    }
}
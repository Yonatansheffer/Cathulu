using System;
using System.Collections;
using UnityEngine;
using GameHandlers;
using Random = UnityEngine.Random;

namespace _WHY.Scripts.Boss
{
    public class WaveManager : MonoBehaviour
    {
        [Serializable]
        public class WaveConfig
        {
            public float startDelay = 5f;
            public float initialInterval = 5f;
            public float minInterval = 1f;
            public float intervalDecreaseRate = 0.02f;
            [HideInInspector] public float currentInterval;
        }

        [SerializeField] private WaveConfig enemyWaveConfig = new WaveConfig();
        [SerializeField] private WaveConfig bossShootConfig = new WaveConfig();

        private void Start()
        {
            enemyWaveConfig.currentInterval = enemyWaveConfig.initialInterval;
            bossShootConfig.currentInterval = bossShootConfig.initialInterval;

            StartCoroutine(EnemySpawnWaves());
            StartCoroutine(BossShootingWaves());
        }

        private IEnumerator BossShootingWaves()
        {
            yield return new WaitForSeconds(bossShootConfig.startDelay);

            while (true)
            {
                GameEvents.BossShoots?.Invoke();

                yield return new WaitForSeconds(bossShootConfig.currentInterval);

                bossShootConfig.currentInterval = Mathf.Max(
                    bossShootConfig.minInterval,
                    bossShootConfig.currentInterval - bossShootConfig.intervalDecreaseRate
                );
            }
        }

        private IEnumerator EnemySpawnWaves()
        {
            yield return new WaitForSeconds(enemyWaveConfig.startDelay);

            while (true)
            {
                bool spawnFlying = Random.value > 0.5f;
                GameEvents.ToSpawnEnemy?.Invoke(spawnFlying);
                yield return new WaitForSeconds(enemyWaveConfig.currentInterval);
                enemyWaveConfig.currentInterval = Mathf.Max(
                    enemyWaveConfig.minInterval,
                    enemyWaveConfig.currentInterval - enemyWaveConfig.intervalDecreaseRate
                );
            }
        }
    }
}

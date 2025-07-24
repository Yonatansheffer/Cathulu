using System;
using System.Collections;
using UnityEngine;
using GameHandlers;

namespace _WHY.Scripts.Boss
{
    public class WaveManager : MonoBehaviour
    {
        [Serializable]
        public class WaveConfig
        {
            public float startDelay = 5f;
            public float initialSpawnDuration = 20f;
            public float initialSpawnInterval = 5f;
            public float minSpawnDuration = 5f;
            public float minSpawnInterval = 1f;
            public float durationDecreaseRate = 1f;
            public float intervalDecreaseRate = 0.2f;

            [HideInInspector] public float currentSpawnDuration;
            [HideInInspector] public float currentSpawnInterval;
        }

        [SerializeField] private WaveConfig waveConfig = new WaveConfig();

        private bool bossIsShooting = false;

        private void Start()
        {
            waveConfig.currentSpawnDuration = waveConfig.initialSpawnDuration;
            waveConfig.currentSpawnInterval = waveConfig.initialSpawnInterval;

            StartCoroutine(CombinedWaveRoutine());
        }

        private IEnumerator CombinedWaveRoutine()
        {
            yield return new WaitForSeconds(waveConfig.startDelay);
            while (true)
            {
                print("entered");
                float elapsed = 0f;
                while (elapsed < waveConfig.currentSpawnDuration)
                {
                    GameEvents.ToSpawnEnemy?.Invoke();
                    yield return new WaitForSeconds(waveConfig.currentSpawnInterval);
                    elapsed += waveConfig.currentSpawnInterval;
                }

                bossIsShooting = true;
                GameEvents.BossShoots?.Invoke();
                yield return new WaitUntil(() => !bossIsShooting);
                waveConfig.currentSpawnDuration = Mathf.Max(
                    waveConfig.minSpawnDuration,
                    waveConfig.currentSpawnDuration - waveConfig.durationDecreaseRate
                );

                waveConfig.currentSpawnInterval = Mathf.Max(
                    waveConfig.minSpawnInterval,
                    waveConfig.currentSpawnInterval - waveConfig.intervalDecreaseRate
                );
            }
        }

        public void BossFinishedShooting()
        {
            bossIsShooting = false;
        }
    }
}

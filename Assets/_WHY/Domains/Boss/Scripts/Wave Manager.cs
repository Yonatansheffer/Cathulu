using System;
using System.Collections;
using _WHY.Domains.Utilities.GameHandlers.Scripts;
using UnityEngine;

namespace _WHY.Domains.Boss.Scripts
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
        private bool _isFrozen;
        private Coroutine _waveRoutine;

        private void Start()
        {
            waveConfig.currentSpawnDuration = waveConfig.initialSpawnDuration;
            waveConfig.currentSpawnInterval = waveConfig.initialSpawnInterval;
            _waveRoutine = StartCoroutine(CombinedWaveRoutine());
        }
        
        private void OnEnable()
        {
            GameEvents.FreezeLevel += OnFreeze;
            GameEvents.UnFreezeLevel += OnUnFreeze;
        }

        private void OnDisable()
        {
            GameEvents.FreezeLevel -= OnFreeze;
            GameEvents.UnFreezeLevel -= OnUnFreeze;
            if (_waveRoutine != null)
            {
                StopCoroutine(_waveRoutine);
                _waveRoutine = null;
            }
        }

        private IEnumerator CombinedWaveRoutine()
        {
            yield return new WaitForSeconds(waveConfig.startDelay);

            while (true)
            {
                float elapsed = 0f;
                while (elapsed < waveConfig.currentSpawnDuration)
                {
                    yield return new WaitUntil(() => !_isFrozen);
                    GameEvents.ToSpawnEnemy?.Invoke();
                    float timer = 0f;
                    while (timer < waveConfig.currentSpawnInterval)
                    {
                        yield return null;
                        if (!_isFrozen) timer += Time.deltaTime;
                    }

                    elapsed += waveConfig.currentSpawnInterval;
                }

                yield return new WaitUntil(() => !_isFrozen);

                GameEvents.BossShoots?.Invoke();

                yield return new WaitUntil(() => BossShooting.GetBossState() != BossShooting.BossState.Idle);

                yield return new WaitUntil(() => BossShooting.GetBossState() == BossShooting.BossState.Idle);

                yield return new WaitUntil(() => !_isFrozen);


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

        private void OnFreeze()
        {
            _isFrozen = true;
        }

        private void OnUnFreeze()
        {
            _isFrozen = false;
        }
    }
}

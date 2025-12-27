using System;
using System.Collections;
using B.O.S.S.Domains.Utilities.GameHandlers.Scripts;
using UnityEngine;

namespace B.O.S.S.Domains.Enemies.Scripts.Planet_Enemy
{
    public class WaveManager : MonoBehaviour
    {
        [Serializable]
        public class WaveConfig
        {
            [Tooltip("Delay before the first wave starts")]
            public float startDelay = 0f;
            [Tooltip("Initial time window for spawning enemies per wave")]
            public float initialSpawnDuration = 20f;
            [Tooltip("Initial interval between spawns inside a wave")]
            public float initialSpawnInterval = 5f;
            [Tooltip("Lower bound for spawn duration per wave")]
            public float minSpawnDuration = 5f;
            [Tooltip("Lower bound for interval between spawns")]
            public float minSpawnInterval = 1f;
            [Tooltip("How much to reduce duration after each cycle")]
            public float durationDecreaseRate = 1f;
            [Tooltip("How much to reduce interval after each cycle")]
            public float intervalDecreaseRate = 0.2f;
            [HideInInspector] public float currentSpawnDuration;
            [HideInInspector] public float currentSpawnInterval;
        }

        [Header("Wave Settings")]
        [SerializeField, Tooltip("Configuration for wave timings")]
        private WaveConfig waveConfig = new WaveConfig();

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
                var elapsed = 0f;
                while (elapsed < waveConfig.currentSpawnDuration)
                {
                    while (_isFrozen) yield return null;
                    GameEvents.ToSpawnEnemy?.Invoke();
                    yield return WaitSecondsUnfrozen(waveConfig.currentSpawnInterval);
                    elapsed += waveConfig.currentSpawnInterval;
                }

                while (_isFrozen) yield return null;
                
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

        private IEnumerator WaitSecondsUnfrozen(float seconds)
        {
            var t = 0f;
            while (t < seconds)
            {
                if (!_isFrozen) t += Time.deltaTime;
                yield return null;
            }
        }

        private void OnFreeze() => _isFrozen = true;
        private void OnUnFreeze() => _isFrozen = false;
    }
}

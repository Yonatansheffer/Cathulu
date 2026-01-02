using System;
using System.Collections;
using RiseOfCathulu.Domains.Utilities.GameHandlers.Scripts;
using UnityEngine;

namespace RiseOfCathulu.Domains.Enemies.Scripts.Planet_Enemy
{
    public class WaveManager : MonoBehaviour
    {
        [Serializable]
        public class WaveConfig
        {
            [Tooltip("Delay before the first wave starts")] public float startDelay = 0f;
            [Header("Duration (Wave Length)")]
            public float initialSpawnDuration = 20f;
            public float minSpawnDuration = 5f;
            public float durationDecreaseRate = 1f;

            [Header("Interval (Spawn Speed)")]
            [Tooltip("Time between spawns when player is at the EDGE of range")] public float maxSpawnInterval = 5f; 
            [Tooltip("Time between spawns when player is at the CENTER")] public float minSpawnInterval = 0.5f;

            [HideInInspector] public float currentSpawnDuration;
        }

        [Header("Wave Settings")]
        [SerializeField, Tooltip("Configuration for wave timings")] private WaveConfig waveConfig = new WaveConfig();
        
        [Header("Distance Settings")]
        [SerializeField] private float spawnActivationRange = 15f;
        [SerializeField, Tooltip("Inner radius where is maximum")] private float innerDangerRange = 2f;
        private Transform _playerTransform;
        private bool _isFrozen;
        private Coroutine _waveRoutine;


        private void Awake()
        {
            _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        }

        private void Start()
        {
            if (_playerTransform == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) _playerTransform = player.transform;
            }
            waveConfig.currentSpawnDuration = waveConfig.initialSpawnDuration;
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
                    while (_isFrozen || !IsPlayerInRange()) yield return null;
                    GameEvents.ToSpawnEnemy?.Invoke();
                    float dynamicInterval = GetDynamicInterval();
                    yield return WaitSecondsUnfrozen(dynamicInterval);
                    elapsed += dynamicInterval;
                }
                while (_isFrozen) yield return null;
                UpdateWaveDifficulty();
            }
        }
        
        private float GetDynamicInterval()
        {
            if (_playerTransform == null) return waveConfig.maxSpawnInterval;

            float distance = Vector3.Distance(transform.position, _playerTransform.position);
    
            // InverseLerp outputs 0.0 when distance is at innerDangerRange
            // and 1.0 when distance is at spawnActivationRange
            float t = Mathf.InverseLerp(innerDangerRange, spawnActivationRange, distance);
    
            // Lerp then picks the spawn rate: 
            // If t is 0 (Close), it picks minSpawnInterval.
            // If t is 1 (Far), it picks maxSpawnInterval.
            return Mathf.Lerp(waveConfig.minSpawnInterval, waveConfig.maxSpawnInterval, t);
        }
        
        private void UpdateWaveDifficulty()
        {
            waveConfig.currentSpawnDuration = Mathf.Max(waveConfig.minSpawnDuration,
                waveConfig.currentSpawnDuration - waveConfig.durationDecreaseRate
            );
        }
        
        private bool IsPlayerInRange() =>
            Vector3.Distance(transform.position, _playerTransform.position) <= spawnActivationRange;

        private IEnumerator WaitSecondsUnfrozen(float seconds)
        {
            var t = 0f;
            while (t < seconds)
            {
                if (!_isFrozen && IsPlayerInRange()) 
                    t += Time.deltaTime;
                yield return null;
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, spawnActivationRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, innerDangerRange);
        }

        private void OnFreeze() => _isFrozen = true;
        private void OnUnFreeze() => _isFrozen = false;
        
        /*private void OnGUI()
        {
            // Only show in the editor or development builds
            if (!Debug.isDebugBuild) return;

            GUIStyle style = new GUIStyle();
            style.fontSize = 18;
            style.normal.textColor = Color.white;

            float dist = playerTransform ? Vector3.Distance(transform.position, playerTransform.position) : 0;
            float currentInterval = GetDynamicInterval();

            GUILayout.BeginArea(new Rect(20, 20, 350, 200));
            GUILayout.Label($"--- WAVE MANAGER DEBUG ---", style);
            GUILayout.Label($"Player Distance: {dist:F2}", style);
            GUILayout.Label($"Current Interval: {currentInterval:F2}s", style);
            GUILayout.Label($"Wave Time Left: {waveConfig.currentSpawnDuration:F2}s", style);
    
            if (!IsPlayerInRange()) 
                GUILayout.Label($"STATUS: <color=yellow>PLAYER OUT OF RANGE</color>", style);
            else if (_isFrozen)
                GUILayout.Label($"STATUS: <color=cyan>FROZEN</color>", style);
            else
                GUILayout.Label($"STATUS: <color=red>ACTIVE & SPAWNING</color>", style);

            GUILayout.EndArea();
        }*/
    }
}

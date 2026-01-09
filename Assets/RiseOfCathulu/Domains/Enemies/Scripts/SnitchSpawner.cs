using RiseOfCathulu.Domains.Player.Scripts;
using RiseOfCathulu.Domains.Utilities.GameHandlers.Scripts;
using UnityEngine;

namespace RiseOfCathulu.Domains.Enemies.Scripts
{
    public class SnitchSpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GoldenSnitch snitchPrefab;
        [SerializeField] private GrowthConfig growthConfig;
        [SerializeField] private Collider2D gameArea;

        private PlayerSize _playerSize;
        private GoldenSnitch _currentSnitch;
        private bool _isFirstSpawn = true;  
        private void Awake()
        {
            _playerSize = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerSize>();
            SpawnSnitch();
        }

        private void OnEnable()
        {
            GameEvents.SnitchCaught += SpawnSnitch;
        }

        private void OnDisable()
        {
            GameEvents.SnitchCaught -= SpawnSnitch;
        }

        private void SpawnSnitch()
        {
            int level;
            if (_isFirstSpawn)
            {
                level = 2;
                _isFirstSpawn = false;
            }
            else
                level = _playerSize.CurrentSizeLevel;
            _currentSnitch = Instantiate(snitchPrefab);
            _currentSnitch.InitializeLevel(level, growthConfig);
            _currentSnitch.transform.position = GetRandomPointInArea();
        }

        private Vector3 GetRandomPointInArea()
        {
            Bounds b = gameArea.bounds;
            return new Vector3(Random.Range(b.min.x, b.max.x), Random.Range(b.min.y, b.max.y), 0f);
        }
    }
}
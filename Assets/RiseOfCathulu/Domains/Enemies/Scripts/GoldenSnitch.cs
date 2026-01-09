using System;
using RiseOfCathulu.Domains.Player.Scripts;
using RiseOfCathulu.Domains.Utilities.GameHandlers.Scripts;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RiseOfCathulu.Domains.Enemies.Scripts
{
    [RequireComponent(typeof(Collider2D))]
    public class GoldenSnitch : MonoBehaviour
    {
        private int _sizeLevel;

        [Header("Movement")]
        [SerializeField] private float baseMoveSpeed = 6f;
        [SerializeField] private float panicSpeedMultiplier = 1.8f;
        [SerializeField] private float noiseTimeScale = 0.35f;

        [Header("Distance Behavior")]
        [SerializeField] private float teaseDistance = 8f;
        [SerializeField] private float neutralDistance = 4f;
        [SerializeField] private float panicDistance = 2.5f;

        [Header("Attraction")]
        [SerializeField] private float teaseAttraction = 0.75f;
        [SerializeField] private float neutralAttraction = 0.25f;
        [SerializeField] private float fleeStrength = 1.1f;

        private Transform _player;
        private Rigidbody2D _rb;
        private float _moveSpeed;
        private bool _isFrozen;

        public void InitializeLevel(int level, GrowthConfig config)
        {
            _sizeLevel = level;
            float scale = config.GetScale(level);
            transform.localScale = Vector3.one * scale;

            _moveSpeed = baseMoveSpeed + config.GetSpeed(level) * 0.5f;
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _player = GameObject.FindGameObjectWithTag("Player").transform;
        }

        private void OnEnable()
        {
            GameEvents.FreezeLevel += Freeze;
            GameEvents.UnFreezeLevel += UnFreeze;
        }

        private void OnDisable()
        {
            GameEvents.FreezeLevel -= Freeze;
            GameEvents.UnFreezeLevel -= UnFreeze;
        }

        private void Freeze()
        {
            _isFrozen = true;
            if (_rb) _rb.linearVelocity = Vector2.zero;
        }

        private void UnFreeze()
        {
            _isFrozen = false;
        }

        private void Update()
        {
            if (_isFrozen || !_player) return;
            Move();
        }

        private void Move()
        {
            Vector2 toPlayer = _player.position - transform.position;
            float distance = toPlayer.magnitude;

            // Perlin noise drift
            float t = Time.time * noiseTimeScale + GetInstanceID();
            Vector2 noiseDir = new Vector2(
                Mathf.PerlinNoise(t, 0f) - 0.5f,
                Mathf.PerlinNoise(0f, t) - 0.5f
            ).normalized;

            float attraction;
            float speedMultiplier = 1f;

            if (distance > teaseDistance)
            {
                attraction = teaseAttraction;
            }
            else if (distance > neutralDistance)
            {
                attraction = neutralAttraction;
            }
            else
            {
                attraction = -fleeStrength;
                speedMultiplier = panicSpeedMultiplier;
            }

            Vector2 finalDir =
                noiseDir * (1f - Mathf.Abs(attraction)) +
                toPlayer.normalized * attraction;

            finalDir.Normalize();

            transform.position += (Vector3)(finalDir * _moveSpeed * speedMultiplier * Time.deltaTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            GameEvents.SnitchCaught?.Invoke();
            Destroy(gameObject);
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (!other.gameObject.CompareTag("Player")) return;
            GameEvents.SnitchCaught?.Invoke();
            Destroy(gameObject);
        }
    }
}

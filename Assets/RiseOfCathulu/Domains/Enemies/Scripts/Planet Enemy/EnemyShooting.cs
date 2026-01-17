using RiseOfCathulu.Domains.Player.Scripts;
using RiseOfCathulu.Domains.Utilities.GameHandlers.Scripts;
using RiseOfCathulu.Domains.Utilities.Sound.Scripts;
using UnityEngine;
using UnityEngine.Serialization;

namespace RiseOfCathulu.Domains.Enemies.Scripts.Planet_Enemy
{
    public class EnemyShooting : MonoBehaviour
    {
        [Header("Ball Shot")]
        [SerializeField, Tooltip("Delay before the enemy starts shooting")] private float startShootDelay = 6f;
        [SerializeField, Tooltip("Distance threshold for ball shot")] private float ballShootDistance = 120f;
        [SerializeField, Tooltip("Cooldown for ball shot")] private float ballShootCooldown = 3f;

        [SerializeField, Tooltip("Bullet force factor")] private float bulletForceFactor = 1f;
        [SerializeField] private float sizeFactor = 5f;
        [SerializeField] private GrowthConfig growthConfig;

        private GameObject _player;
        private PlayerSize _playerSize;
        private float _lastBallShootTime = -999f;
        private bool _isDestroyed;
        private bool _isFrozen;
        private float _shootStartTime;

        
        private void Awake()
        {
            _player = GameObject.FindGameObjectWithTag("Player");
            _playerSize = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerSize>();    
        }

        private void OnEnable()
        {
            _shootStartTime = Time.time + startShootDelay;
            GameEvents.PlanetEnemyDestroyed += StopShooting;
            GameEvents.FreezeLevel += OnFreeze;
            GameEvents.UnFreezeLevel += OnUnFreeze;
        }

        private void OnDisable()
        {
            GameEvents.PlanetEnemyDestroyed -= StopShooting;
            GameEvents.FreezeLevel -= OnFreeze;
            GameEvents.UnFreezeLevel -= OnUnFreeze;
        }

        private void StopShooting(Transform destroyedPlanet)
        {
            if (transform.parent != destroyedPlanet) return;
            _isDestroyed = true;
        }


        private void OnFreeze() => _isFrozen = true;
        private void OnUnFreeze() => _isFrozen = false;

        private void Update()
        {
            if (_isFrozen || _isDestroyed || Time.time < _shootStartTime) return;
            CheckProximityAttack();
        }

        private void CheckProximityAttack()
        {
            if (_player == null) return;

            var distance = Vector2.Distance(transform.position, _player.transform.position);
            if (distance > ballShootDistance) return;

            var cd = distance <= ballShootDistance * 0.5f ? ballShootCooldown * 0.5f : ballShootCooldown;
            if (Time.time - _lastBallShootTime >= cd)
            {
                ShootBallBullet();
                _lastBallShootTime = Time.time;
            }
        }

        private void ShootBallBullet()
        {
            var bullet = EnemyBulletPool.Instance.Get();
            bullet.transform.localScale =  growthConfig.GetScale(_playerSize.CurrentSizeLevel) * 
                                           (transform.localScale / sizeFactor);
            bullet.transform.position = transform.position;
            var dir = (_player.transform.position - transform.position).normalized;
            var rb = bullet.GetComponent<Rigidbody2D>();
            rb.linearVelocity = dir * (growthConfig.GetMaxSpeed(_playerSize.CurrentSizeLevel) * bulletForceFactor);
            SoundManager.Instance.PlaySound("Planet Enemy Bullet", transform);
        }
    }
}

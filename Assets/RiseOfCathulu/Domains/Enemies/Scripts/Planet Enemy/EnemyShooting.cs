using RiseOfCathulu.Domains.Utilities.GameHandlers.Scripts;
using RiseOfCathulu.Domains.Utilities.Sound.Scripts;
using UnityEngine;

namespace RiseOfCathulu.Domains.Enemies.Scripts.Planet_Enemy
{
    public class EnemyShooting : BossBaseMono
    {
        [Header("Ball Shot")]
        [SerializeField, Tooltip("Distance threshold for ball shot")] private float ballShootDistance = 120f;
        [SerializeField, Tooltip("Cooldown for ball shot")] private float ballShootCooldown = 3f;
        [SerializeField, Tooltip("Bullet launch force")] private float bulletForce = 20f;

        private GameObject _player;
        private float _lastBallShootTime = -999f;
        private bool _isDestroyed;
        private bool _isFrozen;
        
        private void Awake()
        {
            _player = GameObject.FindGameObjectWithTag("Player");
        }

        private void OnEnable()
        {
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
            if (transform.parent != destroyedPlanet)
                return;
            _isDestroyed = true;
        }


        private void OnFreeze() => _isFrozen = true;
        private void OnUnFreeze() => _isFrozen = false;

        private void Update()
        {
            if (_isFrozen || _isDestroyed) return;
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
            bullet.transform.position = transform.position;
            var dir = (_player.transform.position - transform.position).normalized;
            var rb = bullet.GetComponent<Rigidbody2D>();
            rb.linearVelocity = dir * bulletForce;
            SoundManager.Instance.PlaySound("Planet Enemy Bullet", transform);
        }
    }
}

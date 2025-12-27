using B.O.S.S.Domains.Utilities.GameHandlers.Scripts;
using B.O.S.S.Domains.Utilities.Sound.Scripts;
using UnityEngine;

namespace B.O.S.S.Domains.Enemies.Scripts.Planet_Enemy
{
    public class EnemyShooting : BossBaseMono
    {

        [Header("Player Target")]
        [SerializeField, Tooltip("Player GameObject to target")] private GameObject player;

        [Header("Ball Shot")]
        [SerializeField, Tooltip("Distance threshold for ball shot")] private float ballShootDistance = 120f;
        [SerializeField, Tooltip("Cooldown for ball shot")] private float ballShootCooldown = 3f;
        [SerializeField, Tooltip("Bullet launch force")] private float bulletForce = 20f;

        private float _lastBallShootTime = -999f;
        private bool _isFrozen;
        

        private void OnEnable()
        {
            GameEvents.FreezeLevel += OnFreeze;
            GameEvents.UnFreezeLevel += OnUnFreeze;
        }

        private void OnDisable()
        {
            GameEvents.FreezeLevel -= OnFreeze;
            GameEvents.UnFreezeLevel -= OnUnFreeze;
        }

        private void OnFreeze() => _isFrozen = true;
        private void OnUnFreeze() => _isFrozen = false;
        
        

        private void Update()
        {
            if (_isFrozen) return;
            CheckProximityAttack();
        }
        

        private void CheckProximityAttack()
        {
            if (player == null) return;

            var distance = Vector2.Distance(transform.position, player.transform.position);
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
            var dir = (player.transform.position - transform.position).normalized;
            var rb = bullet.GetComponent<Rigidbody2D>();
            rb.linearVelocity = dir * bulletForce;
            SoundManager.Instance.PlaySound("Boss Bullet", transform);
        }
    }
}

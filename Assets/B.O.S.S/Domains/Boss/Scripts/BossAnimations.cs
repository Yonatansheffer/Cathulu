using B.O.S.S.Domains.Utilities.GameHandlers.Scripts;
using UnityEngine;

namespace B.O.S.S.Domains.Boss.Scripts
{
    public class BossAnimations : MonoBehaviour
    {
        private static readonly int Shoot = Animator.StringToHash("shoot");
        private static readonly int Spawn = Animator.StringToHash("spawn");
        private static readonly int Damage = Animator.StringToHash("damage");
        private static readonly int Death = Animator.StringToHash("death");
        private Animator _animator;

        private void Start()
        {
            _animator = GetComponent<Animator>();
        }

        private void OnEnable()
        {
            GameEvents.BossShoots += TriggerShootAnimation;
            GameEvents.ToSpawnEnemy += TriggerSpawnAnimation;
            GameEvents.EnemySpawned += TriggerSpawnAnimation;
            GameEvents.BossDestroyed += DeathAnimation;
        }
        
        private void OnDisable()
        {
            GameEvents.BossShoots -= TriggerShootAnimation;
            GameEvents.ToSpawnEnemy -= TriggerSpawnAnimation;
            GameEvents.EnemySpawned -= TriggerSpawnAnimation;
            GameEvents.BossDestroyed -= DeathAnimation;
        }
        
        private void DeathAnimation()
        {
            _animator.SetTrigger(Death);
        }

        private void TriggerShootAnimation()
        {
            _animator.SetTrigger(Shoot);
        }

        private void TriggerSpawnAnimation()
        {
            _animator.SetTrigger(Spawn);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if(other.CompareTag("Weapon"))
            {
                _animator.SetTrigger(Damage);
            }
        }
    }
}
using System;
using GameHandlers;
using UnityEngine;

namespace _WHY.Scripts.Boss
{
    public class BossAnimations : MonoBehaviour
    {
        private static readonly int Shoot = Animator.StringToHash("shoot");
        private static readonly int Spawn = Animator.StringToHash("spawn");
        private static readonly int Die = Animator.StringToHash("die");
        private static readonly int Damage = Animator.StringToHash("damage");

        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void OnEnable()
        {
            GameEvents.BossShoots += TriggerShootAnimation;
            //GameEvents.ToSpawnEnemy += TriggerSpawnAnimation;
            //GameEvents.BossLivesChanged += TriggerDamageAnimation;
        }
        
        private void OnDisable()
        {
            GameEvents.BossShoots -= TriggerShootAnimation;
            //GameEvents.ToSpawnEnemy -= TriggerSpawnAnimation;
            //GameEvents.BossLivesChanged -= TriggerDamageAnimation;
        }

        private void TriggerShootAnimation()
        {
            print("sdsd");
            _animator.SetTrigger(Shoot);
        }

        private void TriggerSpawnAnimation(bool dummy)
        {
            _animator.SetTrigger(Spawn);
        }
        
        private void TriggerDamageAnimation(int currentHealth)
        {
            _animator.SetTrigger(currentHealth <= 0 ? Die : Damage);
        }
        
    }
}
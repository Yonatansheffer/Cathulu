using GameHandlers;
using Sound;
using UnityEngine;

namespace _WHY.Scripts.Boss
{
    public class BossHealth : WHYBaseMono
    {
        [SerializeField] private int initialBossHealth = 100;
        private int _currentHealth;
        
        private void Start()
        {
            _currentHealth = initialBossHealth;
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Weapon"))
            {
                _currentHealth--;
                if(_currentHealth <= 0)
                {
                    GameEvents.BossDestroyed?.Invoke();
                    return;
                }
                SoundManager.Instance.PlaySound("Boss Damage", transform);
                GameEvents.BossLivesChanged?.Invoke(_currentHealth);
            }
        }
    }
}
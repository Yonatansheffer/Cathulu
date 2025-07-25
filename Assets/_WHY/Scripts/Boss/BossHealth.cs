using GameHandlers;
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
            GameEvents.BossLivesChanged?.Invoke(_currentHealth);
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Weapon"))
            {
                _currentHealth -= 2;
                GameEvents.BossLivesChanged?.Invoke(_currentHealth);
                GameEvents.UpdateHealthUI?.Invoke(_currentHealth, false);
            }
        }
    }
}
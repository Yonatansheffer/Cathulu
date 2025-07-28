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
                SoundManager.Instance.PlaySound("Explosion", transform);
                GameEvents.BossLivesChanged?.Invoke(_currentHealth);
                GameEvents.UpdateHealthUI?.Invoke(_currentHealth, false);
            }
        }
    }
}
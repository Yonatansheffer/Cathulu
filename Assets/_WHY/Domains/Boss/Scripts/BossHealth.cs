using _WHY.Domains.Utilities.GameHandlers.Scripts;
using Sound;
using UnityEngine;

namespace _WHY.Domains.Boss.Scripts
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
            if (!other.CompareTag("Weapon")) return;
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
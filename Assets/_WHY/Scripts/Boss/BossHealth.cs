using GameHandlers;
using UnityEngine;

namespace _WHY.Scripts.Boss
{
    public class BossHealth : WHYBaseMono
    {
        [SerializeField] private const int InitialBossHealth = 100;
        private int currentHealth;
        
        private void Start()
        {
            currentHealth = InitialBossHealth;
            GameEvents.BossLivesChanged?.Invoke(currentHealth);
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Weapon"))
            {
                currentHealth -= 2;
                GameEvents.BossLivesChanged?.Invoke(currentHealth);
                GameEvents.UpdateHealthUI?.Invoke(currentHealth, false);
            }
        }
    }
}
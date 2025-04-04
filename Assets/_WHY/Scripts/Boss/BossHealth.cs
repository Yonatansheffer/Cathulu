using GameHandlers;
using UnityEngine;

namespace _WHY.Scripts.Boss
{
    public class BossHealth : WHYBaseMono
    {
        private int currentHealth;
        
        private void Start()
        {
            currentHealth = GameManager.GetInitialBossHealth();
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Weapon"))
            {
                currentHealth -= 2;
                GameEvents.BossDamaged?.Invoke(currentHealth);
                if (currentHealth <= 0)
                {
                    Destroy(gameObject);
                    GameEvents.BossDestroyed?.Invoke();
                }
            }
        }
    }
}
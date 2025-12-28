using RiseOfCathulu.Domains.Utilities.GameHandlers.Scripts;
using RiseOfCathulu.Domains.Utilities.Sound.Scripts;
using UnityEngine;

namespace RiseOfCathulu.Domains.Enemies.Scripts.Planet_Enemy
{
    public class PlanetEnemyHealth : BossBaseMono
    {
        [SerializeField, Tooltip("Boss initial amount of lives")] private int initialBossHealth = 100;
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
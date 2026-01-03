using RiseOfCathulu.Domains.Utilities.GameHandlers.Scripts;
using RiseOfCathulu.Domains.Utilities.Sound.Scripts;
using UnityEngine;
using UnityEngine.Serialization;

namespace RiseOfCathulu.Domains.Enemies.Scripts.Planet_Enemy
{
    public class PlanetEnemyHealth : MonoBehaviour
    {
        [SerializeField, Tooltip("initial amount of lives")] private int initialHealth = 100;
        private int _currentHealth;
        
        private void Start()
        {
            _currentHealth = initialHealth;
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Weapon")) return;
            _currentHealth--;
            if(_currentHealth <= 0)
            {
                GameEvents.PlanetEnemyDestroyed?.Invoke(transform.parent);
                return;
            }
            SoundManager.Instance.PlaySound("Boss Damage", transform);
        }
    }
}
using System.Collections;
using B.O.S.S.Domains.Utilities.GameHandlers.Scripts;
using B.O.S.S.Domains.Utilities.Sound.Scripts;
using UnityEngine;

namespace B.O.S.S.Domains.Player.Scripts
{
    public class PlayerHealth : MonoBehaviour
    {
        private const int InitialPlayerHealth = 100;
        private bool _isShieldActive;
        private int _currentPlayerHealth;
        private bool _isOnHitCooldown;

        public static int GetInitialPlayerHealth() => InitialPlayerHealth;

        private void Awake()
        {
            _isShieldActive = false;
            _isOnHitCooldown = false;
            _currentPlayerHealth = InitialPlayerHealth;
        }

        private void OnEnable()
        {
            GameEvents.AddLifeToPlayer += AdjustLives;
            GameEvents.ShieldUpdated += UpdateShield;
        }

        private void OnDisable()
        {
            GameEvents.AddLifeToPlayer -= AdjustLives;
            GameEvents.ShieldUpdated -= UpdateShield;
        }

        private void UpdateShield(bool isActive)
        {
            _isShieldActive = isActive;
            StartCoroutine(HandleCoolDown());
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if ((other.gameObject.CompareTag("Enemy") || other.gameObject.CompareTag("Boss") || 
                 other.gameObject.CompareTag("Boss Bullet")) && !_isShieldActive && !_isOnHitCooldown)
                HandleDamage();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if ((other.gameObject.CompareTag("Enemy") || other.gameObject.CompareTag("Boss") || 
                 other.gameObject.CompareTag("Boss Bullet")) && !_isShieldActive && !_isOnHitCooldown)
                HandleDamage();
        }

        private void HandleDamage()
        {
            AdjustLives(-1);
            StartCoroutine(HandleCoolDown());
        }
        
        private IEnumerator HandleCoolDown()
        {
            _isOnHitCooldown = true;
            yield return new WaitForSeconds(1f);
            _isOnHitCooldown = false;
        }

        private void AdjustLives(int amount)
        {
            if (amount > 0 && _currentPlayerHealth >= InitialPlayerHealth) return;
            _currentPlayerHealth += amount;
            GameEvents.UpdatePlayerLivesUI?.Invoke(_currentPlayerHealth);
            if (amount > 0) return;
            GameEvents.ShakeCamera?.Invoke();
            if(_currentPlayerHealth == 0)
            {
                GameEvents.PlayerDefeated?.Invoke();
                SoundManager.Instance.PlaySound("Lost Life", transform);
            }
            if (amount < 0 && _currentPlayerHealth > 0)
            {
                GameEvents.PlayerLostLife?.Invoke(_currentPlayerHealth);
                SoundManager.Instance.PlaySound("Shield Hit", transform);
            }
        }
    }
}
using System;
using System.Collections;
using GameHandlers;
using UnityEngine;
using Sound;
using Weapons;

namespace MainPlayer
{
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private const int InitialPlayerHealth = 3;
        private bool _isShieldActive;
        private SpriteRenderer _spriteRenderer;
        private int _currentPlayerHealth; 
        private bool _isOnHitCooldown;
        
        public static int GetInitialPlayerHealth() => InitialPlayerHealth;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
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
            if (isActive)
            {
                _isShieldActive = true;
            }
            else
            {
                StartCoroutine(BlinkPlayer());
            }
        }
        
        private IEnumerator BlinkPlayer()
        {
            var endTime = Time.time + 1f;
            while (Time.time < endTime)
            {
                _spriteRenderer.enabled = !_spriteRenderer.enabled; 
                yield return new WaitForSeconds(0.1f);
            }
            _spriteRenderer.enabled = true; 
            _isShieldActive = false;
        }
        
        private void OnCollisionEnter2D(Collision2D other)
        {
            if ((other.gameObject.CompareTag("Enemy") || other.gameObject.CompareTag("Boss") || 
                 other.gameObject.CompareTag("Boss Bullet")) && !_isShieldActive && !_isOnHitCooldown)
            {
                StartCoroutine(HandleDamage());
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if ((other.gameObject.CompareTag("Enemy") || other.gameObject.CompareTag("Boss") || 
                 other.gameObject.CompareTag("Boss Bullet")) && !_isShieldActive && !_isOnHitCooldown)
            {
                StartCoroutine(HandleDamage());
            }
        }

        private IEnumerator HandleDamage()
        {
            _isOnHitCooldown = true;
            AdjustLives(-1);
            yield return new WaitForSeconds(1f);
            _isOnHitCooldown = false;
        }

        private void AdjustLives(int amount)
        {
            _currentPlayerHealth += amount;
            if (_currentPlayerHealth <= 0)
            {
                GameEvents.PlayerLivesChanged?.Invoke(_currentPlayerHealth);
                GameEvents.GameOver?.Invoke();
                SoundManager.Instance.PlaySound("Lost Life", transform);
                return;
            }
            if(amount <0 && _currentPlayerHealth > 0)
            {
                StartCoroutine(BlinkPlayer());
                GameEvents.ShakeCamera?.Invoke();
                SoundManager.Instance.PlaySound("Shield Hit", transform);
            }
            if (_currentPlayerHealth >= InitialPlayerHealth)
            {
                _currentPlayerHealth = InitialPlayerHealth;
            }
            GameEvents.PlayerLivesChanged?.Invoke(_currentPlayerHealth);
        }
        
        /*private IEnumerator OnPlayerHit()
        {
            GameEvents.FreezeStage?.Invoke();
            yield return new WaitForSeconds(1f); // 1 second delay
            SoundManager.Instance.PlaySound("Lost Life", transform);
            GameEvents.PlayerHit?.Invoke();
            GameEvents.ResetWeaponUI?.Invoke(); // reset weapon
        }*/
    }
}

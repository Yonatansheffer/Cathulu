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
        [SerializeField] private int initialPlayerHealth = 10;
        private bool _isShieldActive;
        private SpriteRenderer _spriteRenderer;
        private int _currentPlayerHealth; 
        
        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _isShieldActive = false;
            _currentPlayerHealth = initialPlayerHealth; 
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
                _spriteRenderer.enabled = !_spriteRenderer.enabled; // Toggle visibility
                yield return new WaitForSeconds(0.1f); // Wait for 0.1 seconds before toggling again
            }
            _spriteRenderer.enabled = true; // Ensure the sprite is visible after blinking
            _isShieldActive = false;
        }
        

        private void OnCollisionEnter2D(Collision2D other)
        {
            if ((other.gameObject.CompareTag("Enemy") || other.gameObject.CompareTag("Boss"))  && !_isShieldActive)
            {
                //StartCoroutine(OnPlayerHit());
                AdjustLives(-1);
                SoundManager.Instance.PlaySound("Lost Life", transform);
            }
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if ((other.gameObject.CompareTag("Enemy") || other.gameObject.CompareTag("Boss"))  && !_isShieldActive)
            {
                //StartCoroutine(OnPlayerHit());
                AdjustLives(-1);
                SoundManager.Instance.PlaySound("Lost Life", transform);
            }
        }
        
        private void AdjustLives(int amount)
        {
            _currentPlayerHealth += amount;
            if (_currentPlayerHealth >= initialPlayerHealth)
            {
                _currentPlayerHealth = initialPlayerHealth;
            }
            GameEvents.UpdateHealthUI?.Invoke(_currentPlayerHealth, true);
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

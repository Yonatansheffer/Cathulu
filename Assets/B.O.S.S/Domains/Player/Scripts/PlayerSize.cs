using System;
using System.Collections;
using System.Collections.Generic;
using B.O.S.S.Domains.Enemies.Scripts;
using B.O.S.S.Domains.Utilities.GameHandlers.Scripts;
using B.O.S.S.Domains.Utilities.Sound.Scripts;
using UnityEngine;

namespace B.O.S.S.Domains.Player.Scripts
{
    public class PlayerSize : MonoBehaviour
    {
        [Header("Size Levels")]
        [SerializeField] private int minSizeLevel = 0;
        [SerializeField] private int initialSizeLevel = 2;
        [SerializeField] private int maxSizeLevel = 20;

        [Header("Scale Values")]
        [SerializeField] private float minScale = 0.5f;
        [SerializeField] private float maxScale = 3.5f;

        [Header("Hit Cooldown")]
        [SerializeField] private float hitCooldown = 1f;

        private Dictionary<int, float> _sizeLevelToScale;
        private int _currentSizeLevel;

        private bool _isShieldActive;
        private bool _isOnHitCooldown;

        public int CurrentSizeLevel => _currentSizeLevel;
        public bool IsBiggerThan(int enemySizeLevel) => _currentSizeLevel > enemySizeLevel;
        
        private void Awake()
        {
            BuildSizeDictionary();
            _currentSizeLevel = initialSizeLevel;
            ApplyScale();
        }

        
        private void OnEnable()
        {
            GameEvents.ChangePlayerSize += AdjustSize;
            GameEvents.ShieldUpdated += UpdateShield;
        }

        private void OnDisable()
        {
            GameEvents.ChangePlayerSize -= AdjustSize;
            GameEvents.ShieldUpdated -= UpdateShield;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            HandleHit(collision.collider);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            HandleHit(other);
        }

        private void HandleHit(Collider2D other)
        {
            if (_isShieldActive || _isOnHitCooldown) return;

            // Enemy bullets always hurt
            if (other.CompareTag("Enemy Bullet"))
            {
                TakeHit();
                return;
            }

            if (!other.CompareTag("Enemy")) return;

            var enemy = other.GetComponent<EnemyEatable>(); // or EnemyEatable / FlyingEnemy
            if (enemy == null) return;

            // Eat
            if (enemy.IsEatable && _currentSizeLevel > enemy.SizeLevel)
            {
                AdjustSize(+1);
                SoundManager.Instance.PlaySound("Eat", transform);
                return;
            }

            // Otherwise → hit
            TakeHit();
        }

        
        private void TakeHit()
        {
            AdjustSize(-1);
            GameEvents.ShakeCamera?.Invoke();
            SoundManager.Instance.PlaySound("Shield Hit", transform);
            GameEvents.PlayerLostLife?.Invoke(_currentSizeLevel);

            if (_currentSizeLevel < minSizeLevel)
            {
                GameEvents.PlayerDefeated?.Invoke();
                SoundManager.Instance.PlaySound("Lost Life", transform);
            }

            StartCoroutine(HitCooldown());
        }

        private void AdjustSize(int delta)
        {
            int newLevel = Mathf.Clamp(
                _currentSizeLevel + delta,
                minSizeLevel,
                maxSizeLevel
            );

            if (newLevel == _currentSizeLevel)
                return;

            _currentSizeLevel = newLevel;
            ApplyScale();
        }

        private void ApplyScale()
        {
            float scale = _sizeLevelToScale[_currentSizeLevel];
            transform.localScale = Vector3.one * scale;
        }

        private void BuildSizeDictionary()
        {
            _sizeLevelToScale = new Dictionary<int, float>();

            for (int level = minSizeLevel; level <= maxSizeLevel; level++)
            {
                float t = Mathf.InverseLerp(minSizeLevel, maxSizeLevel, level);
                float scale = Mathf.Lerp(minScale, maxScale, t);
                _sizeLevelToScale[level] = scale;
            }
        }
        
        private void UpdateShield(bool isActive)
        {
            _isShieldActive = isActive;
        }

        private IEnumerator HitCooldown()
        {
            _isOnHitCooldown = true;
            yield return new WaitForSeconds(hitCooldown);
            _isOnHitCooldown = false;
        }
    }
}
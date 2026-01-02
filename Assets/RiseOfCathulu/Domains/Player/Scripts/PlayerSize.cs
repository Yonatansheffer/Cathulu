using System;
using System.Collections;
using System.Collections.Generic;
using RiseOfCathulu.Domains.Enemies.Scripts;
using RiseOfCathulu.Domains.Utilities.GameHandlers.Scripts;
using RiseOfCathulu.Domains.Utilities.Sound.Scripts;
using UnityEngine;

namespace RiseOfCathulu.Domains.Player.Scripts
{
    public class PlayerSize : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private GrowthConfig growthConfig; 
        [SerializeField] private int initialSizeLevel = 2;

        [Header("Scale Values")]
        [SerializeField] private float minScale = 0.5f;
        [SerializeField] private float maxScale = 3.5f;

        [Header("Hit Cooldown")]
        [SerializeField] private float hitCooldown = 1f;

        private TrailRenderer _trailRenderer;
        private int _currentSizeLevel;
        private bool _isShieldActive;
        private bool _isOnHitCooldown;

        public int CurrentSizeLevel => _currentSizeLevel;
        public float CurrentScale => transform.localScale.x;

      
        private void Awake()
        {
            _currentSizeLevel = initialSizeLevel;
            _trailRenderer = GetComponent<TrailRenderer>();
            _trailRenderer.widthMultiplier = _currentSizeLevel * 10;
            _trailRenderer.time = (_currentSizeLevel * 0.1f);
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
            if (other.CompareTag("Enemy Bullet"))
            {
                TakeHit();
                return;
            }
            if (!other.CompareTag("Enemy")) return;
            var enemy = other.GetComponent<FlyingEnemy>(); 
            if (enemy == null) return;
            if (enemy.IsEatable && _currentSizeLevel > enemy.sizeLevel)
            {
                AdjustSize(+1);
                SoundManager.Instance.PlaySound("Eat", transform);
                return;
            }
            TakeHit();
        }

        private void Update()
        {
            CheatSize();
        }


        private void CheatSize()
        {
            if (Input.GetKeyDown(KeyCode.B)) AdjustSize(+1);
            if (Input.GetKeyDown(KeyCode.N)) AdjustSize(-1);
        }

        
        private void TakeHit()
        {
            AdjustSize(-1);
            GameEvents.ShakeCamera?.Invoke();
            SoundManager.Instance.PlaySound("Shield Hit", transform);
            GameEvents.PlayerLostLife?.Invoke(_currentSizeLevel);
            if (_currentSizeLevel < growthConfig.minLevel)
            {
                GameEvents.PlayerDefeated?.Invoke();
                SoundManager.Instance.PlaySound("Lost Life", transform);
            }
            StartCoroutine(HitCooldown());
        }

        private void AdjustSize(int delta)
        {
            int previousSize = _currentSizeLevel;

            _currentSizeLevel = Mathf.Clamp(_currentSizeLevel + delta, growthConfig.minLevel, growthConfig.maxLevel);

            if (_currentSizeLevel != previousSize)
            {
                GetComponent<PlayerDualSenseFeedback>()?
                    .TriggerSizeChangeRumble(_currentSizeLevel - previousSize);
            }

            _trailRenderer.widthMultiplier = _currentSizeLevel * 10;
            _trailRenderer.time = (_currentSizeLevel * 0.1f);
            ApplyScale();
        }

        private void ApplyScale()
        {
            float scale = growthConfig.GetScale(_currentSizeLevel);
            transform.localScale = Vector3.one * scale;
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
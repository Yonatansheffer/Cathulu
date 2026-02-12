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

        [Header("Hit Cooldown")]
        [SerializeField] private float hitCooldown = 1f;
        
        [Header("Light Object")]
        [SerializeField] private GameObject lightObject;
        [SerializeField] private float sizeThreshold = 5f;
        [SerializeField] private float flashSpeed = 3f;
        private SpriteRenderer _lightSprite;
        private Vector3 _initialLightScale; 
        private bool _isFirstMinLevel = true;

        private TrailRenderer _trailRenderer;
        [SerializeField] private float minTrailWidth = 3f;
        [SerializeField] private float trailWidthPerSize = 10f;

        [SerializeField] private float minTrailTime = 0.1f;
        [SerializeField] private float trailTimePerSize = 0.1f;
        
        private PlayerGravityMotor _motor;
        private int _currentSizeLevel;
        private bool _isShieldActive;
        private bool _isOnHitCooldown;
        public int CurrentSizeLevel => _currentSizeLevel;
        public float CurrentScale => transform.localScale.x;
        [SerializeField] private bool isOpening;

      
        private void Awake()
        {
            _motor = GetComponent<PlayerGravityMotor>();
            _lightSprite = lightObject.GetComponent<SpriteRenderer>();
            _currentSizeLevel = initialSizeLevel;
            _trailRenderer = GetComponent<TrailRenderer>();
            _trailRenderer.widthMultiplier = _currentSizeLevel * 10;
            _trailRenderer.time = (_currentSizeLevel * 0.1f);
            ApplyScale();
        }
        
        private void OnEnable()
        {
            GameEvents.ShieldUpdated += UpdateShield;
            GameEvents.PlayerGrow += AdjustSize;
        }

        private void OnDisable()
        {
            GameEvents.ShieldUpdated -= UpdateShield;
            GameEvents.PlayerGrow -= AdjustSize;
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
            if (other.CompareTag("Snitch"))
            {
                AdjustSize(+5);
            }
            if (_isShieldActive || _isOnHitCooldown) return;
            if (other.CompareTag("Enemy Bullet"))
            {
                TakeHit();
                return;
            }
            if (!other.CompareTag("Enemy")) return;
            var enemy = other.GetComponent<FlyingEnemy>(); 
            if (enemy == null) return;
            if (_currentSizeLevel >= enemy.sizeLevel)
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
            UpdateLight();
        }
        
        private void UpdateLight()
        {
            if (_currentSizeLevel == growthConfig.minLevel)
            {
                if (_isFirstMinLevel)
                {
                    _isFirstMinLevel = false;
                }
                 // 1. Handle Color Flashing
                float t = Mathf.PingPong(Time.time * flashSpeed, 1f);
                _lightSprite.color = Color.Lerp(Color.red, Color.white, t);
                // 2. Handle Size Pulsing
                float pulse = Mathf.PingPong(Time.time * flashSpeed, sizeThreshold);
                lightObject.transform.localScale = _initialLightScale + (Vector3.one * pulse);
            }
            if (_currentSizeLevel-1 == growthConfig.minLevel && _isFirstMinLevel)
            {
                _initialLightScale = lightObject.transform.localScale;
            }
            if (_currentSizeLevel - 1 == growthConfig.minLevel && !_isFirstMinLevel)
            {
                lightObject.transform.localScale = _initialLightScale;
                _lightSprite.color = Color.white;
            }
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
            StartCoroutine(HitCooldown());
        }

        private void AdjustSize(int delta)
        {
           int previousSize = _currentSizeLevel;
           int newSize = _currentSizeLevel + delta;
           if (newSize < growthConfig.minLevel && !isOpening)
           {
               GameEvents.PlayerDefeated?.Invoke();
               SoundManager.Instance.PlaySound("Lost Life", transform);
               return;
           }
           _currentSizeLevel = Mathf.Clamp(newSize, growthConfig.minLevel, growthConfig.maxLevel);
            if (_currentSizeLevel != previousSize)
            {
                GetComponent<PlayerDualSenseFeedback>()?
                    .TriggerSizeChangeRumble(_currentSizeLevel - previousSize);
            }
            float targetWidth = _currentSizeLevel * trailWidthPerSize;
            _trailRenderer.widthMultiplier = Mathf.Max(minTrailWidth, targetWidth);

            float targetTime = _currentSizeLevel * trailTimePerSize;
            _trailRenderer.time = Mathf.Clamp(targetTime, minTrailTime, 1f);
            ApplyScale();
            if (_motor != null)
            {
                float speed = growthConfig.GetMaxSpeed(_currentSizeLevel);
                if (_currentSizeLevel >= 10 && _currentSizeLevel < 20)
                {
                    speed *= _currentSizeLevel/4f; // Scale speed up significantly at higher levels
                }
                if (_currentSizeLevel >= 20 && _currentSizeLevel < 30)
                {
                    speed *= _currentSizeLevel/2f; // Scale speed up significantly at higher levels
                }
                if (_currentSizeLevel >= 30 && _currentSizeLevel < 40)
                {
                    speed *= _currentSizeLevel/1.2f; // Scale speed up significantly at higher levels
                }
                _motor.ApplySizeStats(
                    speed,
                    growthConfig.GetConvergence(_currentSizeLevel)
                );
            }
        }

        private void ApplyScale()
        {
            float scale = growthConfig.GetScale(_currentSizeLevel);
            transform.localScale = Vector3.one * scale;
            
        }
      
        private void UpdateShield(bool isActive)
        {
            if (isActive)
            {
                _isShieldActive = true;
            }
            else
            {
                StartCoroutine(ClearShieldGrace());
            }
        }
        
        private IEnumerator ClearShieldGrace()
        {
            yield return new WaitForSeconds(hitCooldown);
            _isShieldActive = false;
        }
        
        private IEnumerator HitCooldown()
        {
            _isOnHitCooldown = true;
            yield return new WaitForSeconds(hitCooldown);
            _isOnHitCooldown = false;
        }
    }
}
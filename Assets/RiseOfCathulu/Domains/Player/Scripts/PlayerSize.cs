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
        private int _currentSizeLevel;
        private bool _isShieldActive;
        private bool _isOnHitCooldown;

        public int CurrentSizeLevel => _currentSizeLevel;
        public float CurrentScale => transform.localScale.x;

      
        private void Awake()
        {
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
        }

        private void OnDisable()
        {
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
            print( "shield " + _isShieldActive + " hit" + _isOnHitCooldown);
            if (_isShieldActive || _isOnHitCooldown) return;
            if (other.CompareTag("Enemy Bullet"))
            {
                print("hello");
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
            print("this " + _currentSizeLevel + " min " + growthConfig.minLevel);
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
            if (isActive)
            {
                StartCoroutine(ClearShieldGrace());
            }
            else
            {
                _isShieldActive = true;
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
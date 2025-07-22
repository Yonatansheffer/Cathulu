using GameHandlers;
using Sound;
using UnityEngine;

namespace MainPlayer
{
    public class Shield : MonoBehaviour
    {
        private SpriteRenderer _spriteRenderer;
        private CapsuleCollider2D _capsuleCollider;
        private Animator _animator;
        
        private void Awake()
        {
            _capsuleCollider = GetComponent<CapsuleCollider2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            UnActivateShield();
        }
        private void OnEnable()
        {
            GameEvents.BossDestroyed += UnActivateShield;
            GameEvents.ShieldCollected += ActivateShield;
        }
        
        private void OnDisable()
        {
            GameEvents.BossDestroyed -= UnActivateShield;
            GameEvents.ShieldCollected -= ActivateShield;
        }
    
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Enemy"))
            {
                SoundManager.Instance.PlaySound("Shield Hit", transform);
                GameEvents.ShieldHit?.Invoke();
                UnActivateShield();
            }
        }
    
        private void ActivateShield()
        {
            _capsuleCollider.enabled = true;
            _spriteRenderer.enabled = true;
        }
    
        private void UnActivateShield()
        {
            _capsuleCollider.enabled = false;
            _spriteRenderer.enabled = false;
        }
        
    }
}
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
        }
        private void OnEnable()
        {
            GameEvents.ShieldUpdated += UpdateShield;
        }
        
        private void OnDisable()
        {
            GameEvents.ShieldUpdated -= UpdateShield;
        }
    
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Enemy") || other.CompareTag("Boss") || other.CompareTag("Boss Bullet"))
            {
                GameEvents.ShakeCamera?.Invoke();
                SoundManager.Instance.PlaySound("Shield Hit", transform);
                GameEvents.ShieldUpdated?.Invoke(false);
            }
        }
    
        private void UpdateShield(bool isActive)
        {
            _capsuleCollider.enabled = isActive;
            _spriteRenderer.enabled = isActive;
        }
        
    }
}
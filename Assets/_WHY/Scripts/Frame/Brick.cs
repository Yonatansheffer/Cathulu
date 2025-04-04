using GameHandlers;
using Sound;
using UnityEngine;

namespace Frame
{
    public class Brick : MonoBehaviour
    {
        private static readonly int Reset = Animator.StringToHash("Reset");
        private static readonly int Break = Animator.StringToHash("Break");
        private SpriteRenderer _spriteRenderer;
        private BoxCollider2D _boxCollider;
        private Animator _animator;
        
        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _boxCollider = GetComponent<BoxCollider2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            GameEvents.ReadyStage += OnRestart;
        }
        
        private void OnDisable()
        {   
            GameEvents.ReadyStage -= OnRestart;
        }
        
        private void OnRestart()
        {
            _boxCollider.enabled = true; // Re-enable the collider
            _spriteRenderer.enabled = true;
            _animator.SetTrigger(Reset);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Weapon"))
            {
                SoundManager.Instance.PlaySound("Brick", transform);
                _animator.SetTrigger(Break);
            }
        }

        // Called by the animation event
        public void RemoveBrick()
        { 
            _boxCollider.enabled = false; 
            _spriteRenderer.enabled = false;
        }
    }
}
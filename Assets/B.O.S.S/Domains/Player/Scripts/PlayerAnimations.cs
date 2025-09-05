using System.Collections;
using _WHY.Domains.Utilities.GameHandlers.Scripts;
using UnityEngine;

namespace _WHY.Domains.Player.Scripts
{
    public class PlayerAnimations : MonoBehaviour
    {
        private static readonly int IsMovingRight = Animator.StringToHash("IsMovingRight");
        private static readonly int IsMovingLeft = Animator.StringToHash("IsMovingLeft");
        private static readonly int Death = Animator.StringToHash("death");
        
        [Header("References")]
        [SerializeField, Tooltip("Capsule GameObject for player visuals")] private GameObject light;
        [SerializeField, Tooltip("Gun GameObject for player weapon")] private GameObject gun;
        private Animator _animator;
        private SpriteRenderer _spriteRenderer;
        private Rigidbody2D _rb;


        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _rb = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            GameEvents.PlayerDefeated += DeathAnimation;
            GameEvents.PlayerLostLife += HitAnimation;
        }

        private void OnDisable()
        {
            GameEvents.PlayerDefeated -= DeathAnimation;
            GameEvents.PlayerLostLife -= HitAnimation;
        }

        private void DeathAnimation()
        {
            if (light != null) light.SetActive(false);
            if (gun != null) gun.SetActive(false);
            if (_animator != null) _animator.SetTrigger(Death);
            if (_rb != null) _rb.simulated = false;
        }

        private void Update()
        {
            if (_rb == null || !_rb.simulated) return;
            UpdateAnimator();
        }

        private void UpdateAnimator()
        {
            var velocityX = _rb != null ? _rb.linearVelocity.x : 0f;
            if (_animator == null) return;

            switch (velocityX)
            {
                case 0:
                    _animator.SetBool(IsMovingRight, false); 
                    _animator.SetBool(IsMovingLeft, false);
                    break;
                case > 0:
                    _animator.SetBool(IsMovingRight, true); 
                    _animator.SetBool(IsMovingLeft, false);
                    break;
                default:
                    _animator.SetBool(IsMovingRight, false); 
                    _animator.SetBool(IsMovingLeft, true);
                    break;
            }
        }
        
        private void HitAnimation(int dummy)
        {
            StartCoroutine(Blink());
        }
        
        private IEnumerator Blink()
        {
            var endTime = Time.time + 1f;
            while (Time.time < endTime)
            {
                if (_spriteRenderer != null)  _spriteRenderer.enabled = !_spriteRenderer.enabled;
                if (light != null) light.SetActive(!light.activeSelf);
                yield return new WaitForSeconds(0.1f);
            }
            if (_spriteRenderer != null) _spriteRenderer.enabled = true;
            if (light != null) light.SetActive(true);
        }
    }
}

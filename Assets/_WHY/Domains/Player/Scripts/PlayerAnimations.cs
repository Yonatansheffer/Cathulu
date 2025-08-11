using _WHY.Domains.Utilities.GameHandlers.Scripts;
using UnityEngine;

namespace _WHY.Domains.Player.Scripts
{
    public class PlayerAnimations : MonoBehaviour
    {
        private static readonly int IsMovingRight = Animator.StringToHash("IsMovingRight");
        private static readonly int IsMovingLeft = Animator.StringToHash("IsMovingLeft");
        private static readonly int Death = Animator.StringToHash("death");
        [SerializeField, Tooltip("Capsule GameObject for player visuals")] private GameObject capsule;
        [SerializeField, Tooltip("Gun GameObject for player weapon")] private GameObject gun;
        private Animator _animator;
        private Rigidbody2D _rb;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _rb = GetComponent<Rigidbody2D>();
        }

        private void OnEnable()
        {
            GameEvents.PlayerDefeated += DeathAnimation;
        }

        private void OnDisable()
        {
            GameEvents.PlayerDefeated -= DeathAnimation;
        }

        private void DeathAnimation()
        {
            if (capsule != null) capsule.SetActive(false);
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
            if (velocityX == 0) { _animator.SetBool(IsMovingRight, false); _animator.SetBool(IsMovingLeft, false); }
            else if (velocityX > 0) { _animator.SetBool(IsMovingRight, true); _animator.SetBool(IsMovingLeft, false); }
            else { _animator.SetBool(IsMovingRight, false); _animator.SetBool(IsMovingLeft, true); }
        }
    }
}
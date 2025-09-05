using System.Collections;
using _WHY.Domains.Utilities.GameHandlers.Scripts;
using UnityEngine;

namespace _WHY.Domains.Player.Scripts
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField, Tooltip("Starting position of the player")] private Vector3 startingPosition;
        [SerializeField, Tooltip("Movement speed")] private float speed; 
        [SerializeField, Tooltip("Gun GameObject for shooting")] private GameObject gun;
        [SerializeField, Tooltip("Force applied for jumping")] private float jumpForce;
        
        [Header("Dashing")]
        [SerializeField, Tooltip("Speed during dash")] private float dashSpeed;
        [SerializeField, Tooltip("Duration of dash in seconds")] private float dashDuration;
        [SerializeField, Tooltip("Cooldown between dashes in seconds")] private float dashCooldown;
        
        [Header("Ground Check")]
        [SerializeField, Tooltip("Transform for ground check position")] private Transform groundCheck;
        [SerializeField, Tooltip("Radius for ground check overlap circle")] private float groundCheckRadius;
        [SerializeField, Tooltip("Layer mask for ground detection")] private LayerMask groundLayer;
        
        private Rigidbody2D _rb;
        private PlayerInputs _inputActions;
        private Vector2 _moveInput;
        private bool _isDashing;
        private float _lastDashTime = -1f;
        private bool _isGrounded;
        
        private void Awake()
        {
            _isDashing = false;
            _rb = GetComponent<Rigidbody2D>();
            _inputActions = new PlayerInputs();
            InitializeInputCallbacks();
        }

        private void OnEnable()
        {
            _inputActions?.Enable();
        }

        private void OnDisable()
        {
            _inputActions?.Disable();
        }

        private void Update()
        {
            if (!_rb.simulated) return;
            CheckGrounded();
            HandleMovement();
        }

        private void InitializeInputCallbacks()
        {
            _inputActions.Movement.Move.performed += ctx => OnMovePerformed(ctx.ReadValue<Vector2>());
            _inputActions.Movement.Move.canceled += _ => _moveInput = Vector2.zero;
            _inputActions.Movement.Shoot.performed += _ => Shoot();
            _inputActions.Movement.Dash.performed += _ => Dash();
            _inputActions.Movement.Jump.performed += _ => Jump();
        }

        private void OnMovePerformed(Vector2 input)
        {
            _moveInput = input;
        }
        
        private void Shoot()
        {
            if (!_rb.simulated)
                return;
            GameEvents.Shoot?.Invoke(gun.transform);
        }
        
        private void CheckGrounded()
        {
            _isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }

        private void Jump()
        {
            if (!_isGrounded || !_rb.simulated) return; 
            _rb.linearVelocity = new Vector2(_rb.linearVelocityX, jumpForce);
        }
        
        private void Dash()
        {
            if (!_rb.simulated || _isDashing || _rb.linearVelocityX == 0 || Time.time - _lastDashTime < dashCooldown)
                return; 
            _lastDashTime = Time.time;  
            StartCoroutine(PerformDash(new Vector2(_rb.linearVelocityX, 0).normalized));
        }

        private IEnumerator PerformDash(Vector2 direction)
        {
            _isDashing = true;
            _rb.linearVelocity = direction * dashSpeed;
            yield return new WaitForSeconds(dashDuration);
            _isDashing = false;
        }
        
        private void HandleMovement()
        {
            if (_isDashing) return;
            var x = _rb.linearVelocityX;
            _rb.linearVelocity = new Vector2(_moveInput.x * speed, _rb.linearVelocityY);
        }
    }
}
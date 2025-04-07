using System.Collections;
using GameHandlers;
using UnityEngine;

namespace MainPlayer
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private Vector3 startingPosition; // Starting position of the player
        //[SerializeField] private float leftBound = -8f;
        //[SerializeField] private float rightBound = 8f;
        [SerializeField] private float speed = 5f;
        [SerializeField] private float dashSpeed = 8f;
        [SerializeField] private float dashDuration = 0.1f; // Duration of the dash
        [SerializeField] private float dashCooldown = 1f;  // Cooldown between dashes
        private Rigidbody2D _rb;
        private PlayerInputs _inputActions;
        private Vector2 _moveInput;
        private bool _isRight; // To determine the direction of the player
        private bool _isDashing; // Flag to know whether dashing
        private float _lastDashTime = -1f; // For dash cooldown
        [SerializeField] private GameObject gun;
        [SerializeField] private float jumpForce = 12f;
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundCheckRadius = 0.2f;
        [SerializeField] private LayerMask groundLayer;
        private bool _isGrounded;


        private void Awake()
        {
            _isRight = false;
            _isDashing = false;
            _rb = GetComponent<Rigidbody2D>();
            _inputActions = new PlayerInputs();
            InitializeInputCallbacks();
        }

        private void OnEnable()
        {
            _inputActions.Enable();
        }

        private void OnDisable()
        {
            _inputActions.Disable();
        }

        private void Update()
        {
            if (!_rb.simulated) 
                return;
            CheckGrounded();
            HandleMovement();
            //KeepInBounds();
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

        
        /*private void KeepInBounds()
        {
            if (transform.position.x > rightBound)
            {
                transform.position = new Vector3(rightBound, transform.position.y, transform.position.z);
            }
            else if (transform.position.x < leftBound)
            {
                transform.position = new Vector3(leftBound, transform.position.y, transform.position.z);
            }
        }*/
        
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
            if (!_isDashing)
            {
                var x = _rb.linearVelocityX;
                _rb.linearVelocity = new Vector2(_moveInput.x * speed, _rb.linearVelocityY);
                if (x != 0 && _rb.linearVelocityX != 0)
                {
                    if (x >= 0 && _rb.linearVelocityX >= 0)
                    {
                        _isRight = true;
                    }
                    else
                    {
                        _isRight = false;
                    }
                }
            }
        }
    }
}

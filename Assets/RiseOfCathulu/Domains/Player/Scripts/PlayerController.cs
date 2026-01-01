using System.Collections;
using RiseOfCathulu.Domains.Utilities.GameHandlers.Scripts;
using UnityEngine;
using UnityEngine.Serialization;


namespace RiseOfCathulu.Domains.Player.Scripts
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField, Tooltip("Starting position of the player")] private Vector3 startingPosition;
        [SerializeField, Tooltip("Gun GameObject for shooting")] private GameObject gun;
        
        [FormerlySerializedAs("dashSpeed")]
        [Header("Dashing")]
        [SerializeField, Tooltip("Speed during dash")] private float boostSpeed;

        [SerializeField, Tooltip("Duration of dash in seconds")] private float dashDuration;
        [SerializeField, Tooltip("Cooldown between dashes in seconds")] private float dashCooldown;
        
        [Header("Ground Check")]
        [SerializeField, Tooltip("Transform for ground check position")] private Transform groundCheck;
        [SerializeField, Tooltip("Radius for ground check overlap circle")] private float groundCheckRadius;
        [SerializeField, Tooltip("Layer mask for ground detection")] private LayerMask groundLayer;
        
        private Rigidbody2D _rb;
        private SpriteRenderer _sr;
        private PlayerInputs _inputActions;
        private bool _isDashing;
        private bool _isShooting;
        private float _lastDashTime = -1f;
        private bool _isGrounded;
        private PlayerGravityMotor _motor;
        private Vector2 _steeringInput;
        private float _thrustInput;
        private PlayerDualSenseFeedback _dualSenseFeedback;
        
        private void Awake()
        {
            _isDashing = false;
            _isShooting = false;
            _rb = GetComponent<Rigidbody2D>();
            _sr = GetComponent<SpriteRenderer>();
            _inputActions = new PlayerInputs();
            _dualSenseFeedback = GetComponent<PlayerDualSenseFeedback>();
            InitializeInputCallbacks();
            _motor = GetComponent<PlayerGravityMotor>();
        }

        private void OnEnable()
        {
            _inputActions?.Enable();
            GameEvents.OnEnteredGravityZone += EnterGravity;
            GameEvents.OnExitedGravityZone += ExitGravity;
        }

        private void OnDisable()
        {
            _inputActions?.Disable();
            GameEvents.OnEnteredGravityZone -= EnterGravity;
            GameEvents.OnExitedGravityZone -= ExitGravity;
        }

        private void EnterGravity(Vector2 center, float strength, float maxSpeed, float grip)
        {
            _motor.EnterGravity(center, strength, maxSpeed, grip);
        }

        private void ExitGravity()
        {
            _motor.ExitGravity();
        }


        private void Update()
        {
            if (!_rb.simulated) return;
            CheckGrounded();
            if (_isShooting)
                Shoot();
        }
        
        private void FixedUpdate()
        {
            if (!_rb.simulated) return;
            _motor.Tick(_steeringInput, _thrustInput);
            Flip();
        }

        private void Flip()
        {
            if (_steeringInput.x <= -0.001f)
            {
                _sr.flipX = false;
            }
            else if (_steeringInput.x >= 0.001f)
            {
                _sr.flipX = true;
            }
        }
        
        private void LateUpdate()
        {
            if (!_rb.simulated) return;
            RotateToFacingDirection();
        }   

        
        private void RotateToFacingDirection()
        {
            Vector2 dir = _motor.FacingDirection;
            if (dir.sqrMagnitude < 0.001f)
                return;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 91f;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        private void InitializeInputCallbacks()
        {
            _inputActions.Movement.Steer.performed += ctx => _steeringInput = ctx.ReadValue<Vector2>();
            _inputActions.Movement.Steer.canceled += _ => _steeringInput = Vector2.zero;
            _inputActions.Movement.Dash.performed += _ => Dash();
            
            _inputActions.Movement.Shoot.performed += _ => _isShooting = true;
            _inputActions.Movement.Shoot.canceled += _ => _isShooting = false;
            
            _inputActions.Movement.Move.performed += ctx => { 
                _thrustInput = ctx.ReadValue<float>();
                _dualSenseFeedback?.SetThrustInput(_thrustInput); };
            _inputActions.Movement.Move.canceled += _ => { _thrustInput = 0f;
                _dualSenseFeedback?.SetThrustInput(0f); };

            
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
            _dualSenseFeedback?.SetGrounded(_isGrounded);
        }
        
        private void Dash()
        {
            if (!_rb.simulated || _isDashing || _steeringInput.sqrMagnitude < 0.01f ||
                Time.time - _lastDashTime < dashCooldown)
                return;
            _lastDashTime = Time.time;
            StartCoroutine(PerformDash(_steeringInput.normalized));
        }

        private IEnumerator PerformDash(Vector2 direction)
        {
            _isDashing = true;
            _motor.SuspendMovement(true);
            _rb.linearVelocity += direction * boostSpeed;
            yield return new WaitForSeconds(dashDuration);
            _motor.SuspendMovement(false);
            _isDashing = false;
        }

    }
}
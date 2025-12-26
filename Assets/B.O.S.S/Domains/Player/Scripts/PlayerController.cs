using System.Collections;
using B.O.S.S.Domains.Utilities.GameHandlers.Scripts;
using UnityEngine;
#if UNITY_STANDALONE_WIN
using DualSenseUnity;
#endif


namespace B.O.S.S.Domains.Player.Scripts
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField, Tooltip("Starting position of the player")] private Vector3 startingPosition;
        [SerializeField, Tooltip("Gun GameObject for shooting")] private GameObject gun;
        
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
        private PlayerGravityMotor _motor;

        
        #if UNITY_STANDALONE_WIN
                private DualSenseController _dualSense;
                private ControllerOutputState _outputState;
        #endif

        
        private void Awake()
        {
            _isDashing = false;
            _rb = GetComponent<Rigidbody2D>();
            _inputActions = new PlayerInputs();
            InitializeInputCallbacks();
            #if UNITY_STANDALONE_WIN
                        var controllers = DualSense.GetControllers();
                        if (controllers.Count > 0)
                        {
                            _dualSense = controllers[0];
                            _outputState = new ControllerOutputState();
                        }
            #endif
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

        void EnterGravity(Vector2 center, float strength, float maxSpeed, float grip)
        {
            _motor.EnterGravity(center, strength, maxSpeed, grip);
        }

        void ExitGravity()
        {
            _motor.ExitGravity();
        }


        private void Update()
        {
            if (!_rb.simulated) return;
            CheckGrounded();
        }
        
        private void FixedUpdate()
        {
            if (!_rb.simulated) return;
            _motor.Tick(_moveInput);
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
            _inputActions.Movement.Move.performed += ctx => OnMovePerformed(ctx.ReadValue<Vector2>());
            _inputActions.Movement.Move.canceled += _ => _moveInput = Vector2.zero;
            _inputActions.Movement.Shoot.performed += _ => Shoot();
            _inputActions.Movement.Dash.performed += _ => Dash();
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
        
        
        private void Dash()
        {
            if (!_rb.simulated || _isDashing || _moveInput.sqrMagnitude < 0.01f ||
                Time.time - _lastDashTime < dashCooldown)
                return;
            _lastDashTime = Time.time;
            StartCoroutine(PerformDash(_moveInput.normalized));
        }

        private IEnumerator PerformDash(Vector2 direction)
        {
            _isDashing = true;
            _motor.SuspendMovement(true);
            _rb.linearVelocity += direction * dashSpeed;
            yield return new WaitForSeconds(dashDuration);
            _motor.SuspendMovement(false);
            _isDashing = false;
        }
    }
}
using RiseOfCathulu.Domains.Utilities.GameHandlers.Scripts;
using RiseOfCathulu.Domains.Utilities.Player_Input.DualSense_For_Unity.Scripts;
using UnityEngine;

namespace RiseOfCathulu.Domains.Player.Scripts
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerDualSenseFeedback : MonoBehaviour
    {
#if UNITY_STANDALONE_WIN
        private DualSenseController _dualSense;
        private ControllerOutputState _outputState;
#endif
        
        [Header("Gun Trigger Wall")]
        [SerializeField] private float gunWallStart = 0.25f;
        [SerializeField] private float gunWallEnd = 0.6f;
        [SerializeField] private float gunWallForce = 1.0f;
        [SerializeField] private float gunWallDuration = 0.12f;


        [Header("Trigger Tuning")]
        [SerializeField] private float thrustStartPosition = 0.15f;
        [SerializeField] private float maxTriggerForce = 0.85f;
        [SerializeField] private float speedForceWeight = 0.4f;
        [Header("Gravity Trigger Resistance")]
        [SerializeField] private float spaceResistanceMultiplier = 0.6f;
        [SerializeField] private float gravityResistanceMultiplier = 1.6f;
        [SerializeField] private float gravityResistanceSpeedBias = 0.7f;

        
        [Header("Rumble")]
        [SerializeField] private float gunRumbleStrength = 0.5f;
        [SerializeField] private float gunRumbleDuration = 0.1f;
        [SerializeField] private float gravityRumbleMax = 0.25f;
        [SerializeField] private float gravityRumbleFadeSpeed = 3f;
        [SerializeField] private float growRumbleStrength = 0.6f;
        [SerializeField] private float growRumbleDuration = 0.18f;
        [SerializeField] private float shrinkRumbleStrength = 0.4f;
        [SerializeField] private float shrinkRumbleDuration = 0.12f;
        [SerializeField] private float gravityEnterStrength = 0.45f;
        [SerializeField] private float gravityEnterDuration = 0.15f;
        [SerializeField] private float speedFactor = 400f;
        [SerializeField] private float speedSmoothing = 6f;

        [SerializeField] private float slingshotSpeedThreshold = 18f;
        [SerializeField] private float slingshotRumbleStrength = 0.55f;
        [SerializeField] private float slingshotRumbleDuration = 0.12f;


        private float _smoothedSpeed;
        private float _gravityEventTimer;
        private float _gravityEventStrength;
        private float _gunRumbleTimer;
        private float _currentGravityRumble;
        private float _sizeRumbleTimer;
        private float _sizeRumbleStrength;
        private bool _sizeRumbleLeft;
        private bool _gunWallActive;
        private float _gunWallTimer;
        private Rigidbody2D _rb;
        private float _currentThrust;
        private bool _inGravity;
        private bool _isGrounded;


        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
#if UNITY_STANDALONE_WIN
            var controllers = DualSense.GetControllers();
            if (controllers.Count > 0)
            {
                _dualSense = controllers[0];
                _outputState = new ControllerOutputState();
            }
#endif
        }

        private void OnEnable()
        {
            GameEvents.OnEnteredGravityZone += OnEnterGravity;
            GameEvents.OnExitedGravityZone += OnExitGravity;
        }

        private void OnDisable()
        {
            GameEvents.OnEnteredGravityZone -= OnEnterGravity;
            GameEvents.OnExitedGravityZone -= OnExitGravity;

#if UNITY_STANDALONE_WIN
            ResetTriggers();
#endif
        }


        public void TriggerGunWall()
        {
#if UNITY_STANDALONE_WIN
            _gunWallActive = true;
            _gunWallTimer = gunWallDuration;
            _gunRumbleTimer = gunRumbleDuration;
#endif
        }

        public void SetThrustInput(float thrust)
        {
            _currentThrust = thrust;
        }

        public void SetGrounded(bool grounded)
        {
            _isGrounded = grounded;
        }

        public void TriggerSizeChangeRumble(int delta)
        {
#if UNITY_STANDALONE_WIN
            if (delta == 0) return;

            if (delta > 0)
            {
                _sizeRumbleStrength = growRumbleStrength;
                _sizeRumbleTimer = growRumbleDuration;
                _sizeRumbleLeft = true;   // heavy motor
            }
            else
            {
                _sizeRumbleStrength = shrinkRumbleStrength;
                _sizeRumbleTimer = shrinkRumbleDuration;
                _sizeRumbleLeft = false;  // sharp motor
            }
#endif
        }

        
        private void FixedUpdate()
        {
#if UNITY_STANDALONE_WIN
            if (_dualSense == null)
                return;
            UpdateRumble();

            if (_gunWallActive)
            {
                UpdateGunWall();
                return;
            }
            UpdateThrustTrigger();
#endif
        }

#if UNITY_STANDALONE_WIN
        private void UpdateThrustTrigger()
        {
            if (_currentThrust < 0.05f)
            {
                _outputState.LeftTriggerEffect.InitializeNoResistanceEffect();
                _dualSense.SetOutputState(_outputState);
                return;
            }

            float speed01 = Mathf.Clamp01(_rb.linearVelocity.magnitude / speedFactor);
            float baseForce =
                (_currentThrust * (1f - speedForceWeight)) +
                (speed01 * speedForceWeight);

// How much gravity is "felt" (slow = heavy)
            float gravityWeight = 0f;

            if (_inGravity && !_isGrounded)
            {
                gravityWeight = Mathf.Lerp(
                    1f,
                    0f,
                    speed01 * gravityResistanceSpeedBias
                );
            }

// Blend between space and gravity resistance
            float resistanceMultiplier = Mathf.Lerp(
                spaceResistanceMultiplier,
                gravityResistanceMultiplier,
                gravityWeight
            );

            float force = baseForce * resistanceMultiplier;
            force = Mathf.Clamp(force, 0f, maxTriggerForce);

            _outputState.LeftTriggerEffect.InitializeContinuousResistanceEffect(
                thrustStartPosition,
                force
            );

            _dualSense.SetOutputState(_outputState);
        }

        private void ResetTriggers()
        {
            _outputState.LeftTriggerEffect.InitializeNoResistanceEffect();
            _outputState.RightTriggerEffect.InitializeNoResistanceEffect();
            _dualSense.SetOutputState(_outputState);
        }
#endif
        private void OnEnterGravity(Vector2 center, float strength, float maxSpeed, float grip)
        {
            _inGravity = true;

#if UNITY_STANDALONE_WIN
            _gravityEventStrength = gravityEnterStrength;
            _gravityEventTimer = gravityEnterDuration;
#endif
        }

        private void OnExitGravity()
        {
            _inGravity = false;

#if UNITY_STANDALONE_WIN
            if (_isGrounded)
            {
                _gravityEventTimer = 0f;
                return;
            }

            float exitSpeed = _rb.linearVelocity.magnitude;

            if (exitSpeed >= slingshotSpeedThreshold)
            {
                _gravityEventStrength = slingshotRumbleStrength;
                _gravityEventTimer = slingshotRumbleDuration;
            }
            else
            {
                _gravityEventTimer = 0f;
            }
#endif
        }



#if UNITY_STANDALONE_WIN
        private void UpdateGunWall()
        {
            
            _outputState.RightTriggerEffect.InitializeSectionResistanceEffect(
                gunWallStart,
                gunWallEnd,
                gunWallForce
            );

            _dualSense.SetOutputState(_outputState);

            _gunWallTimer -= Time.fixedDeltaTime;
            if (_gunWallTimer <= 0f)
            {
                _gunWallActive = false;
            }
        }
#endif
        
        
#if UNITY_STANDALONE_WIN
        private void UpdateRumble()
        {
            if (_dualSense == null)
                return;

            // --- Gravity enter / slingshot event (airborne only) ---
            if (_gravityEventTimer > 0f && !_isGrounded)
            {
                _outputState.LeftRumbleIntensity = _gravityEventStrength;
                _outputState.RightRumbleIntensity = 0f;

                _gravityEventTimer -= Time.fixedDeltaTime;
                _dualSense.SetOutputState(_outputState);
                return;
            }


            // --- Size change rumble ---
            if (_sizeRumbleTimer > 0f)
            {
                _outputState.LeftRumbleIntensity  = _sizeRumbleLeft ? _sizeRumbleStrength : 0f;
                _outputState.RightRumbleIntensity = !_sizeRumbleLeft ? _sizeRumbleStrength : 0f;

                _sizeRumbleTimer -= Time.fixedDeltaTime;
                _dualSense.SetOutputState(_outputState);
                return;
            }

            // --- Gun recoil ---
            if (_gunRumbleTimer > 0f)
            {
                _outputState.RightRumbleIntensity = gunRumbleStrength;
                _outputState.LeftRumbleIntensity = 0f;

                _gunRumbleTimer -= Time.fixedDeltaTime;
                _dualSense.SetOutputState(_outputState);
                return;
            }

// --- Gravity rumble (speed-based, suppressed when grounded) ---
            if (_inGravity && !_isGrounded)
            {
                float rawSpeed = _rb.linearVelocity.magnitude;

                _smoothedSpeed = Mathf.Lerp(
                    _smoothedSpeed,
                    rawSpeed,
                    speedSmoothing * Time.fixedDeltaTime
                );

                float speed01 = Mathf.Clamp01(_smoothedSpeed / speedFactor);
                float target = gravityRumbleMax * (1f - speed01);

                _currentGravityRumble = Mathf.MoveTowards(
                    _currentGravityRumble,
                    target,
                    gravityRumbleFadeSpeed * Time.fixedDeltaTime
                );
            }
            else
            {
                _currentGravityRumble = Mathf.MoveTowards(
                    _currentGravityRumble,
                    0f,
                    gravityRumbleFadeSpeed * Time.fixedDeltaTime
                );
            }

            _outputState.LeftRumbleIntensity = _currentGravityRumble;
            _outputState.RightRumbleIntensity = 0f;
            _dualSense.SetOutputState(_outputState);
        }
#endif

    }
}
git 
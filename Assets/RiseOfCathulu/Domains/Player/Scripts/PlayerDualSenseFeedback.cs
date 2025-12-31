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
        [SerializeField] private float gravityForceMultiplier = 1.3f;
        
        [Header("Rumble - Gun Recoil")]
        [SerializeField] private float gunRumbleStrength = 0.5f;
        [SerializeField] private float gunRumbleDuration = 0.1f;

        [Header("Rumble - Gravity")]
        [SerializeField] private float gravityRumbleMax = 0.25f;
        [SerializeField] private float gravityRumbleFadeSpeed = 3f;

        
        private float _gunRumbleTimer;
        private float _currentGravityRumble;

        
        private bool _gunWallActive;
        private float _gunWallTimer;
        private Rigidbody2D _rb;
        private PlayerGravityMotor _motor;
        private float _currentThrust;
        private bool _inGravity;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _motor = GetComponent<PlayerGravityMotor>();

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
        
        private void FixedUpdate()
        {
#if UNITY_STANDALONE_WIN
            if (_gunWallActive)
            {
                UpdateGunWall();
                return;
            }
            UpdateRumble();

            if (_dualSense == null)
                return;

            UpdateThrustTrigger();
#endif
        }

#if UNITY_STANDALONE_WIN
        private void UpdateThrustTrigger()
        {
            if (_currentThrust < 0.05f)
            {
                _outputState.RightTriggerEffect.InitializeNoResistanceEffect();
                _dualSense.SetOutputState(_outputState);
                return;
            }

            float speed01 = Mathf.Clamp01(_rb.linearVelocity.magnitude / 40f);
            float force =
                (_currentThrust * (1f - speedForceWeight)) +
                (speed01 * speedForceWeight);

            if (_inGravity)
                force *= gravityForceMultiplier;

            force = Mathf.Clamp(force, 0f, maxTriggerForce);

            _outputState.RightTriggerEffect.InitializeContinuousResistanceEffect(
                thrustStartPosition,
                force
            );

            _dualSense.SetOutputState(_outputState);
        }

        private void ResetTriggers()
        {
            _outputState.RightTriggerEffect.InitializeNoResistanceEffect();
            _outputState.LeftTriggerEffect.InitializeNoResistanceEffect();
            _dualSense.SetOutputState(_outputState);
        }
#endif
        

        private void OnEnterGravity(Vector2 center, float strength, float maxSpeed, float grip)
        {
            _inGravity = true;
        }

        private void OnExitGravity()
        {
            _inGravity = false;
        }
        
#if UNITY_STANDALONE_WIN
        private void UpdateGunWall()
        {
            _outputState.LeftTriggerEffect.InitializeSectionResistanceEffect(
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
            // --- Gun recoil (priority) ---
            if (_gunRumbleTimer > 0f)
            {
                _outputState.RightRumbleIntensity = gunRumbleStrength;
                _outputState.LeftRumbleIntensity = 0f;

                _gunRumbleTimer -= Time.fixedDeltaTime;
                _dualSense.SetOutputState(_outputState);
                return;
            }

            // --- Gravity rumble ---
            float targetGravityRumble = _inGravity ? gravityRumbleMax : 0f;

            _currentGravityRumble = Mathf.MoveTowards(
                _currentGravityRumble,
                targetGravityRumble,
                gravityRumbleFadeSpeed * Time.fixedDeltaTime
            );

            _outputState.LeftRumbleIntensity = _currentGravityRumble;
            _outputState.RightRumbleIntensity = 0f;

            _dualSense.SetOutputState(_outputState);
        }
#endif


    }
}

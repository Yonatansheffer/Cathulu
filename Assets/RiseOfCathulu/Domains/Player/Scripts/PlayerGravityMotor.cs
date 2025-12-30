using UnityEngine;

namespace RiseOfCathulu.Domains.Player.Scripts
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerGravityMotor : MonoBehaviour
    {
        [Header("Cruising Movement")]
        [SerializeField, Tooltip("Target cruising speed when input is held")]
        private float speed = 10f;
        [SerializeField, Tooltip("Target drifting speed when no input is held")]
        private float idleSpeed = 1.2f;
        [SerializeField, Tooltip("How fast velocity converges to its target")]
        private float convergenceRate = 4f;
        [SerializeField, Tooltip("Absolute hard cap on velocity magnitude")]
        private float absoluteMaxSpeed = 40f;
        private bool _wasThrusting;


        [Header("Gravity Defaults")]
        private float _defaultInwardGravity = 30f;
        private float _defaultMaxVortexSpeed = 25f;
        private float _defaultVortexGrip = 2f;
        private Rigidbody2D _rb;   
        private bool _isInGravityZone;
        private Vector2 _gravityCenter;
        private float _inwardGravity;
        private float _maxVortexSpeed;
        private float _vortexGrip;
        private bool _suspendMovement;
        private Vector2 _lastMoveDir;
        private float _gravityFade = 0f;
        
        [Header("Acceleration")]
        [SerializeField] private float acceleration = 18f;
        [Header("Trigger Curves")]
        [SerializeField]
        private AnimationCurve accelerationTriggerCurve =
            new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.15f, 1f),
                new Keyframe(1f, 1f)
            );

        [SerializeField]
        private AnimationCurve brakeTriggerCurve =
            new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.4f, 1f),
                new Keyframe(1f, 1f)
            );



        
        private float _gravityMultiplier = 1f;
        private float _steeringMultiplier = 1f;
        [SerializeField] private float orbitBiasStrength = 1.2f;
        [SerializeField] private float slingshotBonus = 2.5f;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            ResetGravityToDefaults();
        }

        private void FixedUpdate()  
        {
            Debug.Log($"Current Speed: {_rb.linearVelocity.magnitude:F2} | In Gravity: {_isInGravityZone} ");
        }
        
        public Vector2 FacingDirection
        {
            get
            {
                if (_lastMoveDir.sqrMagnitude < 0.001f)
                    return Vector2.up;

                return _lastMoveDir;
            }
        }

        public void Tick(Vector2 steering, float thrust, float brake)
        {
            if (_suspendMovement) return;
            ApplyCruising(steering, thrust, brake);
            ApplyGravity();
            ClampAbsoluteSpeed();

        }

        public void EnterGravity(Vector2 gravityCenter, float inwardGravity, float maxVortexSpeed, float vortexGrip)
        {
            _gravityCenter = gravityCenter;
            _inwardGravity = inwardGravity;
            _maxVortexSpeed = maxVortexSpeed;
            _vortexGrip = vortexGrip;
            _gravityFade = 1f;

            _isInGravityZone = true;
        }
        public void ExitGravity()
        {
            _isInGravityZone = false;
            _gravityFade = 0f;

            _rb.linearVelocity += _rb.linearVelocity.normalized * slingshotBonus;
            ResetGravityToDefaults();
        }



        public void SuspendMovement(bool suspend)
        {
            _suspendMovement = suspend;
        }
        
        public void SetDiveState(bool diving)
        {
            _gravityMultiplier = diving ? 2.5f : 1f;
            _steeringMultiplier = diving ? 0.35f : 1f;
        }

        private float ApplyTriggerCurve(float value, float deadZone, AnimationCurve curve)
        {
            if (value < deadZone)
                return 0f;

            float normalized =
                Mathf.InverseLerp(deadZone, 1f, value);

            return curve.Evaluate(normalized);
        }
        
        private void ApplyCruising(Vector2 moveInput, float thrust, float brake)
        {
            float accelInput = ApplyTriggerCurve(thrust, 0.05f, accelerationTriggerCurve);
            float brakeInput = ApplyTriggerCurve(brake, 0.05f, brakeTriggerCurve);

            Vector2 vel = _rb.linearVelocity;
            float speedNow = vel.magnitude;

            // -----------------------------
            // BRAKE (same as before)
            // -----------------------------
            if (brakeInput > 0f && speedNow > 0.1f)
            {
                float gravityProximity = 0f;

                if (_isInGravityZone)
                {
                    Vector2 toCenter = _gravityCenter - _rb.position;
                    gravityProximity = Mathf.Clamp01(1f / (toCenter.magnitude + 1f));
                }

                float brakeTarget = Mathf.Lerp(0.55f, 0.4f, gravityProximity);

                vel = Vector2.Lerp(
                    vel,
                    vel * brakeTarget,
                    brakeInput * Time.fixedDeltaTime * 6f
                );
            }

            // -----------------------------
            // DIRECTION + SPEED CONTROL
            // -----------------------------
            if (moveInput.sqrMagnitude > 0.01f)
            {
                Vector2 desiredDir = moveInput.normalized;
                _lastMoveDir = desiredDir;

                float targetSpeed =
                    Mathf.Lerp(idleSpeed, speed, accelInput);

                Vector2 targetVelocity = desiredDir * targetSpeed;

                vel = Vector2.Lerp(
                    vel,
                    targetVelocity,
                    convergenceRate * Time.fixedDeltaTime
                );
            }
            else
            {
                // No input → gentle drift
                if (speedNow > idleSpeed)
                {
                    vel = Vector2.Lerp(
                        vel,
                        vel.normalized * idleSpeed,
                        convergenceRate * Time.fixedDeltaTime
                    );
                }
            }

            _rb.linearVelocity = vel;
        }


        private void ApplyGravity()
        {
            if (!_isInGravityZone)
                return;

            Vector2 vel = _rb.linearVelocity;

            // Direction & distance to gravity center
            Vector2 toCenter = _gravityCenter - _rb.position;
            float distance = Mathf.Max(0.1f, toCenter.magnitude);
            Vector2 radialDir = toCenter.normalized;

            // Smooth gravity engagement
            _gravityFade = Mathf.MoveTowards(
                _gravityFade,
                1f,
                4f * Time.fixedDeltaTime
            );

            // ----------------------------
            // RADIAL GRAVITY (INEVITABLE)
            // ----------------------------
            float gravityForce =
                (_inwardGravity * _gravityMultiplier * _gravityFade) /
                (distance + 2f);

            vel += radialDir * gravityForce * Time.fixedDeltaTime;

            // ----------------------------
            // TANGENTIAL ORBIT (FLOW)
            // ----------------------------
            Vector2 tangentDir = new Vector2(-radialDir.y, radialDir.x);

            // Ensure orbit follows current motion
            if (Vector2.Dot(vel, tangentDir) < 0f)
                tangentDir = -tangentDir;

            Vector2 orbitTarget = tangentDir * _maxVortexSpeed;

            // Orbit dominates farther from center
            float orbitInfluence = 1f / (distance + 1f);

            vel += (orbitTarget - vel) * (_vortexGrip * orbitBiasStrength * orbitInfluence * Time.fixedDeltaTime);

            _rb.linearVelocity = vel;
        }


        private void ClampAbsoluteSpeed()
        {
            float speedNow = _rb.linearVelocity.magnitude;
            if (speedNow > absoluteMaxSpeed)
            {
                _rb.linearVelocity =
                    _rb.linearVelocity.normalized * absoluteMaxSpeed;
            }
        }

        private void ResetGravityToDefaults()
        {
            _inwardGravity = _defaultInwardGravity;
            _maxVortexSpeed = _defaultMaxVortexSpeed;
            _vortexGrip = _defaultVortexGrip;
        }
    }
}

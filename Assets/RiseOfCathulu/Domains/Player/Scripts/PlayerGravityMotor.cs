using UnityEngine;
using UnityEngine.Serialization;

namespace RiseOfCathulu.Domains.Player.Scripts
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerGravityMotor : MonoBehaviour
    {
        [Header("Cruising Movement")]
        [SerializeField, Tooltip("Target cruising speed when input is held")] private float speed = 10f;
        [SerializeField, Tooltip("Target drifting speed when no input is held")] private float idleSpeed = 1.2f;
        [SerializeField, Tooltip("How fast velocity converges to its target")] private float convergenceRate = 4f;
        [SerializeField, Tooltip("Absolute hard cap on velocity magnitude")] private float absoluteMaxSpeed = 40f;
        [SerializeField] private float cruiseLockThreshold = 0.95f;
        [SerializeField] private float thrustReleaseThreshold = 0.15f;
        private bool _thrustHeld;
        private bool _cruiseLocked;
        private float _maximumSpeed;

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
        private float _gravityFade;
        private bool _isInDestructableGravityZone;
        

        [Header("Trigger Curves")]
        [SerializeField] private AnimationCurve accelerationTriggerCurve = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.15f, 1f),
                new Keyframe(1f, 1f)
            );
        
        private float _gravityMultiplier = 1f;
        [SerializeField] private float steeringFactor =20f;
        [SerializeField] private float orbitBiasStrength = 1.2f;
        [SerializeField] private float slingshotBonus = 2.5f;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            ResetGravityToDefaults();
        }
        

        private void FixedUpdate()  
        {
//            Debug.Log($"Current Speed: {_rb.linearVelocity.magnitude:F2} | In Gravity: {_isInGravityZone} ");
        }
        
        public void ApplySizeStats( float maxSpeed, float convergence)
        {
            _maximumSpeed = maxSpeed;
            speed = maxSpeed; 
            convergenceRate = convergence;
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

        public void Tick(Vector2 steering, float thrust)
        {
            if (_suspendMovement) return;
            ApplyCruising(steering, thrust);
            ApplyGravity();
            ClampAbsoluteSpeed();
        }

        public void EnterGravity(Vector2 gravityCenter, float inwardGravity, float maxVortexSpeed, float vortexGrip, bool isDestructable)
        {
            _isInDestructableGravityZone = isDestructable;
            _gravityCenter = gravityCenter;
            _inwardGravity = inwardGravity * absoluteMaxSpeed;
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
            _isInDestructableGravityZone = false;
            ResetGravityToDefaults();
        }

        public void SuspendMovement(bool suspend)
        {
            _suspendMovement = suspend;
        }
        

        private float ApplyTriggerCurve(float value, float deadZone, AnimationCurve curve)
        {
            if (value < deadZone)
                return 0f;
            float normalized = Mathf.InverseLerp(deadZone, 1f, value);
            return curve.Evaluate(normalized);
        }
        
        private void ApplyCruising(Vector2 steerInput, float thrust)
        {
            float accelInput = ApplyTriggerCurve(thrust, 0.05f, accelerationTriggerCurve);

            Vector2 vel = _rb.linearVelocity;
            float speedNow = vel.magnitude;

            bool pressedThisFrame = false;
            if (_thrustHeld && thrust < thrustReleaseThreshold)
            {
                _thrustHeld = false;
            }
            if (!_thrustHeld && thrust > thrustReleaseThreshold)
            {
                _thrustHeld = true;
                pressedThisFrame = true;
            }
            bool thrusting = _thrustHeld;


            // ----------------------------
            // STEERING (always allowed)
            // ----------------------------
            if (steerInput.sqrMagnitude > 0.01f)
            {
                _lastMoveDir = Vector2.Lerp(
                    _lastMoveDir,
                    steerInput.normalized,
                    steeringFactor * Time.fixedDeltaTime
                ).normalized;
            }

            // ----------------------------
            // CRUISE STATE LOGIC (NO GRAVITY)
            // ----------------------------
            if (!_isInGravityZone)
            {
                if (pressedThisFrame)
                {
                    _cruiseLocked = false;
                }
                // Accelerating toward max
                if (thrusting && !_cruiseLocked)
                {
                    float targetSpeed = Mathf.Lerp(idleSpeed, speed, accelInput);
                    Vector2 targetVelocity = _lastMoveDir * targetSpeed;

                    vel = Vector2.Lerp(
                        vel,
                        targetVelocity,
                        convergenceRate * Time.fixedDeltaTime
                    );

                    // Lock cruise when close enough to max
                    if (vel.magnitude >= speed * cruiseLockThreshold)
                    {
                        vel = vel.normalized * speed;
                        _cruiseLocked = true;
                    }
                }
                // Cruise lock: maintain inertia even with no input
                else if (_cruiseLocked)
                {
                    vel = vel.normalized * speed;
                }
                // Not thrusting and not cruising → slow drift
                else
                {
                    if (speedNow > idleSpeed)
                    {
                        vel = Vector2.Lerp(
                            vel,
                            vel.normalized * idleSpeed,
                            convergenceRate * Time.fixedDeltaTime
                        );
                    }
                }
            }
            // ----------------------------
            // IN GRAVITY → unchanged behavior
            // ----------------------------
            else
            {
                _cruiseLocked = false;

                if (thrusting)
                {
                    float targetSpeed = Mathf.Lerp(idleSpeed, speed, accelInput);
                    vel = Vector2.Lerp(
                        vel,
                        _lastMoveDir * targetSpeed,
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
            /*if (_isInDestructableGravityZone)
            {
                absoluteMaxSpeed = _maximumSpeed * 100f;
            }
            else
            {
                absoluteMaxSpeed = _maximumSpeed;
            }*/

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

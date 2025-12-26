using System;
using UnityEngine;

namespace B.O.S.S.Domains.Player.Scripts
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


        public void Tick(Vector2 input)
        {
            if (_suspendMovement) return;
            ApplyCruising(input);
            ApplyGravity();
            ClampAbsoluteSpeed();
        }

        public void EnterGravity(Vector2 gravityCenter, float inwardGravity, float maxVortexSpeed, float vortexGrip)
        {
            _gravityCenter = gravityCenter;
            _inwardGravity = inwardGravity;
            _maxVortexSpeed = maxVortexSpeed;
            _vortexGrip = vortexGrip;
            _isInGravityZone = true;
        }

        public void ExitGravity()
        {
            _isInGravityZone = false;
            ResetGravityToDefaults();
        }

        public void SuspendMovement(bool suspend)
        {
            _suspendMovement = suspend;
        }
        
        private void ApplyCruising(Vector2 input)
        {
            Vector2 vel = _rb.linearVelocity;

            // -------------------------------------------------------
            // OUTSIDE GRAVITY → INSTANT RESPONSE
            // -------------------------------------------------------
            if (!_isInGravityZone)
            {
                if (input.sqrMagnitude > 0.01f)
                {
                    // Instant movement
                    vel = input.normalized * speed;
                    _lastMoveDir = input.normalized;

                }
                else if (vel.sqrMagnitude > 0.01f)
                {   
                    // Instant idle drift
                    vel = vel.normalized * idleSpeed;
                }

                _rb.linearVelocity = vel;
                return;
            }

            // -------------------------------------------------------
            // INSIDE GRAVITY → SMOOTH CONVERGENCE
            // -------------------------------------------------------
            if (input.sqrMagnitude > 0.01f)
            {
                Vector2 desiredDir = input.normalized;
                _lastMoveDir = desiredDir;

                Vector2 targetVelocity = desiredDir * speed;
                vel = Vector2.Lerp(
                    vel,
                    targetVelocity,
                    convergenceRate * Time.fixedDeltaTime
                );
            }

            _rb.linearVelocity = vel;
        }


        private void ApplyGravity()
        {
            if (!_isInGravityZone) return;

            Vector2 vel = _rb.linearVelocity;

            Vector2 toCenter = _gravityCenter - _rb.position;
            float distance = Mathf.Max(0.1f, toCenter.magnitude);
            Vector2 radialDir = toCenter.normalized;

            // Radial gravity pull
            float gravityForce = _inwardGravity / (distance + 2f);
            vel += radialDir * gravityForce * Time.fixedDeltaTime;

            // Tangential orbit force
            Vector2 tangentDir = new Vector2(-radialDir.y, radialDir.x);
            if (Vector2.Dot(vel, tangentDir) < 0f)
                tangentDir = -tangentDir;

            Vector2 orbitTarget = tangentDir * _maxVortexSpeed;
            vel += (orbitTarget - vel) * _vortexGrip * Time.fixedDeltaTime;

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

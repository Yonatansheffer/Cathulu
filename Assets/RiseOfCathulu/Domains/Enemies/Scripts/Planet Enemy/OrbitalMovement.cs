using System;
using RiseOfCathulu.Domains.Utilities.GameHandlers.Scripts;
using UnityEngine;

namespace RiseOfCathulu.Domains.Enemies.Scripts.Planet_Enemy
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class OrbitalMovement : MonoBehaviour
    {
        [SerializeField] private Transform gravityPoint;
        [SerializeField] private float orbitRadius = 5f;
        [SerializeField] private float orbitSpeed = 6f;
        [SerializeField] private bool clockwise = true;
        
        [Header("Tilting Movement")]
        [SerializeField, Tooltip("Max tilt angle while idle")]
        private float idleTiltAngle = 15f;
        [SerializeField, Tooltip("Tilt oscillation speed while idle")]
        private float tiltSpeed = 2f;
        

        [Header("Spin Control")]
        [SerializeField] private float angularDamping = 20f; // higher = faster decay

        private Rigidbody2D _rb;
        private bool _isFrozen;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            gravityPoint = transform.parent;
        }

        private void OnEnable()
        {
            GameEvents.FreezeLevel += OnFreeze;
            GameEvents.UnFreezeLevel += OnUnFreeze;
        }
        
        private void OnDisable()
        {
            GameEvents.FreezeLevel -= OnFreeze;
            GameEvents.UnFreezeLevel -= OnUnFreeze;
        }
        
        private void OnFreeze()
        {
            _isFrozen = true;
        }
        
        private void OnUnFreeze()
        {
            _isFrozen = false;
        }

        private void FixedUpdate()
        {
            if (gravityPoint == null || _isFrozen)
                return;
            Vector2 toCenter = gravityPoint.position - transform.position;
            Vector2 radialDir = toCenter.normalized;
            HandleIdleTilt();
            _rb.position = (Vector2)gravityPoint.position - radialDir * orbitRadius;

            // 2. Tangential direction
            Vector2 tangentDir = clockwise ? new Vector2(radialDir.y, -radialDir.x)
                : new Vector2(-radialDir.y, radialDir.x);

            _rb.linearVelocity = tangentDir * orbitSpeed;
            _rb.angularVelocity = Mathf.Lerp(_rb.angularVelocity, 0f,angularDamping * Time.fixedDeltaTime);
        }
        
        
        private void HandleIdleTilt()
        {
            var angle = Mathf.Sin(Time.time * tiltSpeed) * idleTiltAngle;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }
}
using UnityEngine;

namespace B.O.S.S.Domains.Enemies.Scripts.Planet_Enemy
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

        private Rigidbody2D rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            gravityPoint = transform.parent;
        }

        private void FixedUpdate()
        {
            if (gravityPoint == null)
                return;

            Vector2 toCenter = gravityPoint.position - transform.position;
            Vector2 radialDir = toCenter.normalized;
            HandleIdleTilt();
            // 1. Lock position to orbit radius
            rb.position = (Vector2)gravityPoint.position - radialDir * orbitRadius;

            // 2. Tangential direction
            Vector2 tangentDir = clockwise
                ? new Vector2(radialDir.y, -radialDir.x)
                : new Vector2(-radialDir.y, radialDir.x);

            // 3. Constant-speed orbit
            rb.linearVelocity = tangentDir * orbitSpeed;

            // 4. FAST angular velocity decay
            rb.angularVelocity = Mathf.Lerp(
                rb.angularVelocity,
                0f,
                angularDamping * Time.fixedDeltaTime
            );
        }
        
        
        private void HandleIdleTilt()
        {
            var angle = Mathf.Sin(Time.time * tiltSpeed) * idleTiltAngle;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }
}
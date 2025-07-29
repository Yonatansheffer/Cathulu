using UnityEngine;

namespace _WHY.Scripts.Boss
{
    public class BossMovement : WHYBaseMono
    {
        [Header("Floating Movement")]
        [SerializeField] private float moveRadius = 3f;           // רדיוס בציר Y
        [SerializeField] private float moveSpeed = 1f;
        [SerializeField] private float idleTiltAngle = 15f;
        [SerializeField] private float tiltSpeed = 2f;

        [Header("X Movement Range")]
        [SerializeField] private float minX = -5f;
        [SerializeField] private float maxX = 5f;
        private Vector3 _startPosition;
        private Vector3 _targetPosition;

        private void Start()
        {
            _startPosition = transform.position;
            PickNewTargetPosition();
        }

        private void Update()
        {
            HandleFloatingMovement();
            if (BossShooting.GetBossState() != BossShooting.BossState.Idle) return;
            HandleIdleTilt();
        }

        private void HandleFloatingMovement()
        {
            transform.position = Vector3.MoveTowards(transform.position, _targetPosition, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, _targetPosition) < 0.1f)
            {
                PickNewTargetPosition();
            }
        }

        private void PickNewTargetPosition()
        {
            float randomX = Random.Range(minX, maxX);
            float randomYOffset = Random.Range(-moveRadius, moveRadius);
            float newY = _startPosition.y + randomYOffset;
            _targetPosition = new Vector3(randomX, newY, _startPosition.z);
        }

        private void HandleIdleTilt()
        {
            float angle = Mathf.Sin(Time.time * tiltSpeed) * idleTiltAngle;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }
}
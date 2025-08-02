using UnityEngine;

namespace _WHY.Scripts.Boss
{
    public class BossMovement : WHYBaseMono
    {
        [Header("Floating Movement")]
        [SerializeField] private float moveRadius = 3f; 
        [SerializeField] private float initialMoveSpeed = 1f;
        [SerializeField] private float shootingMoveSpeed = 1f;
        [SerializeField] private float idleTiltAngle = 15f;
        [SerializeField] private float tiltSpeed = 2f;

        [Header("X Movement Range")]
        [SerializeField] private float minX = -5f;
        [SerializeField] private float maxX = 5f;
        private Vector3 _startPosition;
        private Vector3 _targetPosition;
        private float _moveSpeed;

        private void Start()
        {
            _moveSpeed = initialMoveSpeed;
            _startPosition = transform.position;
            PickNewTargetPosition();
        }

        private void Update()
        {
            HandleFloatingMovement();
            if (BossShooting.GetBossState() != BossShooting.BossState.Idle)
            {
               _moveSpeed = shootingMoveSpeed; 
               return;
            }
            _moveSpeed = initialMoveSpeed;
            HandleIdleTilt();
        }

        private void HandleFloatingMovement()
        {
            transform.position = Vector3.MoveTowards(transform.position,
                _targetPosition, _moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, _targetPosition) < 0.1f)
            {
                PickNewTargetPosition();
            }
        }

        private void PickNewTargetPosition()
        {
            var randomX = Random.Range(minX, maxX);
            var randomYOffset = Random.Range(-moveRadius, moveRadius);
            var newY = _startPosition.y + randomYOffset;
            _targetPosition = new Vector3(randomX, newY, _startPosition.z);
        }

        private void HandleIdleTilt()
        {
            var angle = Mathf.Sin(Time.time * tiltSpeed) * idleTiltAngle;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }
}
using UnityEngine;

namespace _WHY.Scripts.Enemies
{
    public class CrawlingEnemy : Enemy
    {
        [SerializeField] private float moveSpeed = 2f;
        private Vector2 _moveDirection = Vector2.right;
        private Rigidbody2D _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void OnEnable()
        {
            // Set random direction when spawned (left or right)
            _moveDirection = Random.value < 0.5f ? Vector2.left : Vector2.right;
        }

        protected override void Move()
        {
            _rb.linearVelocity = _moveDirection * moveSpeed;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            FlipDirection();
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Weapon"))
            {
                CrawlingEnemyPool.Instance.Return(gameObject.GetComponent<CrawlingEnemy>());
            }
        }

        private void FlipDirection()
        {
            _moveDirection *= -1;
        }
    }
}
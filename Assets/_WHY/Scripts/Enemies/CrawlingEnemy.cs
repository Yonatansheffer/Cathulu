using System.Collections;
using UnityEngine;

namespace _WHY.Scripts.Enemies
{
    public class CrawlingEnemy : Enemy
    {
        private static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");
        [SerializeField] private float moveSpeed = 2f;
        private Vector2 _moveDirection = Vector2.right;
        private Rigidbody2D _rb;
        private bool _isMoving;
        private Animator _animator;
        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();
            _spriteRenderer = GetComponent<SpriteRenderer>(); 
        }
        
        private void Start()
        {
            StartCoroutine(WaitBeforeMoving());
        }
        
        private void Update()
        {
            UpdateAnimation();
        }
        
        private void UpdateAnimation()
        {
            _animator.SetFloat(MoveSpeed, _rb.linearVelocity.magnitude);
            _spriteRenderer.flipX = _moveDirection.x < 0f;
        }
        
        private IEnumerator WaitBeforeMoving()
        {
            yield return new WaitForSeconds(5f);
            _isMoving = true;
        }

        public override void Reset()
        {
            _isMoving = false;
        } 

        private void OnEnable()
        {
            // Set random direction when spawned (left or right)
            _moveDirection = Random.value < 0.5f ? Vector2.left : Vector2.right;
        }

        protected override void Move()
        {
            if (!_isMoving) return;
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
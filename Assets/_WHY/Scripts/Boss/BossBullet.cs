using UnityEngine;
using Utilities;
using GameHandlers;

namespace _WHY.Scripts.Boss
{
    public class BossBullet : WHYBaseMono, IPoolable
    {
        private static readonly int Hit = Animator.StringToHash("Hit");
        private Rigidbody2D _rb;
        private Animator _animator;
        private bool _isFrozen = false;
        private Vector2 _savedVelocity;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _rb = GetComponent<Rigidbody2D>();
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

        public void Reset()
        {
            _isFrozen = false;
            _rb.linearVelocity = Vector2.zero;
        }

        private void OnFreeze()
        {
            if (_isFrozen) return;
            _isFrozen = true;
            _savedVelocity = _rb.linearVelocity;
            _rb.linearVelocity = Vector2.zero;
            _rb.simulated = false;
            _animator.speed = 0;
        }

        private void OnUnFreeze()
        {
            if (!_isFrozen) return;
            _isFrozen = false;
            _rb.simulated = true;
            _rb.linearVelocity = _savedVelocity;
            _animator.speed = 1;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                ReturnToPool();
                return;
            }
            if (IsHittableTag(other.tag))
            {
                _rb.linearVelocity = Vector2.zero;
                _animator.SetTrigger(Hit);
            }
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                ReturnToPool();
                return;
            }
            if (IsHittableTag(other.gameObject.tag))
            {
                _rb.linearVelocity = Vector2.zero;  
                _animator.SetTrigger(Hit);
            }
        }

        private bool IsHittableTag(string nameTag)
        {
            return nameTag is "Step" or "Background" or "Floor";
        }

        public void ReturnToPool()
        {
            BossBulletPool.Instance.Return(this);
        }
    }
}

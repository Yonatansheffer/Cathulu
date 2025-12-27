using B.O.S.S.Domains.Utilities.GameHandlers.Scripts;
using UnityEngine;

namespace B.O.S.S.Domains.Enemies.Scripts.Planet_Enemy
{
    public class EnemyBullet : BossBaseMono, IPoolable
    {
        private bool _isFrozen;
        private Rigidbody2D _rb;
        private Vector2 _savedVelocity;
        [SerializeField] private float lifeTime = 5f;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void OnEnable()
        {
            GameEvents.FreezeLevel += OnFreeze;
            GameEvents.UnFreezeLevel += OnUnFreeze;
            Invoke(nameof(ReturnToPool), lifeTime);
        }

        private void OnDisable()
        {
            GameEvents.FreezeLevel -= OnFreeze;
            GameEvents.UnFreezeLevel -= OnUnFreeze;
            CancelInvoke(nameof(ReturnToPool));

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
        }

        private void OnUnFreeze()
        {
            if (!_isFrozen) return;
            _isFrozen = false;
            _rb.simulated = true;
            _rb.linearVelocity = _savedVelocity;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if(other.gameObject.CompareTag("Weapon")) return;
            ReturnToPool();
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if(other.gameObject.CompareTag("Weapon")) return;
            ReturnToPool();
        }

        public void ReturnToPool()
        {
            EnemyBulletPool.Instance.Return(this);
        }
    }
}

using System.Collections;
using RiseOfCathulu.Domains.Utilities.GameHandlers.Scripts;
using UnityEngine;

namespace RiseOfCathulu.Domains.Enemies.Scripts.Planet_Enemy
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyBullet : MonoBehaviour, IPoolable
    {
        [Header("Lifetime")]
        [SerializeField] private float lifeTime = 5f;

        private Rigidbody2D _rb;
        private bool _isFrozen;
        private bool _returned;
        private Vector2 _savedVelocity;
        private Coroutine _lifeRoutine;

        // ----------------------------------------------------

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void OnEnable()
        {
            Reset();

            GameEvents.FreezeLevel += OnFreeze;
            GameEvents.UnFreezeLevel += OnUnFreeze;

            _lifeRoutine = StartCoroutine(LifeTimer());
        }

        private void OnDisable()
        {
            GameEvents.FreezeLevel -= OnFreeze;
            GameEvents.UnFreezeLevel -= OnUnFreeze;

            if (_lifeRoutine != null)
            {
                StopCoroutine(_lifeRoutine);
                _lifeRoutine = null;
            }
        }

        // ----------------------------------------------------
        // Pool reset (CALLED by pool OR OnEnable)
        // ----------------------------------------------------
        public void Reset()
        {
            _returned = false;
            _isFrozen = false;

            if (_rb)
            {
                _rb.simulated = true;
                _rb.linearVelocity = Vector2.zero;
                _rb.angularVelocity = 0f;
            }
        }

        // ----------------------------------------------------
        // Freeze handling
        // ----------------------------------------------------
        private void OnFreeze()
        {
            if (_isFrozen || _returned) return;

            _isFrozen = true;

            if (_rb)
            {
                _savedVelocity = _rb.linearVelocity;
                _rb.linearVelocity = Vector2.zero;
                _rb.simulated = false;
            }
        }

        private void OnUnFreeze()
        {
            if (!_isFrozen || _returned) return;

            _isFrozen = false;

            if (_rb)
            {
                _rb.simulated = true;
                _rb.linearVelocity = _savedVelocity;
            }
        }

        // ----------------------------------------------------
        // Lifetime logic (freeze-aware)
        // ----------------------------------------------------
        private IEnumerator LifeTimer()
        {
            float elapsed = 0f;

            while (elapsed < lifeTime)
            {
                if (!_isFrozen)
                    elapsed += Time.deltaTime;

                yield return null;
            }

            ReturnToPool();
        }

        // ----------------------------------------------------
        // Collisions
        // ----------------------------------------------------
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_returned) return;
            if (other.CompareTag("Weapon")) return;

            ReturnToPool();
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (_returned) return;
            if (other.gameObject.CompareTag("Weapon")) return;

            ReturnToPool();
        }

        // ----------------------------------------------------
        // Pool return
        // ----------------------------------------------------
        public void ReturnToPool()
        {
            if (_returned) return;

            _returned = true;

            if (_lifeRoutine != null)
            {
                StopCoroutine(_lifeRoutine);
                _lifeRoutine = null;
            }

            EnemyBulletPool.Instance.Return(this);
        }
    }
}

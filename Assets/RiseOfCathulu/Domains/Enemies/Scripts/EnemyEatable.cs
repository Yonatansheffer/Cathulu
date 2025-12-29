using RiseOfCathulu.Domains.Player.Scripts;
using RiseOfCathulu.Domains.Utilities.GameHandlers.Scripts;
using UnityEngine;

namespace RiseOfCathulu.Domains.Enemies.Scripts
{
    public class EnemyEatable : MonoBehaviour
    {
        [Header("Eatable Settings")]
        [SerializeField] private int sizeLevel = 1;
        [SerializeField] private float eatableChance = 0.5f;

        public int SizeLevel => sizeLevel;
        public bool IsEatable { get; private set; }

        private SpriteRenderer _spriteRenderer;
        private Color _baseColor;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer)
                _baseColor = _spriteRenderer.color;
        }

        private void OnEnable()
        {
            ResetEatableState();
        }

        public void ResetEatableState()
        {
            IsEatable = Random.value < eatableChance;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!collision.gameObject.CompareTag("Player")) return;

            var playerSize = collision.gameObject.GetComponent<PlayerSize>();
            if (playerSize == null) return;

            // Eat
            if (IsEatable && playerSize.IsBiggerThan(sizeLevel))
            {
                GameEvents.ChangePlayerSize?.Invoke(+1);
                ReturnEnemy();
            }
            // Hit
            else
            {
                GameEvents.ChangePlayerSize?.Invoke(-1);
            }
            ReturnEnemy();
        }

        private void ReturnEnemy()
        {
            var enemy = GetComponent<FlyingEnemy>();
            if (enemy != null)
            {
                enemy.ReturnToPoolExternally();
            }
        }
    }
}
using RiseOfCathulu.Domains.Player.Scripts;
using UnityEngine;

namespace RiseOfCathulu.Domains.Enemies.Scripts.Planet_Enemy
{
    public class PlanetEnemyShield : MonoBehaviour
    {
        [SerializeField] private float requiredSizeLevel;
        private PlayerSize _playerSize;
        private SpriteRenderer _spriteRenderer;
        private CapsuleCollider2D _capsuleCollider;
        private void Awake()
        {
            _playerSize = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerSize>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _capsuleCollider = GetComponent<CapsuleCollider2D>();

        }

        private void Update()
        {
            CheckPlayerSize();
        }

        private void CheckPlayerSize()
        {
            if (_playerSize.CurrentSizeLevel >= requiredSizeLevel)
            {
                _spriteRenderer.enabled = false;
                _capsuleCollider.enabled = false;
            }
            else
            {
                _spriteRenderer.enabled = true;
                _capsuleCollider.enabled = true;
            }
        }
    }
}
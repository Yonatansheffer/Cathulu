using RiseOfCathulu.Domains.Player.Scripts;
using UnityEngine;

namespace RiseOfCathulu.Domains.Enemies.Scripts
{
    public class LevelIndicator : MonoBehaviour
    {
        [Header("Colors")]
        [SerializeField] private Color eatableColor = Color.green;
        [SerializeField] private Color dangerousColor = Color.red;
    
        [Header("References")]
        [SerializeField] private SpriteRenderer lightRenderer;
        [SerializeField] private Enemy parentEnemy;

        private PlayerSize _player;

        private void Awake()
        {
            // Cache the player reference once
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) _player = playerObj.GetComponent<PlayerSize>();
        
            // If not assigned in inspector, try to find on parent
            if (parentEnemy == null) parentEnemy = GetComponentInParent<Enemy>();
            if (lightRenderer == null) lightRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            if (_player == null || parentEnemy == null || lightRenderer == null) return;    
            if (_player.CurrentSizeLevel > parentEnemy.sizeLevel)
            {
                lightRenderer.color = eatableColor;
            }
            else
            {
                lightRenderer.color = dangerousColor;
            }
        }
    }
}
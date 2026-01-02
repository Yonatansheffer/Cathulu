using UnityEngine;
using RiseOfCathulu.Domains.Player.Scripts;

namespace RiseOfCathulu.Domains.Enemies.Scripts
{
    public class EatableIndicator : MonoBehaviour
    {
        [Header("Colors")]
        [SerializeField] private Color eatableColor = Color.green;
        [SerializeField] private Color dangerousColor = Color.red;
    
        [Header("References")]
        [SerializeField] private SpriteRenderer lightRenderer;
        [SerializeField] private FlyingEnemy parentEnemy; 
        
        private void Awake()
        {
            if (parentEnemy == null) parentEnemy = GetComponentInParent<FlyingEnemy>();
            if (lightRenderer == null) lightRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
                lightRenderer.color = parentEnemy.IsEatable ? eatableColor : dangerousColor;
        }
    }
}
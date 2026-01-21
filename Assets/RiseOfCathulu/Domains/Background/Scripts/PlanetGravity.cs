using System;
using RiseOfCathulu.Domains.Utilities.GameHandlers.Scripts;
using UnityEngine;

namespace RiseOfCathulu.Domains.Background.Scripts
{
    [RequireComponent(typeof(Collider2D))]
    public class PlanetGravity : MonoBehaviour
    {
        [SerializeField] private Transform planetCenter;
        [SerializeField] private PlanetDestruction planetDestruction;
        [SerializeField] private float destructableGravityMultiplier = 100f;
        private bool _isDestructableApplied;
        private float _initialGravityStrength;

        [Header("Planet Stats")]
        public float gravityStrength = 30f;
        public float maxOrbitSpeed = 25f;
        public float vortexGrip = 2f;
        public int levelForCollectibles = 1;

        private void Awake()
        {
            _initialGravityStrength = gravityStrength;
        }

        private void Reset()
        {
            planetCenter = transform.parent;
        }

        private void Update()
        {
            if(planetDestruction != null && planetDestruction.isDestructable && !_isDestructableApplied)
            {
                gravityStrength *= destructableGravityMultiplier;
                _isDestructableApplied = true;
            }
            else if (planetDestruction != null && !planetDestruction.isDestructable && _isDestructableApplied)
            {
                gravityStrength = _initialGravityStrength;
                _isDestructableApplied = false;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            GameEvents.OnEnteredGravityZone?.Invoke(
                planetCenter.position,
                gravityStrength,
                maxOrbitSpeed,
                vortexGrip
            );
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            GameEvents.OnExitedGravityZone?.Invoke();
        }
    }
}
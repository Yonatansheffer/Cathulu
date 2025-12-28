using RiseOfCathulu.Domains.Utilities.GameHandlers.Scripts;
using UnityEngine;

namespace RiseOfCathulu.Domains.Background.Scripts
{
    [RequireComponent(typeof(Collider2D))]
    public class PlanetGravity : MonoBehaviour
    {
        [SerializeField] private Transform planetCenter;

        [Header("Planet Stats")]
        public float gravityStrength = 30f;
        public float maxOrbitSpeed = 25f;
        public float vortexGrip = 2f;

        private void Reset()
        {
            planetCenter = transform.parent;
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
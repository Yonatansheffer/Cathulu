using B.O.S.S.Domains.Utilities.GameHandlers.Scripts;

namespace B.O.S.S.Domains.Background.Scripts
{
    using UnityEngine;

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
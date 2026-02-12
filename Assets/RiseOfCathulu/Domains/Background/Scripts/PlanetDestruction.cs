using System.Linq;
using RiseOfCathulu.Domains.Utilities.GameHandlers.Scripts;
using RiseOfCathulu.Domains.Utilities.Sound.Scripts;
using UnityEngine;

namespace RiseOfCathulu.Domains.Background.Scripts
{
    public class PlanetDestruction : MonoBehaviour
    {
        [Header("Particles")]
        [SerializeField, Tooltip("Stars particle prefab on death")] private GameObject orangeStarsParticles;
        [SerializeField, Tooltip("Stars particle size")] private float particlesSize;
        [SerializeField, Tooltip("Points awarded for destroying this planet")] private int pointsForKill = 1;
        [SerializeField] private SpriteRenderer destructibleLight;
        [SerializeField] private GameObject[] planetEnemies;
        [SerializeField] private bool isSun;
        [SerializeField] private bool isTutorial;
        public bool isDestructable;
        
        private void Update()
        {
            CheckPlayerSize();
        }

        private void CheckPlayerSize()
        {
            if (planetEnemies.Any(enemy => enemy)) return;
            destructibleLight.color = Color.springGreen;
            isDestructable = true;
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if(!isDestructable || !other.gameObject.CompareTag("Player")) return;
            var particles = Instantiate(orangeStarsParticles, transform.position, Quaternion.identity);
            particles.transform.localScale *= particlesSize;
            Destroy(particles, 2f);
            SoundManager.Instance.PlaySound("Explosion", transform);
            GameEvents.AddPoints?.Invoke(pointsForKill);
            if(isTutorial) GameEvents.TutorialFinished?.Invoke();
            if (isSun) GameEvents.DestroyedSun?.Invoke();
            Destroy(gameObject);
        }
    }
}
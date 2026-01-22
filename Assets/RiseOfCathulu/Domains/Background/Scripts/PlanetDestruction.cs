using System;
using System.Collections;
using System.Linq;
using RiseOfCathulu.Domains.Player.Scripts;
using RiseOfCathulu.Domains.Utilities.GameHandlers.Scripts;
using RiseOfCathulu.Domains.Utilities.Sound.Scripts;
using UnityEngine;
using UnityEngine.Serialization;

namespace RiseOfCathulu.Domains.Background.Scripts
{
    public class PlanetDestruction : MonoBehaviour
    {
        [Header("Particles")]
        [SerializeField, Tooltip("Stars particle prefab on death")] private GameObject orangeStarsParticles;
        [SerializeField, Tooltip("Stars particle size")] private float particlesSize;
        [SerializeField, Tooltip("Points awarded for destroying this enemy")] private int pointsForKill = 1;

        [SerializeField] private SpriteRenderer destructibleLight;
        [SerializeField] private GameObject[] planetEnemies;
        [SerializeField] private bool isSun;
        [SerializeField] private bool isTutorial;

        [SerializeField] private Color color;
        
        private PlayerSize _playerSize;
        public bool isDestructable = false;
        
        private void Awake()
        {
            _playerSize = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerSize>();
        }

        private void Update()
        {
            CheckPlayerSize();
        }

        private void CheckPlayerSize()
        {
            if (planetEnemies.Any(enemy => enemy)) return;
            destructibleLight.color = color;
            isDestructable = true;
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if(!isDestructable || !other.gameObject.CompareTag("Player")) return;
            var particles = Instantiate(orangeStarsParticles, transform.position, Quaternion.identity);
            Vector3 parentWorldScale = transform.lossyScale;
            particles.transform.localScale = 
                Vector3.Scale(particles.transform.localScale, parentWorldScale * particlesSize);
            Destroy(particles, 2f);
            SoundManager.Instance.PlaySound("Explosion", transform);
            GameEvents.AddPoints?.Invoke(pointsForKill);
            if (isTutorial) StartCoroutine(EndTutorial());
            if (isSun) GameEvents.DestroyedSun?.Invoke();
            Destroy(gameObject);
        }
        
        private IEnumerator EndTutorial()
        {
            yield return new WaitForSeconds(1f);
            GameEvents.TutorialFinished?.Invoke();
        }
    }
}
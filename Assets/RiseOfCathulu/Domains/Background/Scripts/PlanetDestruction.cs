using System;
using System.Linq;
using RiseOfCathulu.Domains.Player.Scripts;
using RiseOfCathulu.Domains.Utilities.Sound.Scripts;
using UnityEngine;

namespace RiseOfCathulu.Domains.Background.Scripts
{
    public class PlanetDestruction : MonoBehaviour
    {
        [Header("Particles")]
        [SerializeField, Tooltip("Stars particle prefab on death")] private GameObject orangeStarsParticles;
        [SerializeField, Tooltip("Stars particle size")] private float particlesSize;

        [SerializeField] private float requiredSizeLevel;
        [SerializeField] private GameObject destructibleLight;
        [SerializeField] private GameObject[] planetEnemies;
        private PlayerSize _playerSize;
        private bool _isDestructable = false;
        
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
            if (_playerSize.CurrentSizeLevel >= requiredSizeLevel)
            {
                if (planetEnemies.Any(enemy => enemy))
                {
                    return;
                }
                destructibleLight.SetActive(true);
                _isDestructable = true;
            }
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if(!_isDestructable || !other.gameObject.CompareTag("Player")) return;
            var particles = Instantiate(orangeStarsParticles, transform.position, Quaternion.identity);
            Vector3 parentWorldScale = transform.lossyScale;
            particles.transform.localScale = 
                Vector3.Scale(particles.transform.localScale, parentWorldScale * particlesSize);
            Destroy(particles, 2f);
            SoundManager.Instance.PlaySound("Explosion", transform);
            Destroy(gameObject);
        }
    }
}
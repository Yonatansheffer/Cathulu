using System.Collections;
using RiseOfCathulu.Domains.Utilities.GameHandlers.Scripts;
using RiseOfCathulu.Domains.Utilities.Sound.Scripts;
using UnityEngine;

namespace RiseOfCathulu.Domains.Enemies.Scripts.Planet_Enemy
{
    public class PlanetEnemyAnimations : MonoBehaviour
    {
        [SerializeField, Tooltip("Stars particle prefab on death")] private GameObject orangeStarsParticles;
        [SerializeField, Tooltip("Stars particle size")] private float particlesSize;
        private static readonly int Shoot = Animator.StringToHash("shoot");
        private static readonly int Spawn = Animator.StringToHash("spawn");
        private static readonly int Damage = Animator.StringToHash("damage");
        private static readonly int Death = Animator.StringToHash("death");
        private Animator _animator;

        private void Start()
        {
            _animator = GetComponent<Animator>();
        }

        private void OnEnable()
        {
            GameEvents.PlanetEnemyShoots += TriggerShootAnimation;
            GameEvents.ToSpawnEnemy += TriggerSpawnAnimation;
            GameEvents.EnemySpawned += TriggerSpawnAnimation;
            GameEvents.PlanetEnemyDestroyed += DeathAnimation;
        }
        
        private void OnDisable()
        {
            GameEvents.PlanetEnemyShoots -= TriggerShootAnimation;
            GameEvents.ToSpawnEnemy -= TriggerSpawnAnimation;
            GameEvents.EnemySpawned -= TriggerSpawnAnimation;
            GameEvents.PlanetEnemyDestroyed -= DeathAnimation;
        }
        
        private void DeathAnimation(Transform parent)
        {
            if (transform.parent != parent) return;
            _animator.SetTrigger(Death);
            StartCoroutine(ShakeAndDestroy());
        }

        private void TriggerShootAnimation()
        {
            _animator.SetTrigger(Shoot);
        }

        private void TriggerSpawnAnimation()
        {
            _animator.SetTrigger(Spawn);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if(other.CompareTag("Weapon"))
            {
                _animator.SetTrigger(Damage);
            }
        }
        private IEnumerator ShakeAndDestroy()
        {
            yield return new WaitForSeconds(0.1f);
            transform.rotation = Quaternion.identity;
            var duration = 1f;
            var elapsed = 0f;
            var startTilt = 40f;
            var endTilt = 7f;
            var frequency = 45f;
            while (elapsed < duration)
            {
                SoundManager.Instance.PlaySound("Boss Damage", transform);
                var currentTilt = Mathf.Lerp(startTilt, endTilt, elapsed / duration);
                var angle = Mathf.Sin(Time.time * frequency) * currentTilt;
                transform.rotation = Quaternion.Euler(0f, 0f, angle);
                elapsed += Time.deltaTime;
                yield return null;
            }
            transform.rotation = Quaternion.identity;
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
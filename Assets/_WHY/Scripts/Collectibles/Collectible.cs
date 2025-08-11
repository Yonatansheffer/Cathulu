using System.Collections;
using UnityEngine;

namespace _WHY.Scripts.Collectibles
{
    [RequireComponent(typeof(SpriteRenderer))]
    public abstract class Collectible : MonoBehaviour
    {
        private SpriteRenderer _spriteRenderer;
        [SerializeField, Tooltip("Speed at which the collectible falls")] protected float fallSpeed = 2f;
        [SerializeField, Tooltip("Time before collectible is destroyed after hitting floor")] protected float timeForDestroy = 20f;
        [SerializeField, Tooltip("Duration of blinking effect before destruction")] private float blinkDuration = 3f;
        [SerializeField, Tooltip("Interval between blinks")] private float blinkInterval = 0.1f;

        
        protected virtual void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }
    
        protected virtual void Update()
        {
            transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Floor") || other.CompareTag("Step"))
            {
                StopMovement();
                StartCoroutine(DestroyAfterDelay());
            }
            else if (other.CompareTag("Player"))
            {
                HandlePickup();
            }
        }

        protected abstract void HandlePickup();
        
        private IEnumerator DestroyAfterDelay()
        {
            yield return new WaitForSeconds(timeForDestroy);
            StartCoroutine(Blink(3f));
            yield return new WaitForSeconds(3f);
            Destroy(gameObject);
        }

        private IEnumerator Blink(float duration)
        {
            var endTime = Time.time + duration;
            while (Time.time < endTime)
            {
                _spriteRenderer.enabled = !_spriteRenderer.enabled; 
                yield return new WaitForSeconds(0.1f);
            }
            _spriteRenderer.enabled = true; 
        }

        public void StopMovement()
        {
            fallSpeed = 0;
        }
    }
}
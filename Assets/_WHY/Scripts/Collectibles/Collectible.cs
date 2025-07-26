using System.Collections;
using UnityEngine;

namespace Collectibles
{
    public abstract class Collectible : MonoBehaviour
    {
        private SpriteRenderer spriteRenderer;
        [SerializeField] protected float fallSpeed = 2f;
        
        protected virtual void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
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
            yield return new WaitForSeconds(30f);
            StartCoroutine(Blink(3f));
            yield return new WaitForSeconds(3f);
            Destroy(gameObject);
        }

        private IEnumerator Blink(float duration)
        {
            var endTime = Time.time + duration;
            while (Time.time < endTime)
            {
                spriteRenderer.enabled = !spriteRenderer.enabled; // Toggle visibility
                yield return new WaitForSeconds(0.1f); // Wait for 0.1 seconds before toggling again
            }
            spriteRenderer.enabled = true; // Ensure the sprite is visible after blinking
        }

        public void StopMovement()
        {
            fallSpeed = 0;
        }
    }
}
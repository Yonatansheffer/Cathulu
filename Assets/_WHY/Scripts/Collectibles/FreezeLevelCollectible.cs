using System.Collections;
using GameHandlers;
using Sound;
using UnityEngine;

namespace Collectibles
{
    public class FreezeLevelCollectible : Collectible
    {
        [SerializeField] private const float FreezeDuration = 10f;

        protected override void HandlePickup()
        {
            SoundManager.Instance.PlaySound("Freeze", transform);
            var sr = GetComponent<SpriteRenderer>();
            var col = GetComponent<Collider2D>();
            if (sr) sr.enabled = false;
            if (col) col.enabled = false;
            StartCoroutine(FreezeLevelCoroutine());
        }

        private IEnumerator FreezeLevelCoroutine()
        {
            GameEvents.FreezeLevel?.Invoke();
            yield return new WaitForSeconds(FreezeDuration);
            GameEvents.UnFreezeLevel?.Invoke();
            Destroy(gameObject);
        }

        public static float GetFreezeDuration() => FreezeDuration;
    }
}
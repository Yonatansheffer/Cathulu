using GameHandlers;
using Sound;
using UnityEngine;

namespace Collectibles
{
    public class FreezeLevelCollectible : Collectible
    {
        [SerializeField] private float freezeDuration = 5f;
        protected override void HandlePickup() 
        { 
            SoundManager.Instance.PlaySound("Freeze", transform);
            StartCoroutine(FreezeLevelCoroutine());
            Destroy(gameObject);
        }
        
        private System.Collections.IEnumerator FreezeLevelCoroutine()
        {
            GameEvents.FreezeLevel?.Invoke();
            yield return new WaitForSeconds(freezeDuration);
            GameEvents.UnFreezeLevel?.Invoke();
        }
    }
}
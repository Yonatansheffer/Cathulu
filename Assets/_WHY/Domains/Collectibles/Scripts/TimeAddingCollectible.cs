using _WHY.Domains.Utilities.GameHandlers.Scripts;
using Sound;
using UnityEngine;

namespace _WHY.Domains.Collectibles.Scripts
{
    public class TimeAddingCollectible : Collectible
    {
        [SerializeField] private float timeAddingDuration = 5f;
        protected override void HandlePickup() 
        { 
            SoundManager.Instance.PlaySound("Collected", transform);
            GameEvents.AddTime?.Invoke(timeAddingDuration);
            Destroy(gameObject);
        }
        
    }
}
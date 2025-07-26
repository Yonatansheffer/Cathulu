using GameHandlers;
using Sound;
using UnityEngine;

namespace Collectibles
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
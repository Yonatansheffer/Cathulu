using B.O.S.S.Domains.Utilities.GameHandlers.Scripts;
using B.O.S.S.Domains.Utilities.Sound.Scripts;
using UnityEngine;

namespace B.O.S.S.Domains.Collectibles.Scripts
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
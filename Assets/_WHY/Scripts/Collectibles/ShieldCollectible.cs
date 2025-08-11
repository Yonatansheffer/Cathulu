using _WHY.Scripts.Collectibles;
using GameHandlers;
using Sound;

namespace Collectibles
{
    public class ShieldCollectible : Collectible
    {
        protected override void HandlePickup() 
        { 
            SoundManager.Instance.PlaySound("Collected", transform);
            GameEvents.ShieldUpdated?.Invoke(true); 
            Destroy(gameObject);
        }
    }
}
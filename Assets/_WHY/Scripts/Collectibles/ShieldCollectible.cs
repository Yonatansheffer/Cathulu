using GameHandlers;
using Sound;

namespace Collectibles
{
    public class ShieldCollectible : Collectible
    {
        protected override void HandlePickup() 
        { 
            SoundManager.Instance.PlaySound("Collected", transform);
            GameEvents.ShieldCollected?.Invoke(); 
            Destroy(gameObject);
        }
    }
}
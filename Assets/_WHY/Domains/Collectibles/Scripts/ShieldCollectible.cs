using _WHY.Domains.Utilities.GameHandlers.Scripts;
using Sound;

namespace _WHY.Domains.Collectibles.Scripts
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
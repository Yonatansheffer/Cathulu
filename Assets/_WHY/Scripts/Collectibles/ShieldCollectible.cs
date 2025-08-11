using GameHandlers;
using Sound;

namespace _WHY.Scripts.Collectibles
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
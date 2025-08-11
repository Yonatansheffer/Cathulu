using _WHY.Domains.Utilities.GameHandlers.Scripts;
using Sound;

namespace _WHY.Domains.Collectibles.Scripts
{
    public class LifeCollectible : Collectible
    {
        protected override void HandlePickup() 
        { 
            SoundManager.Instance.PlaySound("Collected", transform);
            GameEvents.AddLifeToPlayer?.Invoke(1);
            Destroy(gameObject);
        }
    }
}
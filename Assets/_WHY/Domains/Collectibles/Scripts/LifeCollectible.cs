using _WHY.Domains.Utilities.GameHandlers.Scripts;
using _WHY.Domains.Utilities.Sound.Scripts;

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
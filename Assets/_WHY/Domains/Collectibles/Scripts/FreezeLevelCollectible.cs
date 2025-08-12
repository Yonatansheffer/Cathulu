using _WHY.Domains.Utilities.GameHandlers.Scripts;

namespace _WHY.Domains.Collectibles.Scripts
{
    public class FreezeLevelCollectible : Collectible
    {
        protected override void HandlePickup()
        {
            GameEvents.FreezeCollected?.Invoke();
            Destroy(gameObject);
        }
    }
}
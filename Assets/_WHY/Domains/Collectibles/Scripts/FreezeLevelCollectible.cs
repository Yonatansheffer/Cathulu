using _WHY.Domains.Utilities.GameHandlers.Scripts;
using Sound;

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
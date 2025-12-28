using RiseOfCathulu.Domains.Utilities.GameHandlers.Scripts;

namespace RiseOfCathulu.Domains.Collectibles.Scripts
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
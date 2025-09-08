using B.O.S.S.Domains.Utilities.GameHandlers.Scripts;

namespace B.O.S.S.Domains.Collectibles.Scripts
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
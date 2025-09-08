using B.O.S.S.Domains.Utilities.GameHandlers.Scripts;
using B.O.S.S.Domains.Utilities.Sound.Scripts;

namespace B.O.S.S.Domains.Collectibles.Scripts
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
using B.O.S.S.Domains.Utilities.GameHandlers.Scripts;
using B.O.S.S.Domains.Utilities.Sound.Scripts;

namespace B.O.S.S.Domains.Collectibles.Scripts
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
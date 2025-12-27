using B.O.S.S.Domains.Utilities.GameHandlers.Scripts;
using B.O.S.S.Domains.Utilities.Sound.Scripts;

namespace B.O.S.S.Domains.Collectibles.Scripts
{
    public class SizeCollectible : Collectible
    {
        protected override void HandlePickup() 
        { 
            SoundManager.Instance.PlaySound("Collected", transform);
            GameEvents.ChangePlayerSize?.Invoke(1);
            Destroy(gameObject);
        }
    }
}
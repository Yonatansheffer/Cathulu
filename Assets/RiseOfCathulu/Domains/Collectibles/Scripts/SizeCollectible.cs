using RiseOfCathulu.Domains.Utilities.GameHandlers.Scripts;
using RiseOfCathulu.Domains.Utilities.Sound.Scripts;

namespace RiseOfCathulu.Domains.Collectibles.Scripts
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
using GameHandlers;
using Sound;

namespace Collectibles
{
    public class BalloonStopperCollectible : Collectible
    {
        protected override void HandlePickup() 
        { 
            SoundManager.Instance.PlaySound("Collected", transform);
            GameEvents.EnemyStopperCollected?.Invoke(); 
            Destroy(gameObject);
        }
    }
}
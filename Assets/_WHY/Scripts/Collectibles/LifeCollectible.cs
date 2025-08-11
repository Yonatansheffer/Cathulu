using GameHandlers;
using Sound;

namespace _WHY.Scripts.Collectibles
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
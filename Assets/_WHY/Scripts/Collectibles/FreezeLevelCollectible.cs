using _WHY.Scripts.Collectibles;
using GameHandlers;
using Sound;

namespace Collectibles
{
    public class FreezeLevelCollectible : Collectible
    {
        protected override void HandlePickup()
        {
            SoundManager.Instance.PlaySound("Freeze", transform);
            GameEvents.FreezeCollected?.Invoke();
            Destroy(gameObject);
        }
    }
}
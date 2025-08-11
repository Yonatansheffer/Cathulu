using _WHY.Domains.Utilities.GameHandlers.Scripts;
using Sound;
using UnityEngine;

namespace _WHY.Domains.Collectibles.Scripts
{
    public class PointCollectible : Collectible
    {
        [SerializeField] private int pointsValue;

        protected override void HandlePickup()
        {
            SoundManager.Instance.PlaySound("Collected", transform);
            GameEvents.AddPoints?.Invoke(pointsValue);
            Destroy(gameObject);
        }
    }
}

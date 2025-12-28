using RiseOfCathulu.Domains.Utilities.GameHandlers.Scripts;
using RiseOfCathulu.Domains.Utilities.Sound.Scripts;
using UnityEngine;

namespace RiseOfCathulu.Domains.Collectibles.Scripts
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

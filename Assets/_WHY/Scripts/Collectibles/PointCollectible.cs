using GameHandlers;
using Sound;
using UnityEngine;

namespace _WHY.Scripts.Collectibles
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

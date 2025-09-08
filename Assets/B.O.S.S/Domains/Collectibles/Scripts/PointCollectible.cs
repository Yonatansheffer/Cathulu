using B.O.S.S.Domains.Utilities.GameHandlers.Scripts;
using B.O.S.S.Domains.Utilities.Sound.Scripts;
using UnityEngine;

namespace B.O.S.S.Domains.Collectibles.Scripts
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

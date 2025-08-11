using _WHY.Scripts.Collectibles;
using GameHandlers;
using Sound;
using UnityEngine;

namespace Collectibles
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
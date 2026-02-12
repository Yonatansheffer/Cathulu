using RiseOfCathulu.Domains.Utilities.GameHandlers.Scripts;
using RiseOfCathulu.Domains.Utilities.Sound.Scripts;
using UnityEngine;
using UnityEngine.Serialization;

namespace RiseOfCathulu.Domains.Collectibles.Scripts
{
    public class SpeedCollectible : Collectible
    {
        [SerializeField] private float speedFactor = 1.5f;
        [SerializeField] private float speedDuration = 8f;

        protected override void HandlePickup() 
        { 
            SoundManager.Instance.PlaySound("Collected", transform);
            GameEvents.AddSpeed?.Invoke(speedFactor, speedDuration);
            Destroy(gameObject);
        }
        
    }
}
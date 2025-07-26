using GameHandlers;
using Sound;
using UnityEngine;
using Weapons;

namespace Collectibles
{
    public class WeaponCollectible : Collectible
    {
        [SerializeField] private WeaponType weaponType;
        protected override void HandlePickup() 
        { 
            SoundManager.Instance.PlaySound("Collected", transform);
            GameEvents.WeaponCollected?.Invoke(weaponType); 
            Destroy(gameObject);
        }

        public WeaponType GetWeaponType()
        {
            return weaponType;
        }
    }
}

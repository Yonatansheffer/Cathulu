using _WHY.Domains.Utilities.GameHandlers.Scripts;
using _WHY.Domains.Weapons.Scripts;
using Sound;
using UnityEngine;
using Weapons;

namespace _WHY.Domains.Collectibles.Scripts
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

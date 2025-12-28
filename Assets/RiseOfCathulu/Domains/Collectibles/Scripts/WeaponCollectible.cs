using RiseOfCathulu.Domains.Utilities.GameHandlers.Scripts;
using RiseOfCathulu.Domains.Utilities.Sound.Scripts;
using RiseOfCathulu.Domains.Weapons.Scripts;
using UnityEngine;

namespace RiseOfCathulu.Domains.Collectibles.Scripts
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

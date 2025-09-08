using B.O.S.S.Domains.Utilities.GameHandlers.Scripts;
using B.O.S.S.Domains.Utilities.Sound.Scripts;
using B.O.S.S.Domains.Weapons.Scripts;
using UnityEngine;

namespace B.O.S.S.Domains.Collectibles.Scripts
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

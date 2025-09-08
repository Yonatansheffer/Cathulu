using System;
using UnityEngine;

namespace B.O.S.S.Domains.Weapons.Scripts
{
    [CreateAssetMenu(fileName = "WeaponSettings", menuName = "Weapon System/Weapon Settings", order = 1)]
    public class WeaponSettings : ScriptableObject
    {
        public WeaponConfig[] weaponConfigs;
        public WeaponType defaultWeapon;
    }
    
    [Serializable]
    public class WeaponConfig
    {
        public WeaponType weaponType;
        public Projectile projectilePrefab;
        public float shotSpeed;
        public int maxProjectileCount;
        public float shotCooldown;
    }
    public enum WeaponType
    {
        SpellGun,
        LightGun,
        FireGun
    }
}
using System;
using UnityEngine;

namespace Weapons
{
    [CreateAssetMenu(fileName = "WeaponSettings", menuName = "Weapon System/Weapon Settings", order = 1)]
    public class WeaponSettings : ScriptableObject
    {
        public WeaponConfig[] weaponConfigs;
    }
    
    [Serializable]
    public class WeaponConfig
    {
        public WeaponType weaponType;
        public Projectile projectilePrefab;
        public float shotSpeed;
        public int maxProjectileCount;
    }
    public enum WeaponType
    {
        SpellGun,
        LightGun
    }
}
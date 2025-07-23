using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using GameHandlers;
using Sound;

namespace Weapons
{
    public class WeaponController : MonoBehaviour 
    {
        [SerializeField] private WeaponSettings settings;
        [SerializeField] private ShootingLight shootingLight;
        [SerializeField] private Vector3 adding;
        private static readonly int LeftShoot = Animator.StringToHash("LeftShoot");
        private static readonly int RightShoot = Animator.StringToHash("RightShoot");
        private static readonly int Idle = Animator.StringToHash("Idle");
        private WeaponConfig _currentWeaponConfig;
        private List<Projectile> _activeProjectiles;
        
        private void Awake()
        {
            _activeProjectiles = new List<Projectile>();
            ResetWeapons();
        }
        private void OnEnable()
        {
            GameEvents.Shoot += Shoot;
            GameEvents.DefaultWeapon += ResetWeapons;
            GameEvents.RestartLevel += ResetWeapons;
            GameEvents.WeaponCollected += SwitchWeapon;
            GameEvents.FreezeLevel += StopAllProjectiles;
        }
        private void OnDisable()
        {
            GameEvents.Shoot -= Shoot;
            GameEvents.DefaultWeapon -= ResetWeapons;
            GameEvents.RestartLevel -= ResetWeapons;
            GameEvents.WeaponCollected -= SwitchWeapon;
            GameEvents.FreezeLevel -= StopAllProjectiles;
        }
        
        private void ResetWeapons()
        { 
            foreach (var projectile in _activeProjectiles)
            {
                Destroy(projectile.gameObject);
            }
            _activeProjectiles.Clear();
            SwitchWeapon(WeaponType.LightGun);   
        }
        
        private void SwitchWeapon(WeaponType weaponType)
        {
            _currentWeaponConfig = settings.weaponConfigs.FirstOrDefault(config => config.weaponType == weaponType);
            _activeProjectiles.Clear();
        }
        
        private void StopAllProjectiles()
        {
            foreach (var projectile in _activeProjectiles)
            {
                var animator = projectile.GetComponent<Animator>();
                animator.SetTrigger(Idle);
                projectile.Stop();
            }
        }

        private void Shoot(Transform t)
        {
            if (_currentWeaponConfig == null || _activeProjectiles.Count >= _currentWeaponConfig.maxProjectileCount)
                return;
            SoundManager.Instance.PlaySound(
                _currentWeaponConfig.weaponType == WeaponType.LightGun ? "Light Shot" : "Shoot", transform);
            shootingLight.gameObject.SetActive(true);
            Vector2 shootDirection = t.up; // Uses the transform's up direction (based on rotation)
            var shootProjectile = Instantiate(_currentWeaponConfig.projectilePrefab,
                t.position + t.TransformDirection(adding), // Transform offset based on rotation
                t.rotation); // Match the gun's rotation

            // Launch in the direction the gun is facing
            shootProjectile.Launch(shootDirection * _currentWeaponConfig.shotSpeed);

            var projectile = shootProjectile.GetComponent<Projectile>();
            _activeProjectiles.Add(shootProjectile);
            projectile.OnDestroy += () => _activeProjectiles.Remove(shootProjectile); 
        }
    }
}


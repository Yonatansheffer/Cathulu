using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _WHY.Domains.Utilities.GameHandlers.Scripts;
using Sound;
using UnityEngine;
using Weapons;

namespace _WHY.Domains.Weapons.Scripts
{
    public class WeaponController : MonoBehaviour 
    {
        [SerializeField, Tooltip("Weapon settings configuration")] private WeaponSettings settings;
        [SerializeField, Tooltip("Shooting light effect GameObject")] private ShootingLight shootingLight;
        [SerializeField, Tooltip("Offset for projectile spawn position")] private Vector3 adding;
        private static readonly int Idle = Animator.StringToHash("Idle");
        private WeaponConfig _currentWeaponConfig;
        private float _lastShotTime = -Mathf.Infinity;
        private List<Projectile> _activeProjectiles;
        private Coroutine _switchBackCoroutine;
        
        private void Awake()
        {
            _activeProjectiles = new List<Projectile>();
            ResetWeapons();
        }
        private void OnEnable()
        {
            GameEvents.Shoot += Shoot;
            GameEvents.RestartLevel += ResetWeapons;
            GameEvents.WeaponCollected += SwitchWeapon;
            GameEvents.PlayerDefeated += StopAllProjectiles;
        }
        private void OnDisable()
        {
            GameEvents.Shoot -= Shoot;
            GameEvents.RestartLevel -= ResetWeapons;
            GameEvents.WeaponCollected -= SwitchWeapon;
            GameEvents.PlayerDefeated -= StopAllProjectiles;
        }
        
        private void ResetWeapons()
        { 
            foreach (var projectile in _activeProjectiles)
            {
                Destroy(projectile.gameObject);
            }
            _activeProjectiles.Clear();
            SwitchWeapon(settings.defaultWeapon);   
        }
        
        private void SwitchWeapon(WeaponType weaponType)
        {
            _currentWeaponConfig = settings.weaponConfigs.FirstOrDefault(config => config.weaponType == weaponType);
            _activeProjectiles.Clear();
            if (_switchBackCoroutine != null)
            {
                StopCoroutine(_switchBackCoroutine);
                _switchBackCoroutine = null;
            }
            if (weaponType != settings.defaultWeapon)
                _switchBackCoroutine = StartCoroutine(SwitchBackToDefaultAfterDelay());
        }
        private IEnumerator SwitchBackToDefaultAfterDelay()
        {
            yield return new WaitForSeconds(15f);
            _switchBackCoroutine = null;
            SwitchWeapon(settings.defaultWeapon);
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
            if (Time.time < _lastShotTime + _currentWeaponConfig.shotCooldown) return;
            _lastShotTime = Time.time;
            PlayShotSound();
            shootingLight.gameObject.SetActive(true);
            Vector2 shootDirection = t.up;
            var shootProjectile = Instantiate(_currentWeaponConfig.projectilePrefab,
                t.position + t.TransformDirection(adding), t.rotation);
            shootProjectile.Launch(shootDirection * _currentWeaponConfig.shotSpeed);
            var projectile = shootProjectile.GetComponent<Projectile>();
            _activeProjectiles.Add(shootProjectile);
            projectile.OnDestroy += () => _activeProjectiles.Remove(shootProjectile); 
        }

        private void PlayShotSound()
        {
            if (_currentWeaponConfig == null) return;
            var sound = _currentWeaponConfig.weaponType switch
            {
                WeaponType.SpellGun => "Spell Shot",
                WeaponType.LightGun => "Light Shot",
                WeaponType.FireGun => "Fire Shot",
                _ => null
            };
            if (sound != null) SoundManager.Instance.PlaySound(sound, transform);
        }
    }
}


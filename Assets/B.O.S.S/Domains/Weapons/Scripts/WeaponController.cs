using System.Collections;
using System.Collections.Generic;
using System.Linq;
using B.O.S.S.Domains.Utilities.GameHandlers.Scripts;
using B.O.S.S.Domains.Utilities.Sound.Scripts;
using UnityEngine;

namespace B.O.S.S.Domains.Weapons.Scripts
{
    public class WeaponController : MonoBehaviour
    {
        [Header("Weapon Settings")]
        [SerializeField, Tooltip("Weapon settings configuration")]
        private WeaponSettings settings;
        [SerializeField, Tooltip("Shooting light effect GameObject")]
        private ShootingLight shootingLight;
        [SerializeField, Tooltip("Offset for projectile spawn position")]
        private Vector3 adding;

        private static readonly int Idle = Animator.StringToHash("Idle");

        private WeaponConfig _currentWeaponConfig;
        private float _lastShotTime = -Mathf.Infinity;
        private readonly List<Projectile> _activeProjectiles = new();
        private Coroutine _switchBackCoroutine;

        private void Awake()
        {
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
            DestroyAllProjectiles();
            SwitchWeapon(settings.defaultWeapon);
        }

        private void SwitchWeapon(WeaponType weaponType)
        {
            _currentWeaponConfig = settings.weaponConfigs.FirstOrDefault(config => config.weaponType == weaponType);
            _activeProjectiles.Clear();
            CancelSwitchBackCoroutine();

            if (weaponType != settings.defaultWeapon)
                _switchBackCoroutine = StartCoroutine(SwitchBackToDefaultAfterDelay());
        }

        private void CancelSwitchBackCoroutine()
        {
            if (_switchBackCoroutine != null)
            {
                StopCoroutine(_switchBackCoroutine);
                _switchBackCoroutine = null;
            }
        }

        private IEnumerator SwitchBackToDefaultAfterDelay()
        {
            yield return new WaitForSeconds(15f);
            _switchBackCoroutine = null;
            SwitchWeapon(settings.defaultWeapon);
        }

        private void Shoot(Transform t)
        {
            if (!CanShoot()) return;

            _lastShotTime = Time.time;
            PlayShotSound();
            shootingLight.gameObject.SetActive(true);

            SpawnProjectile(t);
        }

        private bool CanShoot()
        {
            return _currentWeaponConfig != null &&
                   _activeProjectiles.Count < _currentWeaponConfig.maxProjectileCount &&
                   Time.time >= _lastShotTime + _currentWeaponConfig.shotCooldown;
        }

        private void SpawnProjectile(Transform t)
        {
            Vector2 shootDirection = t.up;
            var projectileInstance = Instantiate(
                _currentWeaponConfig.projectilePrefab,
                t.position + t.TransformDirection(adding),
                t.rotation
            );

            projectileInstance.Launch(shootDirection * _currentWeaponConfig.shotSpeed);
            RegisterProjectile(projectileInstance);
        }

        private void RegisterProjectile(Projectile projectile)
        {
            _activeProjectiles.Add(projectile);
            projectile.OnDestroy += () => _activeProjectiles.Remove(projectile);
        }

        private void StopAllProjectiles()
        {
            foreach (var projectile in _activeProjectiles)
            {
                if (projectile != null)
                {
                    var animator = projectile.GetComponent<Animator>();
                    animator?.SetTrigger(Idle);
                    projectile.Stop();
                }
            }
        }

        private void DestroyAllProjectiles()
        {
            foreach (var projectile in _activeProjectiles)
            {
                if (projectile != null)
                    Destroy(projectile.gameObject);
            }
            _activeProjectiles.Clear();
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

            if (!string.IsNullOrEmpty(sound))
                SoundManager.Instance.PlaySound(sound, transform);
        }
    }
}

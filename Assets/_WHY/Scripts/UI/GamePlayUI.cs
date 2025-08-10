using System;
using Weapons;
using System.Collections;
using Collectibles;
using GameHandlers;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI
{
    public class GamePlayUI : MonoBehaviour
    {
        [SerializeField] private WeaponSettings settings;
        [SerializeField] private Slider bossHealthBar;
        [SerializeField] private TextMeshProUGUI pointsText;
        [SerializeField] private TextMeshProUGUI timeCountText;
        [SerializeField] private Image freezeImage;
        [SerializeField] private Image timeImage;
        [SerializeField] private Image spellGunImage;
        [SerializeField] private Image lightGunImage;
        [SerializeField] private Image fireGunImage;
        [SerializeField] private Image shieldImage;
        [SerializeField] private Image[] lifeImages;
        [SerializeField] private Image[] noLifeImages;
        [SerializeField] private GameObject orangeStarsParticles;
        [SerializeField] private float blinkDuration = 3f;
        private Coroutine _weaponCoroutine;
        [SerializeField] private Canvas canvas; 
        
        private void Start()
        {
            DeactivateAllPowerUps();
            ActivateDefaultWeapon(settings.defaultWeapon);
        }

        private void OnEnable()
        {
            GameEvents.BossLivesChanged += UpdateBossHealth;
            GameEvents.PlayerLivesChanged += UpdatePlayerHealth;
            GameEvents.UpdateScoreUI += UpdateScore;
            GameEvents.UpdateTimeUI += UpdateTimeCount;
            GameEvents.AddTime += UpdateTime;
            GameEvents.WeaponCollected += AddWeaponCollected;
            GameEvents.ShieldUpdated += UpdateShield;
            GameEvents.FreezeLevel += UpdateFreeze;
        }
        
        private void OnDisable()
        {
            GameEvents.UpdateScoreUI -= UpdateScore;
            GameEvents.BossLivesChanged -= UpdateBossHealth;
            GameEvents.PlayerLivesChanged -= UpdatePlayerHealth;
            GameEvents.UpdateTimeUI -= UpdateTimeCount;
            GameEvents.AddTime -= UpdateTime;
            GameEvents.WeaponCollected -= AddWeaponCollected;
            GameEvents.ShieldUpdated -= UpdateShield;
            GameEvents.FreezeLevel -= UpdateFreeze;
        }
        
        private void DeactivateAllPowerUps()
        {
            DeactivateAllWeapons();
            SetPowerUpActive(shieldImage, false);
            SetPowerUpActive(freezeImage, false);
            SetPowerUpActive(timeImage, false);
        }

        private void DeactivateAllWeapons()
        {
            if(_weaponCoroutine != null)
            {
                StopCoroutine(_weaponCoroutine);
                _weaponCoroutine = null;
            }
            SetPowerUpActive(spellGunImage, false);
            SetPowerUpActive(lightGunImage, false);
            SetPowerUpActive(fireGunImage,  false);
        }

        private void ActivateDefaultWeapon(WeaponType defaultWeapon)
        {
            switch (defaultWeapon)
            {
                case WeaponType.SpellGun:
                    SetPowerUpActive(spellGunImage, true);
                    break;
                case WeaponType.LightGun:
                    SetPowerUpActive(lightGunImage, true);
                    break;
                case WeaponType.FireGun:
                    SetPowerUpActive(fireGunImage, true);
                    break;
            }
        }
        
        private void UpdateShield(bool isActive)
        {
            SetPowerUpActive(shieldImage, isActive);
        }
        
        private void UpdateFreeze()
        {
            if (gameObject.activeInHierarchy)
                StartCoroutine(HandlePowerUpDisplay(freezeImage,
                    GameLoopManager.GetFreezeDuration()-blinkDuration, blinkDuration));
        }
        
        private void UpdateTime(float dummy)
        {
            var particles = Instantiate(orangeStarsParticles, timeImage.transform.position, Quaternion.identity, canvas.transform);

            particles.transform.localScale = Vector3.one * 0.5f;

            Destroy(particles, 0.8f);

            if (gameObject.activeInHierarchy)
                StartCoroutine(HandlePowerUpDisplay(timeImage,
                    1f, 0f));
        }

        
        private void UpdateBossHealth(int amount)
        {
            bossHealthBar.value = amount;
        }
        
        private void UpdatePlayerHealth(int amount)
        {
            for (var i = 0; i < lifeImages.Length; i++)
            {
                var isActive = i < amount;
                lifeImages[i].gameObject.SetActive(isActive);
                noLifeImages[i].gameObject.SetActive(!isActive);
            }
        }
        
        private void UpdateScore(int points)
        {
            pointsText.text = points.ToString();
        }
        
        private void UpdateTimeCount(int time)
        {
            timeCountText.text = time.ToString();
        }
        
        private void AddWeaponCollected(WeaponType weaponType)
        {
            DeactivateAllWeapons();
            if (weaponType == settings.defaultWeapon)
            {
                ActivateDefaultWeapon(weaponType);
                return;
            }
            switch (weaponType)
            {
                case WeaponType.SpellGun:
                    _weaponCoroutine =
                        StartCoroutine(HandlePowerUpDisplay(spellGunImage, 12f, blinkDuration));
                    break;
                case WeaponType.LightGun:
                    _weaponCoroutine =
                        StartCoroutine(HandlePowerUpDisplay(lightGunImage, 12f, blinkDuration));
                    break;
                case WeaponType.FireGun:
                    _weaponCoroutine =
                        StartCoroutine(HandlePowerUpDisplay(fireGunImage, 12f, blinkDuration));
                    break;
            }
        }
        
        private void SetPowerUpActive(Image powerUp,bool isActive)
        {
            Color color = isActive ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.15f); 
            powerUp.color = color;
        }
        
        private IEnumerator HandlePowerUpDisplay(Image image, float activeDuration, float blinkingDuration)
        {
            SetPowerUpActive(image, true);
            yield return new WaitForSeconds(activeDuration);

            float blinkInterval = 0.3f;
            float blinkTime = 0f;
            bool visible = true;

            while (blinkTime < blinkingDuration)
            {
                visible = !visible;
                image.color = visible ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.1f);
                yield return new WaitForSeconds(blinkInterval);
                blinkTime += blinkInterval;
            }

            SetPowerUpActive(image, false);
            ActivateDefaultWeapon(settings.defaultWeapon);
        }
    }
}

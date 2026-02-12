using System.Collections;
using RiseOfCathulu.Domains.Utilities.GameHandlers.Scripts;
using RiseOfCathulu.Domains.Weapons.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace RiseOfCathulu.Domains.Utilities.UI.Scripts
{
    public class GamePlayUI : MonoBehaviour
    {
        [Header("Weapon Settings")]
        [SerializeField] private WeaponSettings settings;

        [Header("Player UI")]
        [SerializeField] private Slider scoreMeter; 
        [SerializeField] private int maxScoreValue = 100;
        private Coroutine _meterCoroutine; // Track to prevent overlapping animations

        [Header("Power-Up UI")]
        [SerializeField] private Image freezeImage;
        [SerializeField] private Image timeImage;
        [SerializeField] private Image timeLight;
        [SerializeField] private Image spellGunImage;
        [SerializeField] private Image lightGunImage;
        [SerializeField] private Image fireGunImage;
        [SerializeField] private Image shieldImage;

        [Header("Particles")]
        [SerializeField] private GameObject orangeStarsParticles;

        [Header("Blink Settings")]
        [SerializeField] private float blinkDuration = 3f;

        [Header("Canvas Reference")]
        [SerializeField] private Canvas canvas;

        private Coroutine _weaponCoroutine;

        private void Start()
        {
            InitializeUI();
        }

        private void OnEnable()
        {
            GameEvents.UpdateScoreUI += UpdateScore;
            GameEvents.WeaponCollected += AddWeaponCollected;
            GameEvents.ShieldUpdated += UpdateShield;
            GameEvents.FreezeUI += UpdateFreeze;
        }

        private void OnDisable()
        {
            GameEvents.UpdateScoreUI -= UpdateScore;
            GameEvents.WeaponCollected -= AddWeaponCollected;
            GameEvents.ShieldUpdated -= UpdateShield;
            GameEvents.FreezeUI -= UpdateFreeze;
        }

        private void InitializeUI()
        {
            DeactivateAllPowerUps();
            ActivateDefaultWeapon(settings.defaultWeapon);
        }

        private void DeactivateAllPowerUps()
        {
            DeactivateAllWeapons();
            freezeImage.gameObject.SetActive(false);
            shieldImage.gameObject.SetActive(false);
            //timeImage.gameObject.SetActive(false);
        }

        private void DeactivateAllWeapons()
        {
            if (_weaponCoroutine != null)
            {
                StopCoroutine(_weaponCoroutine);
                _weaponCoroutine = null;
            }
            spellGunImage.gameObject.SetActive(false);
            lightGunImage.gameObject.SetActive(false);
            fireGunImage.gameObject.SetActive(false);
        }

        private void ActivateDefaultWeapon(WeaponType defaultWeapon)
        {
            switch (defaultWeapon)
            {
                case WeaponType.SpellGun: spellGunImage.gameObject.SetActive(true); break;
                case WeaponType.LightGun: lightGunImage.gameObject.SetActive(true); break;
                case WeaponType.FireGun:  fireGunImage.gameObject.SetActive(true); break;
            }
        }

        private void UpdateShield(bool isActive)
        {
            shieldImage.gameObject.SetActive(isActive);
        }

        private void UpdateFreeze(int duration)
        {
            if (gameObject.activeInHierarchy)
                StartCoroutine(HandlePowerUpDisplay(freezeImage, duration - blinkDuration, blinkDuration));
        }
        
        private void UpdateScore(int totalPoints)
        {
            if (scoreMeter != null)
            {
                // Use your maxScoreValue (100) to get the 0-1 ratio
                float targetFill = (float)totalPoints / maxScoreValue;
        
                // Clamp it just in case points briefly exceed max before subtraction
                targetFill = Mathf.Clamp01(targetFill);

                if (_meterCoroutine != null) StopCoroutine(_meterCoroutine);
                _meterCoroutine = StartCoroutine(AnimateBar(targetFill));
            }
        }

        private IEnumerator AnimateBar(float targetFill)
        {
            float startFill = scoreMeter.value;
            float elapsed = 0;
            float duration = 0.15f; 

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                scoreMeter.value = Mathf.Lerp(startFill, targetFill, elapsed / duration);
                yield return null;
            }
            scoreMeter.value = targetFill;

            // FLASH EFFECT: If the bar just filled up (target >= 1)
            if (targetFill >= 0.99f) 
            {
                Image fillImage = scoreMeter.fillRect.GetComponent<Image>();
                Color originalColor = fillImage.color;
                fillImage.color = Color.white; // Flash white
                yield return new WaitForSeconds(0.05f);
                fillImage.color = originalColor; // Back to normal
            }
    
            _meterCoroutine = null;
        }

        private void AddWeaponCollected(WeaponType weaponType)
        {
            DeactivateAllWeapons();

            if (weaponType == settings.defaultWeapon)
            {
                ActivateDefaultWeapon(weaponType);
                return;
            }

            var duration = 12f;
            switch (weaponType)
            {
                case WeaponType.SpellGun: _weaponCoroutine =
                        StartCoroutine(HandlePowerUpDisplay(spellGunImage,  duration, blinkDuration));
                    break;
                case WeaponType.LightGun: _weaponCoroutine = 
                        StartCoroutine(HandlePowerUpDisplay(lightGunImage, duration, blinkDuration));
                    break;
                case WeaponType.FireGun: _weaponCoroutine = 
                        StartCoroutine(HandlePowerUpDisplay(fireGunImage, duration, blinkDuration));
                    break;
            }
        }

        private IEnumerator HandlePowerUpDisplay(Image image,  float activeDuration, float blinkingDuration)
        {
            image.gameObject.SetActive(true);
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

            image.gameObject.SetActive(false);
            ActivateDefaultWeapon(settings.defaultWeapon);
        }
    }
}

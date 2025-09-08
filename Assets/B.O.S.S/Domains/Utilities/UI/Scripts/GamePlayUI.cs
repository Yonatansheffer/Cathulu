using System.Collections;
using B.O.S.S.Domains.Utilities.GameHandlers.Scripts;
using B.O.S.S.Domains.Weapons.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace B.O.S.S.Domains.Utilities.UI.Scripts
{
    public class GamePlayUI : MonoBehaviour
    {
        [Header("Weapon Settings")]
        [SerializeField] private WeaponSettings settings;

        [Header("Boss UI")]
        [SerializeField] private Slider bossHealthBar;

        [Header("Player UI")]
        [SerializeField] private TextMeshProUGUI pointsText;
        [SerializeField] private TextMeshProUGUI timeCountText;
        [SerializeField] private Image[] lifeImages;
        [SerializeField] private Image[] noLifeImages;

        [Header("Power-Up UI")]
        [SerializeField] private Image freezeImage;
        [SerializeField] private Image freezeLight;
        [SerializeField] private Image timeImage;
        [SerializeField] private Image timeLight;
        [SerializeField] private Image spellGunImage;
        [SerializeField] private Image spellGunLight;
        [SerializeField] private Image lightGunImage;
        [SerializeField] private Image lightGunLight;
        [SerializeField] private Image fireGunImage;
        [SerializeField] private Image fireGunLight;
        [SerializeField] private Image shieldImage;
        [SerializeField] private Image shieldLight;

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
            GameEvents.BossLivesChanged += UpdateBossHealth;
            GameEvents.UpdatePlayerLivesUI += UpdatePlayerHealth;
            GameEvents.UpdateScoreUI += UpdateScore;
            GameEvents.UpdateTimeUI += UpdateTimeCount;
            GameEvents.AddTime += UpdateTime;
            GameEvents.WeaponCollected += AddWeaponCollected;
            GameEvents.ShieldUpdated += UpdateShield;
            GameEvents.FreezeUI += UpdateFreeze;
        }

        private void OnDisable()
        {
            GameEvents.UpdateScoreUI -= UpdateScore;
            GameEvents.BossLivesChanged -= UpdateBossHealth;
            GameEvents.UpdatePlayerLivesUI -= UpdatePlayerHealth;
            GameEvents.UpdateTimeUI -= UpdateTimeCount;
            GameEvents.AddTime -= UpdateTime;
            GameEvents.WeaponCollected -= AddWeaponCollected;
            GameEvents.ShieldUpdated -= UpdateShield;
            GameEvents.FreezeUI -= UpdateFreeze;        }

        private void InitializeUI()
        {
            DeactivateAllPowerUps();
            ActivateDefaultWeapon(settings.defaultWeapon);
        }

        private void DeactivateAllPowerUps()
        {
            DeactivateAllWeapons();
            SetPowerUpActive(shieldImage, shieldLight.gameObject, false);
            SetPowerUpActive(freezeImage, freezeLight.gameObject, false);
            SetPowerUpActive(timeImage, timeLight.gameObject, false);
        }

        private void DeactivateAllWeapons()
        {
            if (_weaponCoroutine != null)
            {
                StopCoroutine(_weaponCoroutine);
                _weaponCoroutine = null;
            }
            SetPowerUpActive(spellGunImage, spellGunLight.gameObject, false);
            SetPowerUpActive(lightGunImage, lightGunLight.gameObject, false);
            SetPowerUpActive(fireGunImage, fireGunLight.gameObject, false);
        }

        private void ActivateDefaultWeapon(WeaponType defaultWeapon)
        {
            switch (defaultWeapon)
            {
                case WeaponType.SpellGun: SetPowerUpActive(spellGunImage, spellGunLight.gameObject, true); break;
                case WeaponType.LightGun: SetPowerUpActive(lightGunImage, lightGunLight.gameObject,true); break;
                case WeaponType.FireGun:  SetPowerUpActive(fireGunImage, fireGunLight.gameObject, true);  break;
            }
        }

        private void SetPowerUpActive(Image powerUp, GameObject lightObj, bool isActive)
        {
            powerUp.color = isActive
                ? Color.white
                : new Color(0.5f, 0.5f, 0.5f, 0.25f);

            if (lightObj != null)
                lightObj.SetActive(isActive);
        }

        private void UpdateShield(bool isActive)
        {
            SetPowerUpActive(shieldImage, shieldLight.gameObject, isActive);
        }

        private void UpdateFreeze(int duration)
        {
            if (gameObject.activeInHierarchy)
                StartCoroutine(HandlePowerUpDisplay(freezeImage, freezeLight.gameObject,
                    duration - blinkDuration, blinkDuration));
        }

        private void UpdateTime(float _)
        {
            ShowTimeAddedParticles();
            if (gameObject.activeInHierarchy)
                StartCoroutine(HandlePowerUpDisplay(timeImage, timeLight.gameObject,
                    1f, 0f));
        }

        private void ShowTimeAddedParticles()
        {
            var particles = Instantiate(orangeStarsParticles, timeImage.transform.position, Quaternion.identity, canvas.transform);
            particles.transform.localScale = Vector3.one * 0.5f;
            Destroy(particles, 0.8f);
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

            var duration = 12f;
            switch (weaponType)
            {
                case WeaponType.SpellGun:
                    _weaponCoroutine = StartCoroutine(HandlePowerUpDisplay(
                        spellGunImage, spellGunLight.gameObject, duration, blinkDuration));
                    break;
                case WeaponType.LightGun:
                    _weaponCoroutine = StartCoroutine(HandlePowerUpDisplay(
                        lightGunImage, lightGunLight.gameObject, duration, blinkDuration));
                    break;
                case WeaponType.FireGun:
                    _weaponCoroutine = StartCoroutine(HandlePowerUpDisplay(
                        fireGunImage, fireGunLight.gameObject, duration, blinkDuration));
                    break;
            }
        }

        private IEnumerator HandlePowerUpDisplay(
            Image image, GameObject lightObj, float activeDuration, float blinkingDuration)
        {
            SetPowerUpActive(image, lightObj, true);
            yield return new WaitForSeconds(activeDuration);

            float blinkInterval = 0.3f;
            float blinkTime = 0f;
            bool visible = true;

            while (blinkTime < blinkingDuration)
            {
                visible = !visible;
                image.color = visible ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.1f);
                if (lightObj != null)
                    lightObj.SetActive(visible);

                yield return new WaitForSeconds(blinkInterval);
                blinkTime += blinkInterval;
            }

            SetPowerUpActive(image, lightObj, false);
            ActivateDefaultWeapon(settings.defaultWeapon);
        }
    }
}

using System;
using Weapons;
using System.Collections;
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
        [SerializeField] private TextMeshProUGUI bossHealthText;
        [SerializeField] private TextMeshProUGUI pointsText;
        [SerializeField] private TextMeshProUGUI timeCountText;
        [SerializeField] private TextMeshProUGUI playerHealthText;
        [SerializeField] private Slider playerHealthBar;
        [SerializeField] private TextMeshProUGUI weaponCountdownText;
        [SerializeField] private Image freezeImage;
        [SerializeField] private Image timeImage;
        [SerializeField] private Image spellGunImage;
        [SerializeField] private Image lightGunImage;
        [SerializeField] private Image fireGunImage;
        [SerializeField] private Image shieldImage;
        [SerializeField] private Image[] lifeImages;
        [SerializeField] private Image[] noLifeImages;
        [SerializeField] private int weaponCountdown = 15;
        private WeaponType? _currentWeaponType;
        [SerializeField] private GameObject orangeStarsParticles;
        private Coroutine _countdownCoroutine;

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
            GameEvents.UnFreezeLevel += UpdateFreeze;
        }
        
        private void OnDisable()
        {
            GameEvents.UpdateScoreUI -= UpdateScore;
            GameEvents.BossLivesChanged -= UpdateBossHealth;
            GameEvents.PlayerLivesChanged -= UpdatePlayerHealth;
            GameEvents.UpdateTimeUI -= UpdateTimeCount;
            GameEvents.WeaponCollected -= AddWeaponCollected;
            GameEvents.ShieldUpdated -= UpdateShield;
            GameEvents.FreezeLevel -= UpdateFreeze;
        }
        
        private void DeactivateAllPowerUps()
        {
            SetPowerUpActive(spellGunImage, false);
            SetPowerUpActive(lightGunImage, false);
            SetPowerUpActive(fireGunImage,  false);
            SetPowerUpActive(shieldImage, false);
            SetPowerUpActive(freezeImage, false);
            SetPowerUpActive(timeImage, false);
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
                StartCoroutine(HandlePowerUpDisplay(freezeImage, 7f, 3f));
        }
        
        private void UpdateTime(float dummy)
        {
            var particles = Instantiate(orangeStarsParticles, timeImage.transform.position, Quaternion.identity);
            Destroy(particles, 0.8f);
            if (gameObject.activeInHierarchy)
                StartCoroutine(HandlePowerUpDisplay(timeImage, 1f, 0f)); // No blink, just fade after 1 sec
        }
        
        private void UpdateBossHealth(int amount)
        {
            bossHealthBar.value = amount;
            bossHealthText.text = amount.ToString();
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
            var particles = Instantiate(orangeStarsParticles, timeImage.transform.position, Quaternion.identity);
            Destroy(particles, 0.5f);
            pointsText.text = points.ToString();
        }
        
        private void UpdateTimeCount(int time)
        {
            timeCountText.text = time.ToString();
        }
        
        private void AddWeaponCollected(WeaponType weaponType)
        {
            _currentWeaponType = weaponType;

            // Deactivate all first
            DeactivateAllPowerUps();

            // Check if it's the default weapon → activate immediately without coroutine
            if (weaponType == settings.defaultWeapon)
            {
                ActivateDefaultWeapon(weaponType);
                return;
            }

            // Otherwise → activate with coroutine (12s + 3s blink)
            switch (weaponType)
            {
                case WeaponType.SpellGun:
                    StartCoroutine(HandlePowerUpDisplay(spellGunImage, 12f, 3f));
                    break;
                case WeaponType.LightGun:
                    StartCoroutine(HandlePowerUpDisplay(lightGunImage, 12f, 3f));
                    break;
                case WeaponType.FireGun:
                    StartCoroutine(HandlePowerUpDisplay(fireGunImage, 12f, 3f));
                    break;
            }
        }
        
        private void SetPowerUpActive(Image powerUp,bool isActive)
        {
            Color color = isActive ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.4f); // grey + transparent
            powerUp.color = color;
        }
        
        private IEnumerator HandlePowerUpDisplay(Image image, float activeDuration, float blinkDuration)
        {
            SetPowerUpActive(image, true);

            // Wait for active duration
            yield return new WaitForSeconds(activeDuration);

            // Blink phase
            float blinkInterval = 0.3f;
            float blinkTime = 0f;
            bool visible = true;

            while (blinkTime < blinkDuration)
            {
                visible = !visible;
                image.color = visible ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.1f);
                yield return new WaitForSeconds(blinkInterval);
                blinkTime += blinkInterval;
            }

            // Deactivate after blinking
            SetPowerUpActive(image, false);

            // Reactivate the default weapon
            ActivateDefaultWeapon(settings.defaultWeapon);
        }

                
        /*
        private IEnumerator WeaponCountdownCoroutine(int seconds)
        {
            var timeLeft = seconds;
            while (timeLeft > 0)
            {
                weaponCountdownText.text = timeLeft.ToString();
                yield return new WaitForSeconds(1f);
                timeLeft--;
            }
            weaponCountdownText.gameObject.SetActive(false);
            spellGunImage.gameObject.SetActive(false);
            lightGunImage.gameObject.SetActive(false);
            fireGunImage.gameObject.SetActive(false);
            AddWeaponCollected(settings.defaultWeapon);
        }
        */




        /*private void RemoveLastWeapon()
        {
            switch (_currentWeaponType)
            {
                case null:
                    return;
                case WeaponType.SpellGun:
                    spellGunImage.gameObject.SetActive(false);
                    break;
                case WeaponType.LightGun:
                    lightGunImage.gameObject.SetActive(false);
                    break;
                case WeaponType.FireGun:
                    fireGunImage.gameObject.SetActive(false);
                    break;
            }
        }*/

        /*
        [SerializeField] private Image readyText;
        [SerializeField] private TextMeshProUGUI timeDisplayText;
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private TextMeshProUGUI locationText;
        [SerializeField] private Image life1;
        [SerializeField] private Image life2;
        [SerializeField] private Image life2;
        [SerializeField] private Image stickyHarpoonCollected;
        [SerializeField] private Image lightGunCollected;
        [SerializeField] private Image doubleHarpoonCollected;
        [SerializeField] private float blinkInterval = 0.2f; // Interval for the ready text to blink
        private Coroutine _blinkCoroutine;
        
        private void Awake()
        {
            HideGameScreen();
            InitializeCollectables();
        }

        private void InitializeCollectables()
        {
            stickyHarpoonCollected.gameObject.SetActive(false);
            lightGunCollected.gameObject.SetActive(false);
            doubleHarpoonCollected.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            GameEvents.DefaultWeapon += InitializeCollectables;
            GameEvents.ResetWeaponUI += InitializeCollectables;
            GameEvents.HideGameUI += HideGameScreen;
            GameEvents.UpdateTimeUI += UpdateTime;
            GameEvents.ReadyStage += ShowReadyText;
            GameEvents.WeaponCollected += AddWeaponCollected;
            GameEvents.GameOver += HideScore;
            GameEvents.StartGame += ShowGameScreen;
        }

        private void OnDisable()
        {
            GameEvents.DefaultWeapon -= InitializeCollectables;
            GameEvents.ResetWeaponUI -= InitializeCollectables;
            GameEvents.HideGameUI -= HideGameScreen;
            GameEvents.UpdateLifeUI -= UpdateLife;
            GameEvents.UpdatePointsUI -= UpdatePoints;
            GameEvents.UpdateTimeUI -= UpdateTime;
            GameEvents.ReadyStage -= ShowReadyText;
            GameEvents.WeaponCollected -= AddWeaponCollected;
            GameEvents.GameOver -= HideScore;
            GameEvents.StartGame -= ShowGameScreen;
        }
        
        private void UpdateTime(int time)
        {
            timeDisplayText.text = time.ToString();
        }
        
        

        private void ShowReadyText()
        {
            readyText.gameObject.SetActive(true);
            if (_blinkCoroutine != null)
            {
                StopCoroutine(_blinkCoroutine);
            }
            _blinkCoroutine = StartCoroutine(BlinkText(readyText));
            Invoke(nameof(HideReadyText), 1f); 
        }

        private IEnumerator BlinkText(Image text)
        {
            while (true)
            {
                text.enabled = !text.enabled;
                yield return new WaitForSeconds(blinkInterval);
            }
        }

        private void HideReadyText()
        {
            if (_blinkCoroutine != null)
            {
                StopCoroutine(_blinkCoroutine);
                _blinkCoroutine = null;
            }
            readyText.gameObject.SetActive(false);
            GameEvents.StartStage?.Invoke(); // Start the stage
        }


        
        private void HideScore()
        {
            pointsText.gameObject.SetActive(false);
        }*/
        
    }
}

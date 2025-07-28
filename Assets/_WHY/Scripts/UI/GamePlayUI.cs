using System;
using Weapons;
using System.Collections;
using GameHandlers;
using TMPro;
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
        [SerializeField] private Image spellGunImage;
        [SerializeField] private Image lightGunImage;
        [SerializeField] private Image fireGunImage;
        [SerializeField] private Image shieldImage;
        [SerializeField] private int weaponCountdown = 15;
        private WeaponType? _currentWeaponType;
        private Coroutine _countdownCoroutine;


        private void Start()
        {
            AddWeaponCollected(settings.defaultWeapon);
        }

        private void OnEnable()
        {
            GameEvents.BossLivesChanged += UpdateHealth;
            GameEvents.UpdateScoreUI += UpdateScore;
            GameEvents.BossLivesChanged += UpdateHealth;
            GameEvents.UpdateTimeUI += UpdateTime;
            GameEvents.WeaponCollected += AddWeaponCollected;
            GameEvents.ShieldUpdated += UpdateShield;
        }
        
        private void OnDisable()
        {
            GameEvents.UpdateScoreUI -= UpdateScore;
            GameEvents.BossLivesChanged -= UpdateHealth;
            GameEvents.UpdateTimeUI -= UpdateTime;
            GameEvents.WeaponCollected -= AddWeaponCollected;
            GameEvents.ShieldUpdated -= UpdateShield;
        }
        
        private void UpdateShield(bool isActive)
        {
            shieldImage.gameObject.SetActive(isActive);
        }
        
        private void UpdateHealth(int amount)
        {
            bossHealthBar.value = amount;
            bossHealthText.text = amount.ToString();
        }
        
        private void UpdateScore(int points)
        {
            pointsText.text = points.ToString();
        }
        
        private void UpdateTime(int time)
        {
            timeCountText.text = $"Time: {time}";
        }
        
        private void AddWeaponCollected(WeaponType weaponType)
        {
            RemoveLastWeapon();
            switch (weaponType)
            {
                case WeaponType.SpellGun:
                    spellGunImage.gameObject.SetActive(true);
                    _currentWeaponType = WeaponType.SpellGun;
                    break;
                case WeaponType.LightGun:
                    lightGunImage.gameObject.SetActive(true);
                    _currentWeaponType = WeaponType.LightGun;
                    break;
                case WeaponType.FireGun:
                    fireGunImage.gameObject.SetActive(true);
                    _currentWeaponType = WeaponType.FireGun;
                    break;
            }
            if (_currentWeaponType == settings.defaultWeapon) return;
            weaponCountdownText.gameObject.SetActive(true);
            if (_countdownCoroutine != null)
                StopCoroutine(_countdownCoroutine);
            _countdownCoroutine = StartCoroutine(WeaponCountdownCoroutine(weaponCountdown));
        }
        
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


        private void RemoveLastWeapon()
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
        }

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

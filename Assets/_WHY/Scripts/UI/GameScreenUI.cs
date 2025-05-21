using System;
using Weapons;
using System.Collections;
using GameHandlers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class GameScreenUI : MonoBehaviour
    {
        [SerializeField] private Slider BossHealthBar;
        [SerializeField] private TextMeshProUGUI pointsText;
        [SerializeField] private TextMeshProUGUI playerLifeText;

        private void Awake()
        {
            HideGameScreen();
            //InitializeCollectables();
        }
        
        private void Start()
        {
            UpdateBossHealth(GameManager.GetInitialBossHealth());
        }

        private void OnEnable()
        {
            GameEvents.UpdatePointsUI += UpdatePoints;
            GameEvents.UpdateLifeUI += UpdateLife;
            GameEvents.StartGame += ShowGameScreen;
            GameEvents.BossDamaged += UpdateBossHealth;
        }
        
        private void OnDisable()
        {
            GameEvents.UpdatePointsUI -= UpdatePoints;
            GameEvents.UpdateLifeUI -= UpdateLife;
            GameEvents.StartGame -= ShowGameScreen;
            GameEvents.BossDamaged -= UpdateBossHealth;
        }
        
        private void UpdateBossHealth(int newHealth)
        {
            BossHealthBar.value = newHealth;
        }
        
        private void UpdatePoints(int points)
        {
            pointsText.text = points.ToString();
        }
        
        private void UpdateLife(int numLives)
        {
            playerLifeText.text = numLives.ToString();

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
        
        private void AddWeaponCollected(WeaponType weaponType)
        {
            InitializeCollectables();
            switch (weaponType)
            {
                case WeaponType.StickyHarpoon:
                    stickyHarpoonCollected.gameObject.SetActive(true);
                    break;
                case WeaponType.LightGun:
                    lightGunCollected.gameObject.SetActive(true);
                    break;
                case WeaponType.DoubleHarpoon:
                    doubleHarpoonCollected.gameObject.SetActive(true);
                    break;
            }
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
        
        private void HideGameScreen()
        {
            /*timeText.gameObject.SetActive(false);
            timeDisplayText.gameObject.SetActive(false);
            player1Text.gameObject.SetActive(false);
            player2Text.gameObject.SetActive(false);
            life1.gameObject.SetActive(false);
            life2.gameObject.SetActive(false);
            locationText.gameObject.SetActive(false);
            stageText.gameObject.SetActive(false);
            hiText.gameObject.SetActive(false);*/
            pointsText.gameObject.SetActive(false);
            BossHealthBar.gameObject.SetActive(false);
        }
        
        private void ShowGameScreen()
        {
            /*timeText.gameObject.SetActive(true);
            timeDisplayText.gameObject.SetActive(true);
            player1Text.gameObject.SetActive(true);
            player2Text.gameObject.SetActive(true);
            locationText.gameObject.SetActive(true);
            stageText.gameObject.SetActive(true);
            hiText.gameObject.SetActive(true);*/
            pointsText.gameObject.SetActive(true);
            BossHealthBar.gameObject.SetActive(true);
        }
    }
}

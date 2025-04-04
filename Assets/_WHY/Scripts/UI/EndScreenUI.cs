using System.Collections;
using GameHandlers;
using Sound;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class EndScreenUI : MonoBehaviour
    {/*
        [SerializeField] private TextMeshProUGUI stage1Text;
        [SerializeField] private TextMeshProUGUI timeBonusText;
        [SerializeField] private TextMeshProUGUI nextExtendText;
        [SerializeField] private TextMeshProUGUI pressEnterText;
        [SerializeField] private Image endScreen;
        [SerializeField] private Image timeOver;
        [SerializeField] private TextMeshProUGUI continueText;
        [SerializeField] private TextMeshProUGUI gameOverCountdownText;
        [SerializeField] private TextMeshProUGUI gameOverText;
        [SerializeField] private Image gameOverImage;
        private bool _canPressEnter; // Flag to know whether the player can press enter
        private Coroutine _blinkCoroutine;
        private Coroutine _countdownCoroutine;

        private void Awake()
        {
            _canPressEnter = false;
            OnRestartStage();
        }

        private void OnEnable()
        {
            GameEvents.BeginGamePlay += OnRestartStage;
            GameEvents.StartStage += OnRestartStage;
            GameEvents.PassedStage += ShowPassedStageScreen;
            GameEvents.TimeOver += ShowTimeOverText;
            GameEvents.GameOver += ShowGameOver;
        }
        
        private void OnDisable()
        {
            GameEvents.BeginGamePlay -= OnRestartStage;
            GameEvents.StartStage -= OnRestartStage;
            GameEvents.PassedStage -= ShowPassedStageScreen;
            GameEvents.TimeOver -= ShowTimeOverText;
            GameEvents.GameOver -= ShowGameOver;
        }

        private void Update()
        {
            CheckForEnter();
        }
        
        private void OnRestartStage()
        {
            HideEndScreen();
            _canPressEnter = false;
        }

        private void ShowPassedStageScreen(int timeBonus, int points)
        {
            stage1Text.gameObject.SetActive(true);
            endScreen.gameObject.SetActive(true);
            timeBonusText.gameObject.SetActive(true);
            timeBonusText.text = $"TIME BONUS  {timeBonus} PTS.";
            StartCoroutine(ShowScore(points));
        }

        private IEnumerator ShowScore(int points)
        {
            yield return new WaitForSeconds(1f);
            nextExtendText.gameObject.SetActive(true);
            nextExtendText.text = $"POINTS COLLECTED  {points}.";
            yield return new WaitForSeconds(5f);
            Application.Quit();
        }
                
        private void ShowTimeOverText()
        {
            timeOver.gameObject.SetActive(true);
            StartCoroutine(WaitAndProceed());
        }

        private IEnumerator WaitAndProceed()
        {
            yield return new WaitForSeconds(3);
            timeOver.gameObject.SetActive(false);
            GameEvents.PlayerLostLife?.Invoke();
        }
        
        private void ShowGameOver()
        {
            pressEnterText.gameObject.SetActive(true);
            _blinkCoroutine = StartCoroutine(BlinkPressEnterText());
            continueText.gameObject.SetActive(true);
            _canPressEnter = true;
            gameOverCountdownText.gameObject.SetActive(true);
            _countdownCoroutine = StartCoroutine(CountDown());
        }
        
        private IEnumerator BlinkPressEnterText()
        {
            while (true) 
            {
                pressEnterText.enabled = !pressEnterText.enabled;
                yield return new WaitForSeconds(0.5f); 
            }
        }

        
        private IEnumerator CountDown()
        {
            var count = 9;
            while (count >= 0)
            {
                gameOverCountdownText.text = count.ToString();
                yield return new WaitForSeconds(1);
                count--;
            }
            StopCoroutine(_blinkCoroutine);
            continueText.gameObject.SetActive(false);
            gameOverCountdownText.gameObject.SetActive(false);
            pressEnterText.gameObject.SetActive(false);
            gameOverText.gameObject.SetActive(true);
            gameOverImage.gameObject.SetActive(true);
            SoundManager.Instance.PlaySound("Game Over", transform);
            _canPressEnter = false;
            yield return new WaitForSeconds(7);
            StopCoroutine(_countdownCoroutine);
            GameEvents.BeginGamePlay?.Invoke();
        }
        
        private void CheckForEnter()
        {
            if (Input.GetKeyDown(KeyCode.Return) && _canPressEnter)
            {
                StopAllCoroutines();
                HideEndScreen();
                _canPressEnter = false;                
                GameEvents.StartGame?.Invoke();
            }
        }

        private void HideEndScreen()
        { 
            stage1Text.gameObject.SetActive(false);
            continueText.gameObject.SetActive(false);
            gameOverText.gameObject.SetActive(false);
            gameOverImage.gameObject.SetActive(false);
            gameOverCountdownText.gameObject.SetActive(false);
            pressEnterText.gameObject.SetActive(false);
            timeOver.gameObject.SetActive(false);
            endScreen.gameObject.SetActive(false);
            timeBonusText.gameObject.SetActive(false);
            nextExtendText.gameObject.SetActive(false);
        }*/
    }
}
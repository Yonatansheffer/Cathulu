using System.Collections;
using _WHY.Domains.Utilities.Sound.Scripts;
using UnityEngine;

namespace _WHY.Domains.Utilities.GameHandlers.Scripts
{
    public enum GameState { Playing, Defeated, InFreeze, TimeOver, PlayerWon }

    public class GameLoopManager : MonoBehaviour
    {
        [Header("Timer")]
        [SerializeField, Tooltip("Starting countdown time (seconds)")]
        private float initialCountDownTime = 200f;

        [Header("Freeze Power-Up")]
        [SerializeField, Tooltip("Duration of stage freeze (seconds)")]
        private int freezeDuration = 6;

        private float _countDownTime;
        private int _currentScore;
        private int _timeBonus;
        private bool _isCountingDown;
        private bool _isTimeOver;
        private GameState _currentGameState;

        private void Start()
        {
            DontDestroyOnLoad(this);
            OnLevelStart();
        }

        private void OnEnable()
        {
            GameEvents.AddTime += AddTime;
            GameEvents.FreezeCollected += OnFreeze;
            GameEvents.RestartLevel += OnLevelStart;
            GameEvents.BossDestroyed += UpdatePlayerWin;
            GameEvents.BossEndedDeath += PlayerWon;
            GameEvents.PlayerDefeated += UpdateDefeatedGameState;
            GameEvents.AddPoints += AddPoints;
        }

        private void OnDisable()
        {
            GameEvents.AddTime -= AddTime;
            GameEvents.FreezeCollected -= OnFreeze;
            GameEvents.RestartLevel -= OnLevelStart;
            GameEvents.BossDestroyed -= UpdatePlayerWin;
            GameEvents.BossEndedDeath -= PlayerWon;
            GameEvents.PlayerDefeated -= UpdateDefeatedGameState;
            GameEvents.AddPoints -= AddPoints;
        }

        private void Update()
        {
            if (!_isCountingDown) return;
            UpdateCountdown();
            GameEvents.UpdateTimeUI?.Invoke(Mathf.FloorToInt(_countDownTime));
        }

        private void OnLevelStart()
        {
            _currentGameState = GameState.Playing;
            ResetStats();
            ResetCountDown();
            GameEvents.UpdateScoreUI?.Invoke(_currentScore);
            GameEvents.UpdateTimeUI?.Invoke(Mathf.FloorToInt(_countDownTime));
        }

        private void ResetStats()
        {
            _currentScore = 0;
            _timeBonus = 0;
        }

        private void ResetCountDown()
        {
            _isCountingDown = false;
            _countDownTime = initialCountDownTime;
            _isCountingDown = true;
            _isTimeOver = false;
        }

        private void AddPoints(int pointsToAdd)
        {
            _currentScore += pointsToAdd;
            GameEvents.UpdateScoreUI?.Invoke(_currentScore);
        }

        private void UpdatePlayerWin()
        {
            GameEvents.FreezeLevel?.Invoke();
            _timeBonus = Mathf.FloorToInt(_countDownTime) * 10;
            AddPoints(_timeBonus);
            _currentGameState = GameState.PlayerWon;
        }

        private void OnFreeze()
        {
            if (_currentGameState == GameState.InFreeze) return;
            _currentGameState = GameState.InFreeze;
            StartCoroutine(FreezeCoroutine());
        }

        private IEnumerator FreezeCoroutine()
        {
            _isCountingDown = false;
            GameEvents.FreezeLevel?.Invoke();
            GameEvents.FreezeUI?.Invoke(freezeDuration);
            SoundManager.Instance.PlaySound("Freeze", transform);
            yield return new WaitForSeconds(freezeDuration);
            GameEvents.UnFreezeLevel?.Invoke();
            _isCountingDown = true;
            _currentGameState = GameState.Playing;
        }

        private void AddTime(float timeToAdd)
        {
            _countDownTime = Mathf.Max(0f, _countDownTime + timeToAdd);
        }

        private void AddTime(int timeToAdd)
        {
            _countDownTime = Mathf.Max(0f, _countDownTime + timeToAdd);
        }

        private void UpdateCountdown()
        {
            if (_countDownTime > 0f)
            {
                _countDownTime = Mathf.Max(0f, _countDownTime - Time.deltaTime);
                GameEvents.UpdateTimeUI?.Invoke(Mathf.FloorToInt(_countDownTime));
                return;
            }

            _isCountingDown = false;
            _isTimeOver = true;
            GameEvents.PlayerDefeated?.Invoke();
        }

        private void UpdateDefeatedGameState()
        {
            _currentGameState = _isTimeOver ? GameState.TimeOver : GameState.Defeated;
            GameEvents.FreezeLevel?.Invoke();
            StartCoroutine(EndScene());
        }

        private void PlayerWon()
        {
            _currentGameState = GameState.PlayerWon;
            StartCoroutine(EndScene());
        }

        private IEnumerator EndScene()
        {
            yield return new WaitForSeconds(1.7f);
            GameEvents.EndScene?.Invoke();
            yield return new WaitForSeconds(0.3f);
            GameEvents.GameOverUI?.Invoke(_currentGameState, _currentScore);
        }
    }
}

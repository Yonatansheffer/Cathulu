using System.Collections;
using Sound;
using UnityEngine;

namespace GameHandlers
{
    public enum GameState
    {
        Playing,
        Defeated,
        TimeOver,
        PlayerWon
    }
    
    public class GameLoopManager : MonoBehaviour
    {
        [SerializeField] private float initialCountDownTime = 1000f; // Initial countdown time in seconds
        private float _countDownTime; // Stores the current time left
        private int _currentScore; // Stores the current points
        private int _timeBonus; // time Bonus to be added to the score at the end of the stage
        private bool _isCountingDown; // Flag to know if the countdown is active
        private GameState _currentGameState; // Stores the current game state
        
        private void Start()
        {
            DontDestroyOnLoad(this);
            OnLevelStart();
        }
        
        private void OnEnable()
        {
            GameEvents.AddTime += AddTime;  
            GameEvents.FreezeLevel += StopCountDown;
            GameEvents.RestartLevel += OnLevelStart;
            GameEvents.BossDestroyed += PlayerWin;
            GameEvents.GameOver += OnGameOver;
            GameEvents.AddPoints += AddPoints;
        }
    
        private void OnDisable()
        {
            GameEvents.AddTime -= AddTime;
            GameEvents.FreezeLevel -= StopCountDown;
            GameEvents.RestartLevel -= OnLevelStart;
            GameEvents.BossDestroyed -= PlayerWin;
            GameEvents.GameOver -= OnGameOver;
            GameEvents.AddPoints -= AddPoints;
        }
        
        private void Update()
        {
            if (!_isCountingDown) 
                return;
            UpdateCountdown();
            GameEvents.UpdateTimeUI?.Invoke(Mathf.FloorToInt(_countDownTime));
        }
    
        private void OnLevelStart()
        {
            _currentGameState = GameState.Playing;
            ResetCountDown();
            ResetStats();
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
            StopCountDown();
            _countDownTime = initialCountDownTime;
            _isCountingDown = true;
        }
        
        private void AddPoints(int pointsToAdd)
        {
            _currentScore += pointsToAdd;
            GameEvents.UpdateScoreUI?.Invoke(_currentScore);
        }

        private void PlayerWin()
        {
            _timeBonus = Mathf.FloorToInt(_countDownTime) * 30;
            AddPoints(_timeBonus);
            _currentGameState = GameState.PlayerWon;
            GameEvents.FreezeLevel?.Invoke();
            GameEvents.GameOverUI?.Invoke(_currentGameState, _currentScore);
        }

        private void StopCountDown()
        {
            _isCountingDown = false;
        }
        
        private void AddTime(float timeToAdd)
        {
            _countDownTime += timeToAdd;
            _countDownTime = Mathf.Max(0, _countDownTime);
        }

        private void UpdateCountdown()
        {
            if (_countDownTime > 0)
            {
                _countDownTime -= Time.deltaTime;
                GameEvents.UpdateTimeUI?.Invoke(Mathf.FloorToInt(_countDownTime));
                _countDownTime = Mathf.Max(0, _countDownTime);
            }
            else
            {
                _currentGameState = GameState.TimeOver;
                StartCoroutine(HandleGameOverSequence());
            }
        }
    
        private void OnGameOver()
        {
            _currentGameState = GameState.Defeated;
            StartCoroutine(HandleGameOverSequence());
        }

        private IEnumerator HandleGameOverSequence()
        {
            GameEvents.FreezeLevel?.Invoke();
            yield return new WaitForSeconds(2f); 
            GameEvents.EndScene?.Invoke();
            yield return new WaitForSeconds(0.3f); 
            GameEvents.GameOverUI?.Invoke(_currentGameState, _currentScore);
        }
        
        private void AddTime(int timeToAdd)
        {
            _countDownTime += timeToAdd;
        }
    }
}
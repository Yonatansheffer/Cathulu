using System.Collections;
using Sound;
using UnityEngine;

namespace GameHandlers
{
    public enum GameState
    {
        Playing,
        Defeated,
        InFreeze,
        TimeOver,
        PlayerWon
    }
    
    public class GameLoopManager : MonoBehaviour
    {
        [SerializeField] private float initialCountDownTime = 200f; // Initial countdown time in seconds
        private float _countDownTime; // Stores the current time left
        private int _currentScore; // Stores the current points
        private int _timeBonus; // time Bonus to be added to the score at the end of the stage
        private bool _isCountingDown; // Flag to know if the countdown is active
        private GameState _currentGameState; // Stores the current game state
        [SerializeField] private const float FreezeDuration = 10f;
        
        public static float GetFreezeDuration() => FreezeDuration;

        
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
            _isCountingDown = false;
            _countDownTime = initialCountDownTime;
            _isCountingDown = true;
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
            if (_currentGameState == GameState.InFreeze)
            {
                print("retin");
                return;
            }
            _currentGameState = GameState.InFreeze;
            print("in");
            StartCoroutine(FreezeCoroutine());
        }
        
        private IEnumerator FreezeCoroutine()
        {
            _isCountingDown = false;
            GameEvents.FreezeLevel?.Invoke();
            GameEvents.FreezeUI?.Invoke();
            yield return new WaitForSeconds(FreezeDuration);
            print("rddddetin");
            GameEvents.UnFreezeLevel?.Invoke();
            _isCountingDown = true;
            _currentGameState = GameState.Playing;
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
                GameEvents.FreezeLevel?.Invoke();
                StartCoroutine(EndScene());
            }
        }
    
        private void UpdateDefeatedGameState()
        {
            _currentGameState = GameState.Defeated;
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

        private void AddTime(int timeToAdd)
        {
            _countDownTime += timeToAdd;
        }
    }
}
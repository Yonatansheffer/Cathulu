using UnityEngine;

namespace GameHandlers
{
    public class GameLoopManager : MonoBehaviour
    {
        [SerializeField] private float initialCountDownTime = 1000f; // Initial countdown time in seconds
        private float _countDownTime; // Stores the current time left
        private int _currentPoints; // Stores the current points
        private int _timeBonus; // time Bonus to be added to the score at the end of the stage
        private bool _isCountingDown; // Flag to know if the countdown is active

        private void Awake()
        {
            ResetStats();
            ResetCountDown();
            DontDestroyOnLoad(this);
        }

        private void OnEnable()
        {
            GameEvents.AddTime += AddTime;  
            GameEvents.FreezeLevel += StopCountDown;
            GameEvents.RestartLevel += OnStageStart;
            GameEvents.BossDestroyed += PassedStage;
            GameEvents.PlayerLivesChanged += CheckGameOver;
            GameEvents.BossLivesChanged += CheckGameOver;
            GameEvents.AddPoints += AddPoints;
        }
    
        private void OnDisable()
        {
            GameEvents.AddTime -= AddTime;
            GameEvents.FreezeLevel -= StopCountDown;
            GameEvents.RestartLevel -= OnStageStart;
            GameEvents.BossDestroyed -= PassedStage;
            GameEvents.PlayerLivesChanged -= CheckGameOver;
            GameEvents.BossLivesChanged -= CheckGameOver;
            GameEvents.AddPoints -= AddPoints;
        }
        
        private void Update()
        {
            if (!_isCountingDown) 
                return;
            UpdateCountdown();
            GameEvents.UpdateTimeUI?.Invoke(Mathf.FloorToInt(_countDownTime));
        }
    
        private void ResetStats()
        {
            _currentPoints = 0;
            _timeBonus = 0;
        }

        private void OnStageStart()
        {
            ResetCountDown();
            GameEvents.UpdatePointsUI?.Invoke(_currentPoints);
            GameEvents.UpdateTimeUI?.Invoke(Mathf.FloorToInt(_countDownTime));
            _isCountingDown = true;
        }
    
        private void AddPoints(int pointsToAdd)
        {
            _currentPoints += pointsToAdd;
            GameEvents.UpdatePointsUI?.Invoke(_currentPoints);
        }

        private void PassedStage()
        {
            _timeBonus = Mathf.FloorToInt(_countDownTime) * 30;
            GameEvents.FreezeLevel?.Invoke();
            GameEvents.HideGameUI?.Invoke();
            GameEvents.PassedStage?.Invoke(_timeBonus, _currentPoints);
        }
    
        private void ResetCountDown()
        {
            StopCountDown();
            _countDownTime = initialCountDownTime;
        }

        private void StopCountDown()
        {
            _isCountingDown = false;
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
                GameEvents.FreezeLevel?.Invoke();
                GameEvents.GameOver.Invoke(false);
            }
        }
    
        private void CheckGameOver(int lives) 
        { 
            print(lives);   
            if (lives <= 0)
                GameEvents.GameOver?.Invoke(false);
        }
    
        private void AddTime(int timeToAdd)
        {
            _countDownTime += timeToAdd;
        }
    
        
    }
}
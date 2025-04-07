using UnityEngine;

namespace GameHandlers
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private float initialCountDownTime = 1000f; // Initial countdown time in seconds
        [SerializeField] private const int InitialBossHealth = 100;
        [SerializeField] private int initialPlayerLives = 3;
        private float _countDownTime; // Stores the current time left
        private int _currentPoints; // Stores the current points
        private int _timeBonus; // time Bonus to be added to the score at the end of the stage
        private bool _isCountingDown; // Flag to know if the countdown is active
        private int _currentLives; // Stores the current lives of the player

        private void Awake()
        {
            ResetStats();
            ResetCountDown();
            StopCountDown();
        }

        private void Start()
        {
            GameEvents.BeginGamePlay?.Invoke(); 
        }
        
        public static int GetInitialBossHealth()
        {
            return InitialBossHealth;
        }

        private void OnEnable()
        {
            GameEvents.AddLife += AddLife;
            GameEvents.AddTime += AddTime;  
            GameEvents.FreezeStage += StopCountDown;
            GameEvents.StartGame += OnGameStart;
            GameEvents.PlayerLostLife += ReducePlayerLife;
            GameEvents.StartStage += OnStageStart;
            GameEvents.BossDestroyed += PassedStage;
            GameEvents.AddPoints += AddPoints;
        }
    
        private void OnDisable()
        {
            GameEvents.AddLife -= AddLife;
            GameEvents.AddTime -= AddTime;
            GameEvents.FreezeStage -= StopCountDown;
            GameEvents.StartGame -= OnGameStart;
            GameEvents.PlayerLostLife -= ReducePlayerLife;
            GameEvents.StartStage -= OnStageStart;
            GameEvents.BossDestroyed -= PassedStage;
            GameEvents.AddPoints -= AddPoints;
        }

        private void OnGameStart()
        {
            ResetStats();
            GameEvents.StartStage?.Invoke();
        }
    
        private void ResetStats()
        {
            _currentLives = initialPlayerLives;
            _currentPoints = 0;
            _timeBonus = 0;
        }

        private void OnStageStart()
        {
            ResetCountDown();
            GameEvents.UpdatePointsUI?.Invoke(_currentPoints);
            GameEvents.UpdateLifeUI?.Invoke(_currentLives);
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
            GameEvents.FreezeStage?.Invoke();
            GameEvents.HideGameUI?.Invoke();
            GameEvents.PassedStage?.Invoke(_timeBonus, _currentPoints);
        }
    
        private void Update()
        {
            if (!_isCountingDown) 
                return;
            UpdateCountdown();
            GameEvents.UpdateTimeUI?.Invoke(Mathf.FloorToInt(_countDownTime));
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
                _countDownTime = Mathf.Max(0, _countDownTime);
            }
            else
            {
                GameEvents.FreezeStage?.Invoke();
                GameEvents.TimeOver.Invoke();
            }
        }
    
        private void ReducePlayerLife()
        {
            _currentLives--;
            CheckGameOver();
        }
    
        private void CheckGameOver() {
            if (_currentLives <= 0)
                GameEvents.GameOver?.Invoke();
            else
            {
                GameEvents.ReadyStage?.Invoke();
                GameEvents.UpdateLifeUI?.Invoke(_currentLives);
            }
        }
    
        private void AddTime(int timeToAdd)
        {
            _countDownTime += timeToAdd;
        }
    
        private void AddLife()
        {
            if (_currentLives >= initialPlayerLives)
                return;
            _currentLives++;
            GameEvents.UpdateLifeUI?.Invoke(_currentLives);
        }
    }
}
using System.Collections;
using RiseOfCathulu.Domains.Utilities.Sound.Scripts;
using UnityEngine;

namespace RiseOfCathulu.Domains.Utilities.GameHandlers.Scripts
{
    public enum GameState { Opening, Playing, Defeated, InFreeze, PlayerWon }

    public class GameLoopManager : MonoBehaviour
    {
        [SerializeField, Tooltip("Duration of stage freeze (seconds)")] private int freezeDuration = 6;
        [SerializeField, Tooltip("Amount of points to Grow")] private int growPrice = 100;
        private float _countDownTime;
        private int _currentScore;
        private GameState _currentGameState = GameState.Opening;

        private void Start()
        {
            DontDestroyOnLoad(this);
            OnLevelStart();
        }

        private void OnEnable()
        {
            GameEvents.FreezeCollected += OnFreeze;
            GameEvents.RestartLevel += OnLevelStart;
            GameEvents.DestroyedSun += UpdatePlayerWin;
            GameEvents.PlanetEnemyEndedDeath += PlayerWon;
            GameEvents.PlayerDefeated += UpdateDefeatedGameState;
            GameEvents.PlayerRequestedSizeIncrease += TryIncreaseSize;
            GameEvents.AddPoints += AddPoints;
        }

        private void OnDisable()
        {
            GameEvents.FreezeCollected -= OnFreeze;
            GameEvents.RestartLevel -= OnLevelStart;
            GameEvents.DestroyedSun -= UpdatePlayerWin;
            GameEvents.PlanetEnemyEndedDeath -= PlayerWon;
            GameEvents.PlayerDefeated -= UpdateDefeatedGameState;
            GameEvents.PlayerRequestedSizeIncrease -= TryIncreaseSize;
            GameEvents.AddPoints -= AddPoints;
        }

        private void OnLevelStart()
        {
            _currentGameState = GameState.Playing;
            _currentScore = 0;
            GameEvents.UpdateScoreUI?.Invoke(_currentScore);
        }

       
        private void TryIncreaseSize()
        {
            if (_currentGameState is not (GameState.Playing or GameState.InFreeze or GameState.Opening)) return;
            if(_currentScore < growPrice) return;
            _currentScore -= growPrice;
            GameEvents.UpdateScoreUI?.Invoke(_currentScore);
            GameEvents.PlayerGrow?.Invoke(1);
        }
        
        private void AddPoints(int pointsToAdd)
        {
            _currentScore += pointsToAdd;
            GameEvents.UpdateScoreUI?.Invoke(_currentScore);
        }

        private void UpdatePlayerWin()
        {
            GameEvents.FreezeLevel?.Invoke();
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
            GameEvents.FreezeLevel?.Invoke();
            GameEvents.FreezeUI?.Invoke(freezeDuration);
            SoundManager.Instance.PlaySound("Freeze", transform);
            yield return new WaitForSeconds(freezeDuration);
            GameEvents.UnFreezeLevel?.Invoke();
            _currentGameState = GameState.Playing;
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
    }
}

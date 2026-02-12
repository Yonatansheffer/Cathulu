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
        [SerializeField] private bool isOpening;
        private float _countDownTime;
        private int _currentScore;
        private int _totalScore;
        private GameState _currentGameState = GameState.Opening;
        

        private void Start()
        {
            if (!isOpening)
            {
                DontDestroyOnLoad(this);
            }
            OnLevelStart();
        }

        private void OnEnable()
        {
            GameEvents.FreezeCollected += OnFreeze;
            GameEvents.RestartLevel += OnLevelStart;
            GameEvents.DestroyedSun += UpdatePlayerWin;
            GameEvents.PlayerDefeated += UpdateDefeatedGameState;
            GameEvents.AddPoints += AddPoints;
        }

        private void OnDisable()
        {
            GameEvents.FreezeCollected -= OnFreeze;
            GameEvents.RestartLevel -= OnLevelStart;
            GameEvents.DestroyedSun -= UpdatePlayerWin;
            GameEvents.PlayerDefeated -= UpdateDefeatedGameState;
            GameEvents.AddPoints -= AddPoints;
        }

        private void OnLevelStart()
        {
            _currentGameState = GameState.Playing;
            GameEvents.UpdateScoreUI?.Invoke(_currentScore);
        }
        
        private void AddPoints(int pointsToAdd)
        {
            if (isOpening) _currentScore += pointsToAdd;
            _currentScore += pointsToAdd;
            _totalScore += pointsToAdd;
            // Check if we hit the threshold
            if (_currentScore >= growPrice)
            {
                // Trigger the growth effect
                GameEvents.PlayerGrow?.Invoke(1);
        
                // Subtract the price. If score was 105, it is now 5.
                _currentScore -= growPrice;
        
                // Optional: Play a "Level Up" sound here
            }

            // Send the final calculated score to the UI
            GameEvents.UpdateScoreUI?.Invoke(_currentScore);
        }

        private void UpdatePlayerWin()
        {
            GameEvents.FreezeLevel?.Invoke();
            _currentGameState = GameState.PlayerWon;
            StartCoroutine(EndScene());
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

        private IEnumerator EndScene()
        {
            yield return new WaitForSeconds(1.7f);
            GameEvents.EndScene?.Invoke();
            yield return new WaitForSeconds(0.3f);
            GameEvents.GameOverUI?.Invoke(_currentGameState, _totalScore);
        }
    }
}

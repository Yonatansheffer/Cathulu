using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RiseOfCathulu.Domains.Utilities.GameHandlers.Scripts
{
    public class SceneLoaderPlaytest : MonoBehaviour
    {
        private const string GamePlaySceneName = "GamePlay 3";
        private const string EndingSceneName = "Ending Scene";

        private PlayerInputs _inputActions;

        private enum GameState
        {
            StartScreen,
            TutorialScreen,
            InLevel,
            EndingScene
        }

        private GameState _state = GameState.InLevel;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            _inputActions = new PlayerInputs();
            _inputActions.Movement.Continue.performed += OnContinue;
        }

        private void OnEnable()
        {
            _inputActions.Enable();
            GameEvents.EndScene += EndGame;
        }

        private void OnDisable()
        {
            _inputActions.Disable();
            GameEvents.EndScene -= EndGame;
        }

        private void OnContinue(UnityEngine.InputSystem.InputAction.CallbackContext _)
        {
            if (_state == GameState.InLevel)
                return;

            if (_state == GameState.StartScreen)
            {
                GameEvents.ContinueUI?.Invoke();
                _state = GameState.TutorialScreen;
                return;
            }

            LoadGamePlay();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                OnExit();
        }

        private void LoadGamePlay()
        {
            _state = GameState.InLevel;
            GameEvents.StopMusic?.Invoke();
            GameEvents.RestartLevel?.Invoke();
            SceneManager.LoadScene(GamePlaySceneName);
        }

        private void EndGame()
        {
            GameEvents.StopMusic?.Invoke();
            StartCoroutine(DelayedGameOver());
        }

        private IEnumerator DelayedGameOver()
        {
            yield return new WaitForSeconds(0.1f);
            SceneManager.LoadScene(EndingSceneName);
            _state = GameState.EndingScene;
        }

        private void OnExit()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}

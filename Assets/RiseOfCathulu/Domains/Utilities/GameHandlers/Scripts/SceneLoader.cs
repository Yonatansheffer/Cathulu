using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RiseOfCathulu.Domains.Utilities.GameHandlers.Scripts
{
    public class SceneLoader : MonoBehaviour
    {
        private const string GamePlaySceneName = "GamePlay 3";
        private const string EndingSceneName = "Ending Scene";
        private const string OpeningSceneName = "OpeningScene";

        private PlayerInputs _inputActions;

        private enum GameState
        {
            StartScreen,
            InstructionScreen,
            TutorialScreen,
            InLevel,
            EndingScene
        }

        private GameState _state = GameState.StartScreen;

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
            GameEvents.TutorialFinished += EndTutorial;

        }

        private void OnDisable()
        {
            _inputActions.Disable();
            GameEvents.EndScene -= EndGame;
            GameEvents.TutorialFinished -= EndTutorial;
        }

        private void OnContinue(UnityEngine.InputSystem.InputAction.CallbackContext _)
        {
            switch (_state)
            {
                case GameState.InLevel:
                    return;
                case GameState.StartScreen:
                    SceneManager.LoadScene(OpeningSceneName);
                    _state = GameState.InstructionScreen;
                    return;
                case GameState.InstructionScreen:
                    GameEvents.ContinueUI?.Invoke();
                    _state = GameState.TutorialScreen;
                    break;
                case GameState.EndingScene:
                    EndTutorial();
                    break;
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                OnExit();
            CheckRestart();
        }
        
        private void CheckRestart()
        {
            if (Input.GetKeyDown(KeyCode.R) && _state == GameState.InLevel)
            {
                StartCoroutine(LoadGamePlay());
            }
        }
        
        private void EndTutorial()
        {
            StartCoroutine(LoadGamePlay());
        }

        private IEnumerator LoadGamePlay()
        {
            _state = GameState.InLevel;
            GameEvents.StopMusic?.Invoke();
            yield return new WaitForSeconds(1f);
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
            _state = GameState.EndingScene;
            SceneManager.LoadScene(EndingSceneName);
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

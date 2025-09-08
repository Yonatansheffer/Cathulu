using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace B.O.S.S.Domains.Utilities.GameHandlers.Scripts
{
    public class SceneLoader : MonoBehaviour
    {
        private const string GamePlaySceneName = "GamePlay";
        private const string EndingSceneName = "Ending Scene";
        private bool _inLevel = true;

        private void Awake()
        {
            DontDestroyOnLoad(this);
        }
        
        private void OnEnable()
        {
            GameEvents.EndScene += EndGame;
            GameEvents.BeginGamePlay += LoadGamePlay;
        }
        
        private void OnDisable()
        {
            GameEvents.EndScene -= EndGame;
            GameEvents.BeginGamePlay -= LoadGamePlay;
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) OnExit();
            if(_inLevel) return;
            if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return)) LoadGamePlay();
        }

        private void LoadGamePlay()
        {
            _inLevel = true;
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
            _inLevel = false;
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
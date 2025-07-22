using System;
using System.Collections;
using GameHandlers;
using Sound;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
    public class SceneLoader : MonoBehaviour
    {
        private const string GamePlaySceneName = "GamePlay";
        private const string OpeningSceneName = "OpeningScene";
        private const string EndingSceneName = "EndingScene";
        private bool _InLevel = false;

        private void Awake()
        {
            DontDestroyOnLoad(this);
        }

        private void Start()
        {
            GameEvents.BeginGameLoop?.Invoke(); 
        }
        
        private void OnEnable()
        {
            GameEvents.GameOver += EndGame;
            GameEvents.RestartLevel += LoadGamePlay;
        }
        
        private void OnDisable()
        {
            GameEvents.GameOver -= EndGame;
            GameEvents.RestartLevel -= LoadGamePlay;
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
            {
                if(_InLevel) return;
                _InLevel = true;
                SoundManager.StopOpeningMusic();
                LoadGamePlay();
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnExit();
            }
        }

        private void LoadGamePlay()
        {
            SceneManager.LoadScene(GamePlaySceneName);
        }

        private void RestartGame()
        {
            //GameEvents.RestartGame?.Invoke();
            SceneManager.LoadScene(OpeningSceneName);
        }

        private void EndGame(bool didPlayerWin)
        {
            StartCoroutine(DelayedGameOver(didPlayerWin));
        }

        private IEnumerator DelayedGameOver(bool didPlayerWin)
        {
            yield return new WaitForSeconds(0.1f); 
            SceneManager.LoadScene(EndingSceneName);
            _InLevel = false;
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
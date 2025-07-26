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
        }
        
        private void OnDisable()
        {
            GameEvents.GameOver -= EndGame;
        }
        private void Update()
        {
            if(_InLevel) return;
            if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
            {
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

        private void EndGame(GameState dummy1, int dummy2)
        {
            StartCoroutine(DelayedGameOver());
        }

        private IEnumerator DelayedGameOver()
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
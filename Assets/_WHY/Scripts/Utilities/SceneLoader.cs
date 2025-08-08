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
        private const string EndingSceneName = "Ending Scene";
        private bool _InLevel = true;

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
            if(_InLevel) return;
            if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
            {
                LoadGamePlay();
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnExit();
            }
        }

        private void LoadGamePlay()
        {
            _InLevel = true;
            GameEvents.StopMusic?.Invoke();
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
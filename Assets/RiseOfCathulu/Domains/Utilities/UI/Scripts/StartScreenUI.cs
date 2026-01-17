using System;
using System.Collections;
using RiseOfCathulu.Domains.Utilities.GameHandlers.Scripts;
using UnityEngine;

namespace RiseOfCathulu.Domains.Utilities.UI.Scripts
{
    public class StartScreenUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField, Tooltip("Blinking 'Press Enter' text GameObject")] private GameObject pressXText;
        [SerializeField, Tooltip("Opening screen root GameObject")] private GameObject openingScreen;
        [SerializeField, Tooltip("Player root GameObject to enable on start")] private GameObject player;

        [Header("Behavior")]
        [SerializeField, Tooltip("Seconds between blink toggles")] private float blinkInterval = 0.2f;
        private Coroutine _blinkRoutine;
        private enum StartState
        {
            None,
            FirstPress,
            Started
        }
        private StartState _state = StartState.None;
        
        private void Start()
        {
            StartBlink();
        }

        private void OnEnable()
        {
            GameEvents.StartUI += OnStartUI;
        }

        private void OnDisable()
        {
            GameEvents.StartUI -= OnStartUI;
            if (_blinkRoutine != null) { StopCoroutine(_blinkRoutine); _blinkRoutine = null; }
        }
        
        private void OnStartUI()
        {
            switch (_state)
            {
                case StartState.None:
                    openingScreen?.SetActive(false);
                    _state = StartState.FirstPress;
                    break;
                case StartState.FirstPress:
                    _state = StartState.Started;
                    GameEvents.BeginGamePlay?.Invoke();
                    break;
            }
        }
        
        private void StartBlink()
        {
            if (_blinkRoutine != null)
            {
                StopCoroutine(_blinkRoutine);
                _blinkRoutine = null;
            }

            if (pressXText && gameObject.activeInHierarchy)
                _blinkRoutine = StartCoroutine(Blink(pressXText));
        }


        private IEnumerator Blink(GameObject obj)
        {
            while (true)
            {
                obj.SetActive(!obj.activeSelf);
                yield return new WaitForSeconds(blinkInterval);
            }
        }
    }
}
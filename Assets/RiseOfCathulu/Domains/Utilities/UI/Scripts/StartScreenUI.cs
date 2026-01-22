    using System;
    using System.Collections;
    using RiseOfCathulu.Domains.Utilities.GameHandlers.Scripts;
    using TMPro;
    using UnityEngine;

    namespace RiseOfCathulu.Domains.Utilities.UI.Scripts
    {
        public class StartScreenUI : MonoBehaviour
        {
            [Header("References")]
            [SerializeField, Tooltip("Blinking 'Press Enter' text GameObject")] private GameObject pressXText;
            [SerializeField, Tooltip("Opening screen root GameObject")] private GameObject openingScreen;
            [SerializeField, Tooltip("Instructions screen root GameObject")] private GameObject instructionsScreen;

            [SerializeField, Tooltip("Player root GameObject to enable on start")] private GameObject player;

            [Header("Behavior")]
            [SerializeField, Tooltip("Seconds between blink toggles")] private float blinkInterval = 0.2f;
            private Coroutine _blinkRoutine;
            [SerializeField] private Canvas canvas;
            [SerializeField] private TextMeshProUGUI pointsText;
            private bool _isInOpening = true;
            
            private void Start()
            {
                StartBlink();
            }

            private void OnEnable()
            {
                GameEvents.ContinueUI += OnStart;
                GameEvents.UpdateScoreUI += UpdateScore;
            }

            private void OnDisable()
            {
                GameEvents.ContinueUI -= OnStart;
                GameEvents.UpdateScoreUI -= UpdateScore;
                if (_blinkRoutine != null) { StopCoroutine(_blinkRoutine); _blinkRoutine = null; }
            }
            
            private void UpdateScore(int points)
            {
                if (!pointsText) return;
                pointsText.text = points.ToString();
            }

            private void OnStart()
            {
                if (_isInOpening)
                {
                    openingScreen?.SetActive(false);
                    _isInOpening = false;
                }
                else
                {
                    instructionsScreen?.SetActive(false);
                    if (canvas) canvas.gameObject.SetActive(true);
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
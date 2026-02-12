    using System;
    using System.Collections;
    using RiseOfCathulu.Domains.Utilities.GameHandlers.Scripts;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

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
            private bool _isInOpening = true;
            [SerializeField] private Slider scoreMeter; 
            [SerializeField] private int maxScoreValue = 100;
            private Coroutine _meterCoroutine; // Track to prevent overlapping animations

            
            private void Start()
            {
                StartBlink();
            }

            private void OnEnable()
            {
                GameEvents.UpdateScoreUI += UpdateScore;
                GameEvents.ContinueUI += OnStart;
            }

            private void OnDisable()
            {
                GameEvents.ContinueUI -= OnStart;
                GameEvents.UpdateScoreUI -= UpdateScore;
                if (_blinkRoutine != null) { StopCoroutine(_blinkRoutine); _blinkRoutine = null; }
            }
            
            private void UpdateScore(int totalPoints)
            {
                if (scoreMeter != null)
                {
                    // Use your maxScoreValue (100) to get the 0-1 ratio
                    float targetFill = (float)totalPoints / maxScoreValue;
        
                    // Clamp it just in case points briefly exceed max before subtraction
                    targetFill = Mathf.Clamp01(targetFill);

                    if (_meterCoroutine != null) StopCoroutine(_meterCoroutine);
                    _meterCoroutine = StartCoroutine(AnimateBar(targetFill));
                }
            }

            private IEnumerator AnimateBar(float targetFill)
            {
                float startFill = scoreMeter.value;
                float elapsed = 0;
                float duration = 0.15f; 

                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    scoreMeter.value = Mathf.Lerp(startFill, targetFill, elapsed / duration);
                    yield return null;
                }
                scoreMeter.value = targetFill;

                // FLASH EFFECT: If the bar just filled up (target >= 1)
                if (targetFill >= 0.99f) 
                {
                    Image fillImage = scoreMeter.fillRect.GetComponent<Image>();
                    Color originalColor = fillImage.color;
                    fillImage.color = Color.white; // Flash white
                    yield return new WaitForSeconds(0.05f);
                    fillImage.color = originalColor; // Back to normal
                }
    
                _meterCoroutine = null;
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
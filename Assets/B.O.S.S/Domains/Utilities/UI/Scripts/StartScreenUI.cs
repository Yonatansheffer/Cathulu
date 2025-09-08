using System.Collections;
using B.O.S.S.Domains.Utilities.GameHandlers.Scripts;
using UnityEngine;

namespace B.O.S.S.Domains.Utilities.UI.Scripts
{
    public class StartScreenUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField, Tooltip("Blinking 'Press Enter' text GameObject")]
        private GameObject pressEnterText;
        [SerializeField, Tooltip("Opening screen root GameObject")]
        private GameObject openingScreen;
        [SerializeField, Tooltip("Player root GameObject to enable on start")]
        private GameObject player;

        [Header("Behavior")]
        [SerializeField, Tooltip("Seconds between blink toggles")]
        private float blinkInterval = 0.2f;

        private bool _isEnterToStart;
        private Coroutine _blinkRoutine;

        private void Start()
        {
            StartBlink();
            GameEvents.BeginGameLoop?.Invoke();
        }

        private void OnDisable()
        {
            if (_blinkRoutine != null) { StopCoroutine(_blinkRoutine); _blinkRoutine = null; }
        }

        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Return) && !Input.GetKeyDown(KeyCode.KeypadEnter)) return;
            if (!_isEnterToStart)
            {
                _isEnterToStart = true;
                if (openingScreen) openingScreen.SetActive(false);
                if (player) player.SetActive(true);
            }
            else
            {
                GameEvents.BeginGamePlay?.Invoke();
            }
        }

        private void StartBlink()
        {
            if (pressEnterText && gameObject.activeInHierarchy)
                _blinkRoutine = StartCoroutine(Blink(pressEnterText));
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
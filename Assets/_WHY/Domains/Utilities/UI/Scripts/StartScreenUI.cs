using System.Collections;
using _WHY.Domains.Utilities.GameHandlers.Scripts;
using UnityEngine;

namespace _WHY.Domains.Utilities.UI.Scripts
{
    public class StartScreenUI : MonoBehaviour
    {
        [SerializeField] private GameObject pressEnterText;
        [SerializeField] private GameObject openingScreen;
        [SerializeField] private GameObject player;
        [SerializeField] private float blinkInterval = 0.2f;
        private bool _isEnterToStart;

        private void Start()
        {
            StartCoroutine(Blink(pressEnterText));
            GameEvents.BeginGameLoop?.Invoke();
        }

        private IEnumerator Blink(GameObject obj)
        {
            while (true)
            {
                obj.SetActive(!obj.activeSelf);
                yield return new WaitForSeconds(blinkInterval);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if (!_isEnterToStart)
                {
                    _isEnterToStart = true;
                    openingScreen.SetActive(false);
                    player.SetActive(true);
                }
                else
                {
                    GameEvents.BeginGamePlay?.Invoke();
                }
            }
        }
    }
}
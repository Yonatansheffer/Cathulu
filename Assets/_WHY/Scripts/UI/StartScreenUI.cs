using System;
using System.Collections;
using GameHandlers;
using UnityEngine;

namespace UI
{
    public class StartScreenUI : MonoBehaviour
    {
        [SerializeField] private GameObject pressEnterText;
        [SerializeField] private GameObject openingScreen;
        [SerializeField] private GameObject player;
        [SerializeField] private float blinkInterval = 0.2f;

        private bool _isEnterToStart = false;

        private void Start()
        {
            StartCoroutine(Blink(pressEnterText));
            GameEvents.BeginGameLoop?.Invoke(); // Optional: move to first Enter if needed
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
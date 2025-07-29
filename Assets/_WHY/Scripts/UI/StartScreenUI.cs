using System;
using System.Collections;
using GameHandlers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class StartScreenUI : MonoBehaviour
    {
        //[SerializeField] private Image startImage;
        [SerializeField] private TextMeshProUGUI pressEnterText;
        [SerializeField] private float blinkInterval = 0.2f;
        /*private Coroutine _blinkCoroutine;
        private bool _canPressEnter*/
        //[SerializeField] private Image pangLogo;
        //[SerializeField] private TextMeshProUGUI mitchellText;
        //[SerializeField] private float licenseDisplayDuration = 3f;
        /*private void Awake()
        {
            _canPressEnter = false;
        }*/

        private void Start()
        {
            StartCoroutine(BlinkText(pressEnterText));
        }

        /*private void OnEnable()
        {
            GameEvents.BeginGameLoop += ShowStartScreen;
        }
        
        private void OnDisable()
        {
            GameEvents.BeginGameLoop -= ShowStartScreen;
        }*/

        /*private void ShowLicenseImage()
        {
            licenseImage.gameObject.SetActive(true);
            pangLogo.gameObject.SetActive(false);
            pressEnterText.gameObject.SetActive(false);
            mitchellText.gameObject.SetActive(false);
            StartCoroutine(ShowLogoAfterLicense());
        }*/

        /*private void ShowStartScreen()
        {
            //licenseImage.gameObject.SetActive(false);
            //pangLogo.gameObject.SetActive(true);
            //mitchellText.gameObject.SetActive(true);
            /*pressEnterText.gameObject.SetActive(true);
            _canPressEnter = true;#1#
        }*/
    
        private IEnumerator BlinkText(TextMeshProUGUI text)
        {
            while (true)
            {
                text.enabled = !text.enabled;
                yield return new WaitForSeconds(blinkInterval);
            }
        }

        /*private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return) && _canPressEnter)
            {
                _canPressEnter = false;
                if (_blinkCoroutine != null)
                {
                    StopCoroutine(_blinkCoroutine);
                }
                pressEnterText.gameObject.SetActive(false);
                startImage.gameObject.SetActive(false);
                //mitchellText.gameObject.SetActive(false);
                //GameEvents.StartGame?.Invoke(); 
            }
        }*/
    }
}
using System;
using UnityEngine;

namespace B.O.S.S.Domains.Utilities.GameHandlers.Scripts
    {
        public class FitCameraToWorldDimensions : MonoBehaviour
        {
            private const float RatioChangeThreshold = 0.01f;
            [SerializeField] private Camera cam;
            [Header("Desired world units visible horizontally")]
            [SerializeField] private float targetWidth = 10f;
            [Header("Desired world units visible vertically")]
            [SerializeField] private float targetHeight = 5f;
            private float _currentAspectRatio;

            private void Awake()
            {
                if (cam == null)
                    cam = Camera.main;
            }

            private void Start()
            {
                _currentAspectRatio = (float)Screen.width / Screen.height;
                FitToDimensions();
            }

            private void Update()
            {
                var newRatio = (float)Screen.width / Screen.height;
                if (!(Math.Abs(newRatio - _currentAspectRatio) > RatioChangeThreshold)) return;
                _currentAspectRatio = newRatio;
                FitToDimensions();
            }

            private void FitToDimensions()
            {
                var widthOrthographicSize = targetWidth / _currentAspectRatio / 2f;
                var heightOrthographicSize = targetHeight / 2f;
                cam.orthographicSize = Mathf.Max(widthOrthographicSize, heightOrthographicSize);
            }
        }
    }
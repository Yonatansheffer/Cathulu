using System.Collections;
using GameHandlers;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _WHY.Scripts.Utilities
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
        [SerializeField] private float smoothSpeed = 5f;
        [SerializeField] private float rightxBound;
        [SerializeField] private float leftxBound;
        [SerializeField] private float yLowerBound;
        [SerializeField] private float yUpperBound;
        [SerializeField] private float _shakeDuration;
        [SerializeField] private float _shakeMagnitude;
        private bool _isZoomingOut = false;
        private Vector3 _originalOffset;    
        private bool _isStartingZoomIn = true;

        [Header("Zoom Settings")]
        [SerializeField] private float startZoomSize = 5f;
        [SerializeField] private float targetZoomSize = 15f;
        [SerializeField] private float zoomSpeed = 1f;

        private Camera _cam;

        private void OnEnable()
        {
            GameEvents.ShakeCamera += CameraShake;
            GameEvents.BossShoots += ZoomOutToCenter;
        }

        private void OnDisable()
        {
            GameEvents.ShakeCamera -= CameraShake;
            GameEvents.BossShoots -= ZoomOutToCenter;
        }

        private void Start()
        {
            _cam = GetComponent<Camera>();
            if (_cam != null)
            {
                _cam.orthographicSize = startZoomSize;
            }
            _originalOffset = offset;
            StartCoroutine(StartZoomToTarget());
        }

        private IEnumerator StartZoomToTarget()
        {
            float duration = 8f;
            float elapsed = 0f;
            Vector3 startPos = transform.position;
            Vector3 endPos = target.position + offset;
            float startSize = _cam.orthographicSize;
            float endSize = targetZoomSize;
            while (elapsed < duration)
            {
                float linearT = elapsed / duration;
                float t = Mathf.Pow(linearT, 3); 
                transform.position = Vector3.Lerp(startPos, endPos, t);
                _cam.orthographicSize = Mathf.Lerp(startSize, endSize, t);
                elapsed += Time.deltaTime;
                yield return null;
            }
            transform.position = endPos;
            _cam.orthographicSize = endSize;
            _isStartingZoomIn = false;
        }



        private void LateUpdate()
        {
            if (target == null || _cam == null || _isZoomingOut || _isStartingZoomIn) return;

            Vector3 desiredPosition = target.position + offset;
            float clampedX = Mathf.Clamp(desiredPosition.x, leftxBound, rightxBound);
            float clampedY = Mathf.Clamp(desiredPosition.y, yLowerBound, yUpperBound);
            Vector3 clampedPosition = new Vector3(clampedX, clampedY, desiredPosition.z);
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, clampedPosition, smoothSpeed * Time.deltaTime);
            transform.position = smoothedPosition;

            if (_cam.orthographicSize > targetZoomSize)
            {
                _cam.orthographicSize = Mathf.Lerp(_cam.orthographicSize, targetZoomSize, zoomSpeed * Time.deltaTime);
            }
        }

        private IEnumerator Shake(float duration, float magnitude)
        {
            Vector3 originalPos = transform.localPosition;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float x = Random.Range(-1f, 1f) * magnitude;
                float y = Random.Range(-1f, 1f) * magnitude;
                transform.localPosition = originalPos + new Vector3(x, y, 0f);
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.localPosition = originalPos;
        }

        private void CameraShake()
        { 
            StartCoroutine(Shake(_shakeDuration, _shakeMagnitude));
        }
    
        private void ZoomOutToCenter()
        {
            StartCoroutine(ZoomOutSequence());
        }

        private IEnumerator ZoomOutSequence()
        {
            _isZoomingOut = true;
            Vector3 centerPosition = new Vector3((leftxBound + rightxBound) / 2f+3f, 0f, offset.z);
            float zoomOutSize = targetZoomSize + 30f; 
            float duration = 2f; 
            float elapsed = 0f;
            Vector3 startPos = transform.position;
            float startSize = _cam.orthographicSize;
            while (elapsed < duration)
            {
                transform.position = Vector3.Lerp(startPos, centerPosition, elapsed / duration);
                _cam.orthographicSize = Mathf.Lerp(startSize, zoomOutSize, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.position = centerPosition;
            _cam.orthographicSize = zoomOutSize;
            yield return new WaitForSeconds(8f);
            elapsed = 0f;
            Vector3 returnPos = target.position + offset;
            float returnSize = targetZoomSize;
            startPos = transform.position;
            startSize = _cam.orthographicSize;

            while (elapsed < duration)
            {
                transform.position = Vector3.Lerp(startPos, returnPos, elapsed / duration);
                _cam.orthographicSize = Mathf.Lerp(startSize, returnSize, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            transform.position = returnPos;
            _cam.orthographicSize = returnSize;
            _isZoomingOut = false;
        }
    }
}

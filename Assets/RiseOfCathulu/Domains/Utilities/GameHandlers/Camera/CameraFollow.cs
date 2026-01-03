using System.Collections;
using RiseOfCathulu.Domains.Player.Scripts;
using RiseOfCathulu.Domains.Utilities.GameHandlers.Scripts;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RiseOfCathulu.Domains.Utilities.GameHandlers.Camera
{
    public class CameraFollow : MonoBehaviour
    {
        [Header("Targets")]
        [SerializeField, Tooltip("Player transform to follow")] private Transform playerTarget;
        [SerializeField] private PlayerSize playerSize; // Reference to track player scale
        [SerializeField, Tooltip("Boss transform to focus on when destroyed")] private Transform bossTarget;

        [Header("Follow")]
        [SerializeField, Tooltip("Camera offset from target")] private Vector3 offset = new Vector3(0f, 0f, -10f);
        [SerializeField, Tooltip("Follow smoothing factor")] private float smoothSpeed = 5f;
        [SerializeField, Tooltip("Left world bound for camera x")] private float leftxBound;
        [SerializeField, Tooltip("Right world bound for camera x")] private float rightxBound;
        [SerializeField, Tooltip("Lower world bound for camera y")] private float yLowerBound;
        [SerializeField, Tooltip("Upper world bound for camera y")] private float yUpperBound;

        [Header("Dynamic Zoom")]
        [SerializeField, Tooltip("Base zoom when player is at min size")] private float baseZoomSize = 10f;
        [SerializeField, Tooltip("Camera zoom increase per unit of player scale")] private float zoomMultiplier = 3.5f;
        [SerializeField, Tooltip("Minimum orthographic size allowed")] private float minZoomSize = 8f;   
        [SerializeField, Tooltip("Maximum orthographic size allowed")] private float maxZoomSize = 10000f;
        [SerializeField, Tooltip("Smoothing speed for size-based zooming")] private float zoomSmoothing = 2f;
        
        [Header("Manual Zoom Settings")]
        [SerializeField] private float startZoomSize = 5f;
        [SerializeField] private float startZoomDuration = 8f;

        [Header("Zoom-Out Sequence")]
        [SerializeField, Tooltip("Additional size to zoom out during boss shooting")] private float zoomOutExtraSize = 30f;
        [SerializeField, Tooltip("Duration for zoom-out and return animations")] private float zoomOutLerpDuration = 2f;
        [SerializeField, Tooltip("Hold time while zoomed out")] private float zoomOutHoldSeconds = 8f;
        [SerializeField, Tooltip("Horizontal offset from center when zooming out")] private float zoomOutCenterXOffset = 3f;

        [Header("Shake")]
        [SerializeField, Tooltip("Default shake duration on shake event")] private float shakeDuration = 0.25f;
        [SerializeField, Tooltip("Default shake magnitude on shake event")] private float shakeMagnitude = 0.4f;

        private UnityEngine.Camera _cam;
        private bool _isZoomingOut;
        private bool _isStartingZoomIn = true;
        private bool _isFrozen; // only pauses special sequences, not follow
        private float _dynamicTargetZoom;

        private Coroutine _focusRoutine;
        private Coroutine _zoomRoutine;
        private Coroutine _shakeRoutine;

        private bool _hasSkippedStartZoom = false;
        
        private void Start()
        {
            _cam = GetComponent<UnityEngine.Camera>();
            if (_cam != null) _cam.orthographicSize = startZoomSize;
            if (playerSize == null && playerTarget != null)
                playerSize = playerTarget.GetComponent<PlayerSize>();

            StartCoroutine(StartZoomToTarget());
        }
        private void OnEnable()
        {
            GameEvents.ShakeCamera += OnShakeCamera;
            GameEvents.PlayerFirstMoved += OnPlayerFirstMove; 
            GameEvents.FreezeLevel += OnFreeze;
            GameEvents.UnFreezeLevel += OnUnfreeze;
        }

        private void OnDisable()
        {
            GameEvents.ShakeCamera -= OnShakeCamera;
            GameEvents.PlayerFirstMoved -= OnPlayerFirstMove;
            GameEvents.FreezeLevel -= OnFreeze;
            GameEvents.UnFreezeLevel -= OnUnfreeze;
        }

        private void OnPlayerFirstMove()
        {
            if (_isStartingZoomIn && !_hasSkippedStartZoom)
            {
                CutStartZoom();
            }
        }

        private void CutStartZoom()
        {
            _hasSkippedStartZoom = true;
            StopAllCoroutines(); 
            UpdateDynamicZoom();
            if (playerTarget != null)
            {
                Vector3 finalPos = playerTarget.position + offset;
                float camHalfHeight = _cam.orthographicSize;
                float camHalfWidth = camHalfHeight * _cam.aspect;
                float clampedX = Mathf.Clamp(finalPos.x, leftxBound + camHalfWidth, rightxBound - camHalfWidth);
                float clampedY = Mathf.Clamp(finalPos.y, yLowerBound + camHalfHeight, yUpperBound - camHalfHeight);
        
                transform.position = new Vector3(clampedX, clampedY, finalPos.z);
            }
            _cam.orthographicSize = _dynamicTargetZoom;
            _isStartingZoomIn = false;
        }

        private void LateUpdate()
        {
            if (playerTarget == null || _cam == null || _isZoomingOut || _isStartingZoomIn) return;

            // 1. Position Follow
            Vector3 desired = playerTarget.position + offset;

            // 2. Calculate the current visible bounds of the camera
            float camHalfHeight = _cam.orthographicSize;
            float camHalfWidth = camHalfHeight * _cam.aspect;

            // 3. Clamp the center position so the EDGES stay within bounds
            // We add half-width to the left and subtract it from the right
            float clampedX = Mathf.Clamp(desired.x, leftxBound + camHalfWidth, rightxBound - camHalfWidth);
            float clampedY = Mathf.Clamp(desired.y, yLowerBound + camHalfHeight, yUpperBound - camHalfHeight);

            // 4. Apply Smoothing
            transform.position = Vector3.Lerp(transform.position, new Vector3(clampedX, clampedY, desired.z), smoothSpeed * Time.deltaTime);

            // 5. Dynamic Zoom Logic
            UpdateDynamicZoom();
        }
        
        private void UpdateDynamicZoom()
        {
            float playerScale = playerSize != null ? playerSize.CurrentScale : 1f;
    
            // 1. Calculate the ideal zoom
            float rawTargetZoom = baseZoomSize + (playerScale * zoomMultiplier);

            // 2. Clamp between Min and Max
            _dynamicTargetZoom = Mathf.Clamp(rawTargetZoom, minZoomSize, maxZoomSize);

            // 3. Smoothly transition
            _cam.orthographicSize = Mathf.Lerp(_cam.orthographicSize, _dynamicTargetZoom, zoomSmoothing * Time.deltaTime);
        }

        private void OnFreeze()   { _isFrozen = true; }
        private void OnUnfreeze() { _isFrozen = false; }

        private void OnShakeCamera()
        {
            if (_shakeRoutine != null) StopCoroutine(_shakeRoutine);
            _shakeRoutine = StartCoroutine(Shake(shakeDuration, shakeMagnitude));
        }

        private IEnumerator StartZoomToTarget()
        {
            // Update the dynamic target once to have a valid end point
            UpdateDynamicZoom();
            
            var duration = Mathf.Max(0f, startZoomDuration);
            var elapsed = 0f;
            var startPos = transform.position;
            var startSize = _cam.orthographicSize;

            while (elapsed < duration)
            {
                if (_isFrozen) { yield return new WaitUntil(() => !_isFrozen); }

                var t0 = elapsed / duration;
                var t = t0 * t0 * t0;
                
                var endPos = playerTarget ? playerTarget.position + offset : startPos;
                transform.position = Vector3.Lerp(startPos, endPos, t);
                
                // Zooming toward the dynamic target calculated from player size
                _cam.orthographicSize = Mathf.Lerp(startSize, _dynamicTargetZoom, t);
                
                elapsed += Time.deltaTime;
                yield return null;
            }

            _isStartingZoomIn = false;
        }


        private IEnumerator ZoomOutSequence()
        {
            _isZoomingOut = true;

            var centerX = (leftxBound + rightxBound) * 0.5f + zoomOutCenterXOffset;
            var centerPos = new Vector3(centerX, 0f, offset.z);
            var zoomOutSize = _dynamicTargetZoom + Mathf.Abs(zoomOutExtraSize);
            var duration = Mathf.Max(0.0001f, zoomOutLerpDuration);
            var elapsed = 0f;
            var startPos = transform.position;
            var startSize = _cam.orthographicSize;

            while (elapsed < duration)
            {
                if (_isFrozen) { yield return new WaitUntil(() => !_isFrozen); }

                var t = elapsed / duration;
                transform.position = Vector3.Lerp(startPos, centerPos, t);
                _cam.orthographicSize = Mathf.Lerp(startSize, zoomOutSize, t);
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.position = centerPos;
            _cam.orthographicSize = zoomOutSize;

            var hold = Mathf.Max(0f, zoomOutHoldSeconds);
            var holdElapsed = 0f;
            while (holdElapsed < hold)
            {
                if (_isFrozen) { yield return new WaitUntil(() => !_isFrozen); }
                holdElapsed += Time.deltaTime;
                yield return null;
            }

            elapsed = 0f;
            var returnPos = playerTarget ? playerTarget.position + offset : transform.position;
            var returnSize = _dynamicTargetZoom;
            startPos = transform.position;
            startSize = _cam.orthographicSize;

            while (elapsed < duration)
            {
                if (_isFrozen) { yield return new WaitUntil(() => !_isFrozen); }

                var t = elapsed / duration;
                transform.position = Vector3.Lerp(startPos, returnPos, t);
                _cam.orthographicSize = Mathf.Lerp(startSize, returnSize, t);
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.position = returnPos;
            _cam.orthographicSize = returnSize;
            _isZoomingOut = false;
        }

        private IEnumerator Shake(float duration, float magnitude)
        {
            var original = transform.localPosition;
            var elapsed = 0f;
            while (elapsed < duration)
            {
                if (_isFrozen) { yield return new WaitUntil(() => !_isFrozen); }

                var x = Random.Range(-1f, 1f) * magnitude;
                var y = Random.Range(-1f, 1f) * magnitude;
                transform.localPosition = original + new Vector3(x, y, 0f);
                elapsed += Time.deltaTime;
                yield return null;
            }
            transform.localPosition = original;
        }
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Vector3 center = new Vector3((leftxBound + rightxBound) / 2f, (yLowerBound + yUpperBound) / 2f, 0f);
            Vector3 size = new Vector3(rightxBound - leftxBound, yUpperBound - yLowerBound, 1f);
            Gizmos.DrawWireCube(center, size);
        }   
    }
}


/*[Header("Boss Focus")]
[SerializeField, Tooltip("Duration of the boss focus in animation")] private float bossFocusInDuration = 1.25f;
[SerializeField, Tooltip("How much to zoom closer relative to target zoom (positive brings closer)")] private float bossZoomCloserBy = 6f;*/



/*private IEnumerator FocusOnBossSequence()
{
    _isZoomingOut = true;

    var desired = bossTarget.position + offset;
    var x = Mathf.Clamp(desired.x, leftxBound, rightxBound);
    var y = Mathf.Clamp(desired.y, yLowerBound, yUpperBound);
    var bossPos = new Vector3(x, y, desired.z);

    var startSize = _cam.orthographicSize;
    var bossZoom = Mathf.Max(6f, targetZoomSize - Mathf.Abs(bossZoomCloserBy));
    var startPos = transform.position;

    var t = 0f;
    var d = Mathf.Max(0f, bossFocusInDuration);
    while (t < d)
    {
        if (_isFrozen) { yield return new WaitUntil(() => !_isFrozen); }

        var u = t / d;
        var e = u < 0.5f ? 4f * u * u * u : 1f - Mathf.Pow(-2f * u + 2f, 3f) / 2f;
        var smoothPos = Vector3.Lerp(startPos, bossPos, e);
        _cam.orthographicSize = Mathf.Lerp(startSize, bossZoom, e);
        var shakeMag = Mathf.Lerp(1f, 0f, u);
        smoothPos.x += Random.Range(-shakeMag, shakeMag);
        smoothPos.y += Random.Range(-shakeMag, shakeMag);
        transform.position = smoothPos;

        t += Time.deltaTime;
        yield return null;
    }

    transform.position = bossPos;
    _cam.orthographicSize = bossZoom;
}*/


/*private void OnBossShoots()
{
    if (_zoomRoutine != null) StopCoroutine(_zoomRoutine);
    _zoomRoutine = StartCoroutine(ZoomOutSequence());
}*/

/*private void OnBossDestroyed()
{
    if (bossTarget == null || _cam == null) return;
    if (_focusRoutine != null) StopCoroutine(_focusRoutine);
    _focusRoutine = StartCoroutine(FocusOnBossSequence());
}*/
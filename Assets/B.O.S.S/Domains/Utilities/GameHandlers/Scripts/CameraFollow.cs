using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace B.O.S.S.Domains.Utilities.GameHandlers.Scripts
{
    public class CameraFollow : MonoBehaviour
    {
        [Header("Targets")]
        [SerializeField, Tooltip("Player transform to follow")] private Transform playerTarget;
        [SerializeField, Tooltip("Boss transform to focus on when destroyed")] private Transform bossTarget;

        [Header("Follow")]
        [SerializeField, Tooltip("Camera offset from target")] private Vector3 offset = new Vector3(0f, 0f, -10f);
        [SerializeField, Tooltip("Follow smoothing factor")] private float smoothSpeed = 5f;
        [SerializeField, Tooltip("Left world bound for camera x")] private float leftxBound;
        [SerializeField, Tooltip("Right world bound for camera x")] private float rightxBound;
        [SerializeField, Tooltip("Lower world bound for camera y")] private float yLowerBound;
        [SerializeField, Tooltip("Upper world bound for camera y")] private float yUpperBound;

        [Header("Zoom")]
        [SerializeField, Tooltip("Orthographic size at start")] private float startZoomSize = 5f;
        [SerializeField, Tooltip("Orthographic size during gameplay")] private float targetZoomSize = 15f;
        [SerializeField, Tooltip("Zoom smoothing speed toward target size")] private float zoomSpeed = 1f;

        [Header("Start Zoom")]
        [SerializeField, Tooltip("Duration of the initial zoom and move-in")] private float startZoomDuration = 8f;

        [Header("Boss Focus")]
        [SerializeField, Tooltip("Duration of the boss focus in animation")] private float bossFocusInDuration = 1.25f;
        [SerializeField, Tooltip("How much to zoom closer relative to target zoom (positive brings closer)")] private float bossZoomCloserBy = 6f;

        [Header("Zoom-Out Sequence")]
        [SerializeField, Tooltip("Additional size to zoom out during boss shooting")] private float zoomOutExtraSize = 30f;
        [SerializeField, Tooltip("Duration for zoom-out and return animations")] private float zoomOutLerpDuration = 2f;
        [SerializeField, Tooltip("Hold time while zoomed out")] private float zoomOutHoldSeconds = 8f;
        [SerializeField, Tooltip("Horizontal offset from center when zooming out")] private float zoomOutCenterXOffset = 3f;

        [Header("Shake")]
        [SerializeField, Tooltip("Default shake duration on shake event")] private float shakeDuration = 0.25f;
        [SerializeField, Tooltip("Default shake magnitude on shake event")] private float shakeMagnitude = 0.4f;

        private Camera _cam;
        private bool _isZoomingOut;
        private bool _isStartingZoomIn = true;
        private bool _isFrozen; // only pauses special sequences, not follow

        private Coroutine _focusRoutine;
        private Coroutine _zoomRoutine;
        private Coroutine _shakeRoutine;

        private void OnEnable()
        {
            GameEvents.ShakeCamera += OnShakeCamera;
            GameEvents.BossShoots += OnBossShoots;
            GameEvents.BossDestroyed += OnBossDestroyed;
            GameEvents.FreezeLevel += OnFreeze;
            GameEvents.UnFreezeLevel += OnUnfreeze;
        }

        private void OnDisable()
        {
            GameEvents.ShakeCamera -= OnShakeCamera;
            GameEvents.BossShoots -= OnBossShoots;
            GameEvents.BossDestroyed -= OnBossDestroyed;
            GameEvents.FreezeLevel -= OnFreeze;
            GameEvents.UnFreezeLevel -= OnUnfreeze;
        }

        private void Start()
        {
            _cam = GetComponent<Camera>();
            if (_cam != null) _cam.orthographicSize = startZoomSize;
            StartCoroutine(StartZoomToTarget());
        }

        private void LateUpdate()
        {
            if (playerTarget == null || _cam == null || _isZoomingOut || _isStartingZoomIn) return;

            var desired = playerTarget.position + offset;
            var clampedX = Mathf.Clamp(desired.x, leftxBound, rightxBound);
            var clampedY = Mathf.Clamp(desired.y, yLowerBound, yUpperBound);
            var clamped = new Vector3(clampedX, clampedY, desired.z);
            var smoothed = Vector3.Lerp(transform.position, clamped, smoothSpeed * Time.deltaTime);
            transform.position = smoothed;

            if (_cam.orthographicSize > targetZoomSize)
                _cam.orthographicSize = Mathf.Lerp(_cam.orthographicSize, targetZoomSize, zoomSpeed * Time.deltaTime);
        }

        private void OnFreeze()   { _isFrozen = true; }
        private void OnUnfreeze() { _isFrozen = false; }

        private void OnShakeCamera()
        {
            if (_shakeRoutine != null) StopCoroutine(_shakeRoutine);
            _shakeRoutine = StartCoroutine(Shake(shakeDuration, shakeMagnitude));
        }

        private void OnBossShoots()
        {
            if (_zoomRoutine != null) StopCoroutine(_zoomRoutine);
            _zoomRoutine = StartCoroutine(ZoomOutSequence());
        }

        private void OnBossDestroyed()
        {
            if (bossTarget == null || _cam == null) return;
            if (_focusRoutine != null) StopCoroutine(_focusRoutine);
            _focusRoutine = StartCoroutine(FocusOnBossSequence());
        }

        private IEnumerator StartZoomToTarget()
        {
            var duration = Mathf.Max(0f, startZoomDuration);
            var elapsed = 0f;
            var startPos = transform.position;
            var endPos = playerTarget ? playerTarget.position + offset : startPos;
            var startSize = _cam.orthographicSize;
            var endSize = targetZoomSize;

            while (elapsed < duration)
            {
                if (_isFrozen) { yield return new WaitUntil(() => !_isFrozen); }

                var t0 = elapsed / duration;
                var t = t0 * t0 * t0;
                transform.position = Vector3.Lerp(startPos, endPos, t);
                _cam.orthographicSize = Mathf.Lerp(startSize, endSize, t);
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.position = endPos;
            _cam.orthographicSize = endSize;
            _isStartingZoomIn = false;
        }

        private IEnumerator FocusOnBossSequence()
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
        }

        private IEnumerator ZoomOutSequence()
        {
            _isZoomingOut = true;

            var centerX = (leftxBound + rightxBound) * 0.5f + zoomOutCenterXOffset;
            var centerPos = new Vector3(centerX, 0f, offset.z);
            var zoomOutSize = targetZoomSize + Mathf.Abs(zoomOutExtraSize);
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
            var returnSize = targetZoomSize;
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
    }
}

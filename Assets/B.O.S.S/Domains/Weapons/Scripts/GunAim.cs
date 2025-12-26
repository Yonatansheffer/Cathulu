using UnityEngine;
using UnityEngine.InputSystem;
    
namespace B.O.S.S.Domains.Weapons.Scripts
{
    public class GunAim : MonoBehaviour
    {
        private Camera _mainCamera;

        private void Start()
        {
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            if (Gamepad.all.Count > 0)
                return;
            if (_mainCamera == null) return;
            var mouseScreenPos = Input.mousePosition;
            var mouseWorldPos = _mainCamera.ScreenToWorldPoint
                (new Vector3(mouseScreenPos.x, mouseScreenPos.y, _mainCamera.nearClipPlane));
            mouseWorldPos.z = 0f;
            var direction = mouseWorldPos - transform.position;
            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle-90);
        }
    }
}
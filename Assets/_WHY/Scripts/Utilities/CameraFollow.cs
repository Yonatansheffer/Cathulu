using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target; // The player
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f); // Camera's offset
    [SerializeField] private float smoothSpeed = 5f; // How smooth the camera follows
    [SerializeField] private float rightxBound; // Right X boundary
    [SerializeField] private float leftxBound; // Left X boundary
    [SerializeField] private float yBound; // Y boundary

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;

        // Clamp the camera's position within the specified bounds
        float clampedX = Mathf.Clamp(desiredPosition.x, leftxBound, rightxBound);
        float clampedY = Mathf.Clamp(desiredPosition.y, -yBound, yBound); // Assuming you want to constrain it only in positive Y
        Vector3 clampedPosition = new Vector3(clampedX, clampedY, desiredPosition.z);

        // Smoothly move to the clamped position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, clampedPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;
    }
}
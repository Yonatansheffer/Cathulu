using UnityEngine;
using UnityEngine.InputSystem;

public class SurfaceStickController : MonoBehaviour
{
    private Rigidbody2D rb;
    private PlayerInputs controls; // Generated Input System class
    private Vector2 moveInput;
    private bool jumpPressed;

    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float raycastDistance = 0.6f; // Distance to detect surfaces
    [SerializeField] private LayerMask surfaceLayer; // Assign this in the Inspector

    private Vector2 surfaceNormal; // Normal of the current surface
    private bool isGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        controls = new PlayerInputs();

        // Bind input actions
        controls.Movement.Move.performed += ctx => moveInput.x = ctx.ReadValue<float>();
        controls.Movement.Move.canceled += ctx => moveInput.x = 0f;
        controls.Movement.Jump.performed += ctx => jumpPressed = true;
    }

    void OnEnable()
    {
        controls.Enable();
    }

    void OnDisable()
    {
        controls.Disable();
    }

    void FixedUpdate()
    {
        CheckSurface();
        UpdateGravity();
        MoveAlongSurface();
        HandleJump();
    }

    void CheckSurface()
    {
        // Cast a ray downward in the direction of gravity to detect the surface
        RaycastHit2D hit = Physics2D.Raycast(transform.position, -surfaceNormal, raycastDistance, surfaceLayer);
        if (hit.collider != null)
        {
            isGrounded = true;
            surfaceNormal = hit.normal; // Update the surface normal
        }
        else
        {
            isGrounded = false;
        }

        // Visualize the raycast in the editor
        Debug.DrawRay(transform.position, -surfaceNormal * raycastDistance, Color.red);
    }

    void UpdateGravity()
    {
        // If grounded, align gravity with the surface normal
        if (isGrounded)
        {
            rb.gravityScale = 0f; // Disable default gravity
            Physics2D.gravity = -surfaceNormal.normalized * 9.81f; // Custom gravity direction
        }
        else
        {
            rb.gravityScale = 1f; // Revert to default gravity when airborne
        }
    }

    void MoveAlongSurface()
    {
        if (!isGrounded) return;

        // Calculate the tangent (perpendicular to the normal) for movement
        Vector2 surfaceTangent = Vector2.Perpendicular(surfaceNormal).normalized;

        // Move along the surface based on input
        Vector2 moveDirection = surfaceTangent * moveInput.x * moveSpeed;
        rb.linearVelocity = new Vector2(moveDirection.x, moveDirection.y);
    }

    void HandleJump()
    {
        if (jumpPressed && isGrounded)
        {
            rb.linearVelocity = surfaceNormal.normalized * jumpForce; // Jump away from the surface
            jumpPressed = false; // Reset jump input
        }
    }

    // Optional: Visualize the player's up direction
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, surfaceNormal * 1f);
    }
}
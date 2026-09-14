using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField]
    float currentSpeed = 0f;

    [SerializeField]
    float walkSpeed = 5f;

    [SerializeField]
    float runSpeed = 10f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private InputManager inputManager;
    private Animator animator;

    // Dipakai oleh FarmingManager untuk mengunci player
    private bool movementLocked = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        inputManager = GetComponent<InputManager>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        // =====================================================
        // PLAYER DI-LOCK
        // =====================================================

        if (movementLocked)
        {
            StopMovement();
            return;
        }

        // =====================================================
        // HOLD TANAM
        // =====================================================

        if (FarmingManager.IsHoldingPlant)
        {
            StopMovement();
            return;
        }

        // =====================================================
        // MOVEMENT NORMAL
        // =====================================================

        rb.linearVelocity = moveInput * currentSpeed;
    }

    // =========================================================
    // LOCK / UNLOCK
    // =========================================================

    public void SetMovementLocked(bool locked)
    {
        movementLocked = locked;

        if (locked)
        {
            StopMovement();
        }
    }

    public bool IsMovementLocked()
    {
        return movementLocked;
    }

    // =========================================================
    // STOP MOVEMENT
    // =========================================================

    private void StopMovement()
    {
        moveInput = Vector2.zero;
        currentSpeed = 0f;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsRunning", false);
        }
    }

    // =========================================================
    // MOVE INPUT
    // =========================================================

    public void Move(InputAction.CallbackContext context)
    {
        // Kalau player sedang dikunci,
        // jangan terima input movement.
        if (movementLocked)
        {
            StopMovement();
            return;
        }

        moveInput = context.ReadValue<Vector2>();

        if (context.performed)
        {
            currentSpeed = walkSpeed;
        }
        else if (context.canceled)
        {
            currentSpeed = 0f;
        }

        if (moveInput != Vector2.zero)
        {
            animator.SetFloat("XInput", moveInput.x);
            animator.SetFloat("YInput", moveInput.y);
        }

        animator.SetBool(
            "IsWalking",
            currentSpeed == walkSpeed &&
            moveInput != Vector2.zero
        );

        animator.SetBool(
            "IsRunning",
            currentSpeed == runSpeed &&
            moveInput != Vector2.zero
        );
    }

    // =========================================================
    // RUN INPUT
    // =========================================================

    public void Run(InputAction.CallbackContext context)
    {
        if (movementLocked)
        {
            StopMovement();
            return;
        }

        if (context.performed)
        {
            currentSpeed = runSpeed;
        }
    }
}
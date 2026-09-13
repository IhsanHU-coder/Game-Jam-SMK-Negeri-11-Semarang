using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    float currentSpeed = 0f;
    [SerializeField]
    float walkSpeed = 5f;
    [SerializeField]

    float runSpeed = 10f;
    Rigidbody2D rb;
    Vector2 moveInput;
    InputManager inputManager;
    Animator animator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        inputManager = GetComponent<InputManager>();
        animator = GetComponent<Animator>();
    }


    // Update is called once per frame
    void Update()
    {
        rb.linearVelocity = moveInput * currentSpeed;

    }

    public void Move(InputAction.CallbackContext context)
    {
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
            currentSpeed == walkSpeed && moveInput != Vector2.zero
        );

        animator.SetBool(
            "IsRunning",
            currentSpeed == runSpeed && moveInput != Vector2.zero
        );

    }
    public void Run(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            currentSpeed = runSpeed;
        }
        // else if (context.canceled)
        // {
        //     currentSpeed = walkSpeed;
        // }
    }

}

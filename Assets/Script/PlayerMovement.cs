using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    float moveSpeed = 5f;
    float runSpeed = 10f;
    Rigidbody2D rb;
    Vector2 moveInput;
    InputManager inputManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        inputManager = GetComponent<InputManager>();
    }


    // Update is called once per frame
    void Update()
    {
        rb.linearVelocity = moveInput * moveSpeed;
    }

    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
    public void Run(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            moveSpeed = runSpeed;
        }
        else if (context.canceled)
        {
            moveSpeed = 5f;
        }
    }

}

using UnityEngine;
using UnityEngine.InputSystem;
using static InputSystem_Actions;

public class InputManager : MonoBehaviour, IPlayerActions
{
    private InputSystem_Actions playerInputAction;

    public Vector2 MoveInput { get; private set; }

    public bool RunInput { get; private set; }

    public bool AttackInput { get; private set; }
    public bool AttackPressed { get; private set; }

    private void Awake()
    {
        playerInputAction = new InputSystem_Actions();

        playerInputAction.Player.SetCallbacks(this);
    }

    private void OnEnable()
    {
        playerInputAction.Player.Enable();
    }

    private void OnDisable()
    {
        playerInputAction.Player.Disable();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        MoveInput = context.ReadValue<Vector2>();
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        RunInput = context.ReadValueAsButton();
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            AttackPressed = true;
            CutTree tree = FindObjectOfType<CutTree>();
            tree.TakeDamage();
        }
    }
    public void ResetAttack()
    {
        AttackPressed = false;
    }

    public void OnLook(InputAction.CallbackContext context)
    {
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
    }

    public void OnJump(InputAction.CallbackContext context)
    {
    }

    public void OnPrevious(InputAction.CallbackContext context)
    {
    }

    public void OnNext(InputAction.CallbackContext context)
    {
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
    }
}
using System.Collections;
using Unity.Cinemachine;
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

    [Header("Camera Shake")]
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private float hitAmplitude = 2f;
    [SerializeField] private float hitFrequency = 2f;
    [SerializeField] private float shakeDuration = 0.15f;

    private CinemachineBasicMultiChannelPerlin noisePerlin;
    private Coroutine shakeCoroutine;

    private void Awake()
    {
        playerInputAction = new InputSystem_Actions();

        playerInputAction.Player.SetCallbacks(this);
        

        if (cinemachineCamera != null)
        {
            noisePerlin = cinemachineCamera.GetCinemachineComponent(
                CinemachineCore.Stage.Noise
            ) as CinemachineBasicMultiChannelPerlin;
        }
    }
    public void Start()
    {
        OnEnable();
        AttackPressed = true;
    }

    public void OnEnable()
    {
        playerInputAction.Player.Enable();
    }

    public void OnDisable()
    {
        playerInputAction.Player.Disable();
    }

    // === Tambahan: kontrol manual enable/disable dari luar (misal saat buka menu Settings) ===
    public void EnablePlayerInput()
    {
        playerInputAction.Player.Enable();
    }

    public void DisablePlayerInput()
    {
        // Reset state supaya nggak "nyangkut" true saat re-enable nanti
        // MoveInput = Vector2.zero;
        // RunInput = false;
        // AttackPressed = false;

        // playerInputAction.Player.Disable();
    }

    public bool IsPlayerInputEnabled()
    {
        return playerInputAction.Player.enabled;
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
        }
    }

    public void ResetAttack()
    {
        AttackPressed = false;
    }

    public void PlayHitShake()
    {
        if (noisePerlin == null)
            return;

        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }

        shakeCoroutine = StartCoroutine(CameraShake());
    }

    private IEnumerator CameraShake()
    {
        noisePerlin.AmplitudeGain = hitAmplitude;
        noisePerlin.FrequencyGain = hitFrequency;

        yield return new WaitForSeconds(shakeDuration);

        noisePerlin.AmplitudeGain = 0f;
        noisePerlin.FrequencyGain = 0f;

        shakeCoroutine = null;
    }

    public void OnLook(InputAction.CallbackContext context) { }

    public void OnInteract(InputAction.CallbackContext context) { }

    public void OnCrouch(InputAction.CallbackContext context) { }

    public void OnJump(InputAction.CallbackContext context) { }

    public void OnPrevious(InputAction.CallbackContext context) { }

    public void OnNext(InputAction.CallbackContext context) { }

    public void OnSprint(InputAction.CallbackContext context) { }

    public void OnPause(InputAction.CallbackContext context) { }
}
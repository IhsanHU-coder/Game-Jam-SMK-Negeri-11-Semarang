using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Kontrol menu pause: dibuka/tutup dengan tombol ESC (New Input System),
/// mengatur Time.timeScale, dan menyediakan method untuk tombol Resume, Back to Menu & Quit.
/// </summary>
public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    [Header("References")]
    [Tooltip("Panel UI Settings/Pause yang mau ditampilkan/disembunyikan")]
    [SerializeField] private GameObject pausePanel;

    [Tooltip("Drag InputManager player di scene ke sini")]
    [SerializeField] private InputManager inputManager;

    [Header("Input (New Input System)")]
    [Tooltip("Drag Input Action 'Pause' yang sudah di-bind ke tombol ESC di sini")]
    [SerializeField] private InputActionReference pauseAction;

    [Header("Back to Menu Settings")]
    [Tooltip("Nama scene MainMenu yang akan di-load saat tombol Back to Menu ditekan.")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool isPaused;
    private bool isReturningToMenu; // guard supaya tombol tidak bisa dipencet dobel saat sedang loading

    public bool IsPaused => isPaused;

    private void Awake()
    {
        // Optional singleton, hapus kalau tidak perlu
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        if (pauseAction != null)
        {
            pauseAction.action.Enable();
            pauseAction.action.performed += OnPausePerformed;
        }
    }

    private void OnDisable()
    {
        if (pauseAction != null)
        {
            pauseAction.action.performed -= OnPausePerformed;
            pauseAction.action.Disable();
        }
    }

    private void Start()
    {
        // Pastikan panel tertutup & waktu normal saat game baru dimulai
        if (pausePanel != null)
            pausePanel.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;
    }

    private void OnPausePerformed(InputAction.CallbackContext ctx)
    {
        // Jangan biarkan ESC toggle pause lagi kalau lagi proses pindah ke menu
        if (isReturningToMenu) return;
        TogglePause();
    }

    public void TogglePause()
    {
        if (isPaused) ResumeGame();
        else PauseGame();
    }

    public void PauseGame()
{
    isPaused = true;
    if (pausePanel != null)
        pausePanel.SetActive(true);

    Time.timeScale = 0f;

    if (inputManager != null)
    {
        Debug.Log("Disabling player input...");
        inputManager.DisablePlayerInput();
    }
    else
    {
        Debug.LogWarning("inputManager is NULL! Assign it in Inspector.");
    }
}

    /// <summary>
    /// Panggil dari OnClick tombol "Resume" di Inspector.
    /// </summary>
    public void ResumeGame()
    {
        isPaused = false;
        if (pausePanel != null)
            pausePanel.SetActive(false);

        Time.timeScale = 1f;

        // Nyalakan lagi input gameplay
        if (inputManager != null)
            inputManager.EnablePlayerInput();
    }

    /// <summary>
    /// Panggil dari OnClick tombol "Back to Menu" di Inspector.
    /// Alurnya sama seperti loading MainMenu -> Gameplay: pausePanel fade out sambil
    /// loading screen fade in, load scene MainMenu, lalu fade out lagi.
    /// </summary>
    public void BackToMenu()
    {
        if (isReturningToMenu) return;
        isReturningToMenu = true;

        // PENTING: reset timeScale dulu SEBELUM load scene baru.
        // Kalau tidak, scene MainMenu yang baru dimuat ikut "freeze" karena
        // timeScale masih 0 peninggalan dari pause.
        Time.timeScale = 1f;

        CanvasGroup pauseCanvasGroup = pausePanel != null
            ? pausePanel.GetComponent<CanvasGroup>()
            : null;

        // mode Single -> scene Gameplay otomatis diganti, tidak perlu unload manual.
        LoadingScreenController.Instance.LoadScene(
            mainMenuSceneName,
            LoadSceneMode.Single,
            pauseCanvasGroup);
    }

    /// <summary>
    /// Panggil dari OnClick tombol "Quit Game" di Inspector.
    /// </summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
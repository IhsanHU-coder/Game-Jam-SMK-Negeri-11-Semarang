using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Kontrol menu pause: dibuka/tutup dengan tombol ESC (New Input System),
/// mengatur Time.timeScale, dan menyediakan method untuk tombol Resume & Quit.
/// </summary>
public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    [Header("References")]
    [Tooltip("Panel UI Settings/Pause yang mau ditampilkan/disembunyikan")]
    [SerializeField] private GameObject pausePanel;

    [Header("Input (New Input System)")]
    [Tooltip("Drag Input Action 'Pause' yang sudah di-bind ke tombol ESC di sini")]
    [SerializeField] private InputActionReference pauseAction;

    private bool isPaused;

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
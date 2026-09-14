using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Mengatur MainMenu: klik Play -> PanelMainMenu fade out sambil loading screen (persistent,
/// lihat LoadingScreenController) fade in -> load scene Gameplay -> setelah siap, loading
/// screen fade out -> delay sekian detik -> scene MainMenu ini di-unload.
/// Juga mengatur buka/tutup Panel Credits dan Panel Settings.
///
/// SETUP DI INSPECTOR:
/// - PanelMainMenu butuh komponen CanvasGroup (Add Component > Canvas Group).
/// - Pastikan ada 1 instance LoadingScreenController di scene ini (atau scene Bootstrap)
///   sebelum tombol Play ditekan.
/// - OnClick() Button "ButtonPlayContinueGame" -> OnPlayClicked().
/// - OnClick() Button "ButtonExit" -> OnExitClicked().
/// - OnClick() Button "ButtonCredits" -> OnCreditsClicked().
/// - OnClick() Button "ButtonBack" (di dalam PanelCredits) -> OnBackClicked().
/// - OnClick() Button "ButtonSettings" -> OnSettingsClicked().
/// - OnClick() Button "ButtonBack" (di dalam PanelSettings) -> OnCloseSettingsClicked().
/// - panelHidden: image yang TAMPIL dari awal (jangan di-nonaktifkan manual di Inspector),
///   otomatis hilang saat Settings dibuka, dan muncul lagi saat Settings ditutup.
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    /// <summary>
    /// Singleton instance, supaya script lain (misal CreditsScroller) bisa
    /// manggil MainMenuManager.Instance.CloseCredits() dsb.
    /// </summary>
    public static MainMenuManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private CanvasGroup panelMainMenu;
    [Tooltip("GameObject panel Credits, defaultnya nonaktif.")]
    [SerializeField] private GameObject panelCredits;
    [Tooltip("GameObject panel Settings, defaultnya nonaktif.")]
    [SerializeField] private GameObject panelSettings;
    [Tooltip("Image yang TAMPIL dari awal. Otomatis hilang saat panel Settings dibuka, dan muncul lagi saat Settings ditutup.")]
    [SerializeField] private GameObject panelHidden;

    [Header("Scene Settings")]
    [Tooltip("Nama scene Gameplay yang akan di-load (harus sudah masuk Build Settings).")]
    [SerializeField] private string gameplaySceneName = "Gameplay";
    [Tooltip("Aktifkan jika MainMenu di-load bareng Gameplay secara additive, lalu MainMenu " +
             "akan di-unload manual setelah loading selesai. Kalau dimatikan, scene akan " +
             "diganti langsung (Single) tanpa perlu unload manual.")]
    [SerializeField] private bool loadAdditive = false;
    [Tooltip("Jeda (detik) setelah loading screen fade out selesai, sebelum scene MainMenu di-unload.")]
    [SerializeField] private float delayBeforeUnloadMainMenu = 2f;
    public GameObject Canvas;

    private void Awake()
    {
        Instance = this;

        panelMainMenu.alpha = 1f;
        panelMainMenu.interactable = true;
        panelMainMenu.blocksRaycasts = true;

        // panelHidden TIDAK di-nonaktifkan di sini -> defaultnya tetap tampil saat start.

        if (panelCredits != null)
        {
            panelCredits.SetActive(false);
        }

        if (panelSettings != null)
        {
            panelSettings.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void LateUpdate()
    {
        panelMainMenu.alpha = 1f;
        panelMainMenu.interactable = true;
        panelMainMenu.blocksRaycasts = true;
    }

    /// <summary>
    /// Assign method ini ke Button "ButtonPlayContinueGame" -> OnClick() di Inspector.
    /// </summary>
    public void OnPlayClicked()
    {
        if (LoadingScreenController.Instance == null)
        {
            Debug.LogError("LoadingScreenController tidak ditemukan!");
            return;
        }

        LoadingScreenController.Instance.LoadScene(
            gameplaySceneName,
            LoadSceneMode.Single,
            panelMainMenu
        );

        Canvas.SetActive(false);
    }

    /// <summary>
    /// Assign method ini ke Button "ButtonExit" -> OnClick() di Inspector.
    /// </summary>
    public void OnExitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// Assign method ini ke Button "ButtonCredits" -> OnClick() di Inspector.
    /// </summary>
    public void OnCreditsClicked()
    {
        panelMainMenu.gameObject.SetActive(false);
        panelCredits.SetActive(true);
    }

    /// <summary>
    /// Assign method ini ke Button "ButtonBack" (di dalam PanelCredits) -> OnClick() di Inspector.
    /// </summary>
    public void OnBackClicked()
    {
        panelCredits.SetActive(false);
        panelMainMenu.gameObject.SetActive(true);
    }

    /// <summary>
    /// Dipanggil dari CreditsScroller (via MainMenuManager.Instance.CloseCredits())
    /// saat credits sudah selesai scroll atau ditutup manual (tombol Back/BackToMenu).
    /// Menyembunyikan panel Credits dan menampilkan lagi panel Main Menu.
    /// </summary>
    public void CloseCredits()
    {
        if (panelCredits != null)
            panelCredits.SetActive(false);

        if (panelMainMenu != null)
            panelMainMenu.gameObject.SetActive(true);
    }

    /// <summary>
    /// Assign method ini ke Button "ButtonSettings" -> OnClick() di Inspector.
    /// Membuka panel Settings dan menyembunyikan image panelHidden.
    /// </summary>
    public void OnSettingsClicked()
    {
        panelMainMenu.gameObject.SetActive(false);
        panelSettings.SetActive(true);

        if (panelHidden != null)
            panelHidden.SetActive(false);
    }

    /// <summary>
    /// Assign method ini ke Button "ButtonBack" (di dalam PanelSettings) -> OnClick() di Inspector.
    /// Menutup panel Settings dan menampilkan lagi image panelHidden.
    /// </summary>
    public void OnCloseSettingsClicked()
    {
        panelSettings.SetActive(false);
        panelMainMenu.gameObject.SetActive(true);

        if (panelHidden != null)
            panelHidden.SetActive(true);
    }

    /// <summary>
    /// Bisa dipanggil dari script lain (misal SettingsController) untuk menutup
    /// panel Settings dan kembali ke Main Menu secara programatis.
    /// </summary>
    public void CloseSettings()
    {
        if (panelSettings != null)
            panelSettings.SetActive(false);

        if (panelMainMenu != null)
            panelMainMenu.gameObject.SetActive(true);

        if (panelHidden != null)
            panelHidden.SetActive(true);
    }
}
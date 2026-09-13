using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashController : MonoBehaviour
{
    [Header("Scene names (harus persis sama seperti di Build Settings)")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Canvas splash (dipaksa selalu di atas MainMenu sampai fade-out selesai)")]
    [SerializeField] private Canvas splashCanvas;
    [SerializeField] private int splashSortingOrder = 999;

    [Header("Audio")]
    [Tooltip("AudioListener yang ada di scene Splash (biasanya nempel di Main Camera Splash)")]
    [SerializeField] private AudioListener splashAudioListener;

    [Header("Background panel (menutupi layar, tetap solid selama semua slide main)")]
    [SerializeField] private CanvasGroup backgroundCanvasGroup;
    [SerializeField] private float backgroundFadeOutDuration = 0.4f;

    [Header("Slides (logo studio, engine, game, dst — urut dari atas ke bawah)")]
    [SerializeField] private List<SplashSlide> slides = new List<SplashSlide>();

    [Header("Optional UI")]
    [Tooltip("Progress bar terpisah dari slide (opsional)")]
    [SerializeField] private UnityEngine.UI.Image progressBarFill;

    [Header("Debug")]
    [Tooltip("Matikan kalau sudah tidak butuh log lagi")]
    [SerializeField] private bool verboseLogging = true;

    private bool mainMenuLoaded;
    private Scene mainMenuScene;

    private readonly List<AudioListener> mainMenuAudioListeners = new List<AudioListener>();

    private void Start()
    {
        if (splashCanvas != null)
        {
            splashCanvas.overrideSorting = true;
            splashCanvas.sortingOrder = splashSortingOrder;
        }

        if (backgroundCanvasGroup != null) backgroundCanvasGroup.alpha = 1f;

        foreach (SplashSlide slide in slides)
        {
            if (slide.logoCanvasGroup != null) slide.logoCanvasGroup.alpha = 0f;
        }

        Log("Start() dipanggil, memulai coroutine paralel.");

        StartCoroutine(LoadMainMenuInBackground());
        StartCoroutine(PlaySlidesThenSwitchScene());
    }

    private IEnumerator LoadMainMenuInBackground()
    {
        Log($"Mulai FULL LOAD scene '{mainMenuSceneName}' dari awal (bareng splash), langsung aktif...");

        AsyncOperation loadOp = SceneManager.LoadSceneAsync(mainMenuSceneName, LoadSceneMode.Additive);
        if (loadOp == null)
        {
            Debug.LogError($"[Splash] SceneManager.LoadSceneAsync gagal untuk '{mainMenuSceneName}'. " +
                            $"Cek nama scene persis sama dengan di Build Settings.");
            yield break;
        }

        while (!loadOp.isDone)
        {
            UpdateProgressBar(loadOp.progress);
            yield return null;
        }

        mainMenuScene = SceneManager.GetSceneByName(mainMenuSceneName);
        if (!mainMenuScene.IsValid())
        {
            Debug.LogError($"[Splash] Scene '{mainMenuSceneName}' tidak valid setelah load selesai.");
            yield break;
        }

        mainMenuAudioListeners.Clear();
        foreach (GameObject root in mainMenuScene.GetRootGameObjects())
        {
            AudioListener[] listeners = root.GetComponentsInChildren<AudioListener>(true);
            foreach (AudioListener listener in listeners)
            {
                listener.enabled = false;
                mainMenuAudioListeners.Add(listener);
            }
        }

        UpdateProgressBar(1f);
        Log($"MainMenu FULL LOADED & AKTIF sejak awal. {mainMenuAudioListeners.Count} AudioListener MainMenu dimatikan sementara.");
        mainMenuLoaded = true;
    }

    private IEnumerator PlaySlidesThenSwitchScene()
    {
        int slideIndex = 0;
        foreach (SplashSlide slide in slides)
        {
            Log($"Memutar slide index {slideIndex}...");
            yield return PlaySingleSlide(slide);
            Log($"Slide index {slideIndex} selesai.");
            slideIndex++;
        }

        Log($"Semua slide selesai. mainMenuLoaded={mainMenuLoaded}");

        while (!mainMenuLoaded)
        {
            yield return null;
        }

        Log("Semua syarat terpenuhi. Menampilkan MainMenu sekarang (harusnya instan).");

        yield return SwitchToMainMenuAndUnloadSplash();
    }

    private IEnumerator PlaySingleSlide(SplashSlide slide)
    {
        if (slide.logoCanvasGroup == null)
        {
            Debug.LogWarning("[Splash] Ada slide dengan logoCanvasGroup == null, di-skip.");
            yield break;
        }

        yield return Fade(slide.logoCanvasGroup, 0f, 1f, slide.fadeInDuration);
        yield return new WaitForSecondsRealtime(slide.holdDuration);
        yield return Fade(slide.logoCanvasGroup, 1f, 0f, slide.fadeOutDuration);
    }

    private IEnumerator Fade(CanvasGroup cg, float from, float to, float duration)
    {
        if (duration <= 0f)
        {
            cg.alpha = to;
            yield break;
        }

        float elapsed = 0f;
        cg.alpha = from;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            cg.alpha = Mathf.Lerp(from, to, t * t * (3f - 2f * t));
            yield return null;
        }
        cg.alpha = to;
    }


    private IEnumerator SwitchToMainMenuAndUnloadSplash()
    {
        if (splashAudioListener != null)
        {
            splashAudioListener.enabled = false;
            Log("AudioListener Splash dimatikan.");
        }

        foreach (AudioListener listener in mainMenuAudioListeners)
        {
            if (listener == null) continue;
            listener.enabled = true;
        }
        Log($"{mainMenuAudioListeners.Count} AudioListener MainMenu dinyalakan kembali.");

        SceneManager.SetActiveScene(mainMenuScene);
        Log($"SetActiveScene -> {mainMenuSceneName}");

        yield return null;

        if (backgroundCanvasGroup != null)
        {
            Log("Fade-out PanelBackground...");
            yield return Fade(backgroundCanvasGroup, 1f, 0f, backgroundFadeOutDuration);
        }

        Log("Unload scene Splash...");
        Scene splashScene = gameObject.scene;
        yield return SceneManager.UnloadSceneAsync(splashScene);

        Log("Scene Splash sudah di-unload. Selesai.");
    }

    private void UpdateProgressBar(float value01)
    {
        if (progressBarFill != null)
        {
            progressBarFill.fillAmount = value01;
        }
    }

    private void Log(string message)
    {
        if (verboseLogging)
        {
            Debug.Log($"[Splash] {message}");
        }
    }
}
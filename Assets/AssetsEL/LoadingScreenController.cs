using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Loading screen persistent (DontDestroyOnLoad) yang bisa dipanggil dari scene manapun
/// (MainMenu -> Gameplay, atau Gameplay -> MainMenu saat Back to Menu dari Pause).
///
/// SETUP:
/// - Prefab-kan GameObject ini (Canvas + PanelLoading + script ini) supaya gampang dipakai ulang.
/// - Taruh 1 instance di scene paling awal (misal Bootstrap/MainMenu). Karena DontDestroyOnLoad,
///   dia akan tetap hidup walau scene lain berganti-ganti.
/// - Image "Loading Fill" HARUS pakai komponen Image (bukan RawImage), dengan:
///     Image Type = Filled, Fill Method = Vertical, Fill Origin = Bottom
/// </summary>
public class LoadingScreenController : MonoBehaviour
{
    public static LoadingScreenController Instance { get; private set; }

    [Header("References")]
    [SerializeField] private CanvasGroup panelLoading;
    [SerializeField] private Image loadingFillImage;
    [SerializeField] private TMP_Text loadingText;

    [Header("Fade Settings")]
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.5f;

    [Header("Loading Animation Settings")]
    [SerializeField] private float fillStepInterval = 0.3f;
    [SerializeField, Range(0.01f, 1f)] private float fillStepAmount = 0.25f;
    [SerializeField] private float dotAnimationInterval = 0.4f;

    private Coroutine fillRoutine;
    private Coroutine dotRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        panelLoading.alpha = 0f;
        panelLoading.interactable = false;
        panelLoading.blocksRaycasts = false;
    }

    /// <summary>
    /// Pindah ke scene baru sambil menampilkan loading screen ini.
    /// </summary>
    /// <param name="sceneName">Nama scene tujuan (harus ada di Build Settings).</param>
    /// <param name="mode">Single = scene lama otomatis diganti. Additive = scene lama tetap ada,
    /// dipakai kalau mau di-unload manual lewat sceneToUnloadAfterLoad.</param>
    /// <param name="panelToCrossFadeOut">Opsional: panel di scene asal yang mau di-fade out
    /// bersamaan loading screen fade in (misal PanelMainMenu). Boleh null.</param>
    /// <param name="sceneToUnloadAfterLoad">Opsional: scene yang di-unload manual setelah loading
    /// selesai (hanya relevan kalau mode = Additive).</param>
    /// <param name="delayBeforeUnload">Jeda detik setelah fade out selesai, sebelum unload scene di atas.</param>
    public void LoadScene(
        string sceneName,
        LoadSceneMode mode = LoadSceneMode.Single,
        CanvasGroup panelToCrossFadeOut = null,
        Scene? sceneToUnloadAfterLoad = null,
        float delayBeforeUnload = 2f)
    {
        StartCoroutine(LoadSceneRoutine(sceneName, mode, panelToCrossFadeOut, sceneToUnloadAfterLoad, delayBeforeUnload));
    }

    private IEnumerator LoadSceneRoutine(
        string sceneName,
        LoadSceneMode mode,
        CanvasGroup panelToCrossFadeOut,
        Scene? sceneToUnloadAfterLoad,
        float delayBeforeUnload)
    {
        // 1. Tampilkan loading screen. Kalau ada panel asal, cross-fade; kalau tidak, fade in biasa.
        panelLoading.blocksRaycasts = true;
        if (panelToCrossFadeOut != null)
        {
            panelToCrossFadeOut.interactable = false;
            yield return StartCoroutine(CrossFade(panelToCrossFadeOut, panelLoading, fadeInDuration));
        }
        else
        {
            yield return StartCoroutine(Fade(panelLoading, panelLoading.alpha, 1f, fadeInDuration));
        }
        panelLoading.interactable = true;

        // 2. Mulai animasi loading (dot text + fill image), looping sampai dihentikan.
        fillRoutine = StartCoroutine(AnimateLoadingFill());
        dotRoutine = StartCoroutine(AnimateLoadingDots());

        // 3. Load scene tujuan secara async, tahan aktivasinya dulu.
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, mode);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f) yield return null;
        op.allowSceneActivation = true;
        while (!op.isDone) yield return null;
        yield return new WaitForEndOfFrame();

        // 3b. Kalau scene lama masih akan di-unload nanti (delay), matikan dulu Camera, AudioListener,
        // dan EventSystem-nya SEKARANG JUGA supaya tidak ada 2 Main Camera / 2 AudioListener / 2 EventSystem
        // aktif bersamaan sebelum scene lama benar-benar di-unload. Ini mencegah script lain
        // (mis. PlayerAttack) salah ambil kamera lama, dan mencegah warning "2 event systems in the scene".
        if (sceneToUnloadAfterLoad.HasValue)
        {
            DisableDuplicateSingletonsInScene(sceneToUnloadAfterLoad.Value);
        }

        // 4. Hentikan animasi loading.
        if (fillRoutine != null) StopCoroutine(fillRoutine);
        if (dotRoutine != null) StopCoroutine(dotRoutine);

        // 5. Fade out loading screen.
        panelLoading.interactable = false;
        yield return StartCoroutine(Fade(panelLoading, panelLoading.alpha, 0f, fadeOutDuration));
        panelLoading.blocksRaycasts = false;

        // 6. Kalau ada scene lama yang perlu di-unload manual (mode Additive), tunggu delay lalu unload.
        if (sceneToUnloadAfterLoad.HasValue)
        {
            yield return new WaitForSeconds(delayBeforeUnload);
            SceneManager.UnloadSceneAsync(sceneToUnloadAfterLoad.Value);
        }
    }

    /// <summary>
    /// Matikan semua Camera, AudioListener, dan EventSystem yang ada di scene tertentu, supaya
    /// tidak bentrok dengan komponen sejenis di scene yang baru aktif sebelum scene lama
    /// benar-benar di-unload (mencegah warning "2 event systems" & referensi kamera stale).
    /// </summary>
    private void DisableDuplicateSingletonsInScene(Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Camera cam in root.GetComponentsInChildren<Camera>(true))
            {
                cam.gameObject.SetActive(false);
            }
            foreach (AudioListener listener in root.GetComponentsInChildren<AudioListener>(true))
            {
                listener.enabled = false;
            }
            foreach (EventSystem es in root.GetComponentsInChildren<EventSystem>(true))
            {
                es.gameObject.SetActive(false);
            }
        }
    }

    private IEnumerator Fade(CanvasGroup cg, float from, float to, float duration)
    {
        if (duration <= 0f)
        {
            cg.alpha = to;
            yield break;
        }

        float t = 0f;
        cg.alpha = from;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime; // pakai unscaled supaya tetap jalan walau Time.timeScale = 0 (saat pause)
            cg.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        cg.alpha = to;
    }

    private IEnumerator CrossFade(CanvasGroup groupOut, CanvasGroup groupIn, float duration)
    {
        float fromOut = groupOut.alpha;
        float fromIn = groupIn.alpha;

        if (duration <= 0f)
        {
            groupOut.alpha = 0f;
            groupIn.alpha = 1f;
        }
        else
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float lerp = t / duration;
                groupOut.alpha = Mathf.Lerp(fromOut, 0f, lerp);
                groupIn.alpha = Mathf.Lerp(fromIn, 1f, lerp);
                yield return null;
            }
            groupOut.alpha = 0f;
            groupIn.alpha = 1f;
        }

        groupOut.blocksRaycasts = false;
    }

    private IEnumerator AnimateLoadingFill()
    {
        float current = 0f;
        loadingFillImage.fillAmount = current;

        while (true)
        {
            current += fillStepAmount;
            if (current > 1f) current = 0f;

            loadingFillImage.fillAmount = current;
            yield return new WaitForSecondsRealtime(fillStepInterval); // realtime, tetap jalan saat timeScale = 0
        }
    }

    private IEnumerator AnimateLoadingDots()
    {
        string[] dots = { "Loading .", "Loading . .", "Loading . . ." };
        int index = 0;

        while (true)
        {
            loadingText.text = dots[index];
            index = (index + 1) % dots.Length;
            yield return new WaitForSecondsRealtime(dotAnimationInterval);
        }
    }
}
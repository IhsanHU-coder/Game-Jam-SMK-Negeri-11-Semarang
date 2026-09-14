using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; // Hapus baris ini dan ganti Text jika tidak pakai TextMeshPro

public class CutsceneController : MonoBehaviour
{
    [Header("Panel Background (Wadah Semua Konten)")]
    [Tooltip("CanvasGroup paling luar yang membungkus SEMUA konten cutscene (TV panel, dialog, teks tengah). " +
             "Alpha-nya akan di-set 1 (full opaque) sejak awal TANPA fade in, dan baru fade out di paling akhir " +
             "setelah scene Gameplay selesai di-load, supaya transisinya smooth.")]
    public CanvasGroup panelBackgroundCanvasGroup;

    [Header("TV Panel (Video + Frame TV)")]
    [Tooltip("CanvasGroup yang membungkus panel TV (video + frame). Fade in & fade out BARENG dengan dialog.")]
    public CanvasGroup tvPanelCanvasGroup;

    [Header("Dialog / Story Text")]
    public TMP_Text storyText; // Jika pakai UI Text biasa, ganti jadi: public Text storyText;
    [Tooltip("CanvasGroup yang membungkus DialoguePanel (parent dari storyText), BUKAN CanvasGroup di TMP-nya langsung. " +
             "Fade in & fade out BARENG dengan tvPanelCanvasGroup.")]
    public CanvasGroup dialoguePanelCanvasGroup;
    public Button nextButton; // klik untuk skip animasi ketik, atau skip jeda (hold) supaya langsung lanjut

    [Header("Isi Cerita")]
    [TextArea(2, 4)]
    public string[] storyLines;

    [Header("Teks Tengah (Transisi setelah Dialog Selesai)")]
    [Tooltip("TMP_Text yang muncul di tengah layar setelah TV panel & dialog fade out bareng.")]
    public TMP_Text middleText;
    public CanvasGroup middleTextCanvasGroup;
    [TextArea(1, 2)]
    public string middleTextContent = "Player sudah diterjunkan ke lokasi";

    [Header("Animasi Ketik (Typewriter)")]
    [Tooltip("Jeda antar huruf saat mengetik, dalam detik")]
    public float typingSpeed = 0.04f;
    [Tooltip("Berapa lama teks diam penuh sebelum lanjut/hilang, setelah selesai diketik")]
    public float holdDuration = 1.5f;

    [Header("Durasi Fade")]
    public float introFadeInDuration = 0.8f;     // Fade in TV panel + dialog bareng di awal
    public float outroFadeOutDuration = 0.8f;    // Fade out TV panel + dialog bareng setelah cerita selesai
    public float middleTextFadeInDuration = 0.6f;
    public float middleTextHoldDuration = 1.5f;
    public float middleTextFadeOutDuration = 0.6f;
    public float finalPanelFadeOutDuration = 1.0f; // Fade out PanelBackground di paling akhir

    [Header("Tujuan Scene Berikutnya")]
    public string gameplaySceneName = "Gameplay";

    [Header("Cleanup Scene Prolog")]
    [Tooltip("Nama scene cutscene/prolog ini sendiri, yang akan di-unload/destroy setelah PanelBackground selesai fade out.")]
    public string prologSceneName = "PrologScene";
    [Tooltip("Jeda dalam detik SETELAH PanelBackground selesai fade out, sebelum PrologScene di-unload/destroy.")]
    public float destroyDelayAfterFadeOut = 2f;

    [Header("Mode Testing")]
    [Tooltip("Jika dicentang, di akhir cerita TIDAK akan load scene Gameplay ataupun fade out PanelBackground, " +
             "hanya muncul log di Console. Gunakan untuk mengetes animasi ketik & fade tanpa mengganggu scene lain. " +
             "Matikan sebelum build/rilis.")]
    public bool testMode = false;

    private int currentLine = 0;
    private bool isTyping = false;
    private bool skipTyping = false;
    private bool skipHold = false;

    private void Start()
    {
        if (nextButton != null)
        {
            nextButton.onClick.AddListener(OnNextPressed);
        }

        // PanelBackground full opaque sejak awal, TIDAK di-fade in
        if (panelBackgroundCanvasGroup != null) panelBackgroundCanvasGroup.alpha = 1f;

        // TV panel & DialoguePanel mulai transparan, nanti di-fade in bareng
        if (tvPanelCanvasGroup != null) tvPanelCanvasGroup.alpha = 0f;
        if (dialoguePanelCanvasGroup != null) dialoguePanelCanvasGroup.alpha = 0f;

        // Teks tengah mulai transparan
        if (middleTextCanvasGroup != null) middleTextCanvasGroup.alpha = 0f;
        if (middleText != null) middleText.text = middleTextContent;

        StartCoroutine(RunCutsceneSequence());
    }

    private void OnDestroy()
    {
        if (nextButton != null)
        {
            nextButton.onClick.RemoveListener(OnNextPressed);
        }
    }

    /// <summary>
    /// Alur utama cutscene:
    /// 1) Fade in TV panel + dialog bareng
    /// 2) Mainkan semua baris dialog (typewriter)
    /// 3) Fade out TV panel + dialog bareng
    /// 4) Fade in teks tengah "Player sudah diterjunkan ke lokasi"
    /// 5) Load scene Gameplay secara additive di belakang
    /// 6) Fade out teks tengah
    /// 7) Fade out PanelBackground (Gameplay sudah siap di belakang -> transisi smooth)
    /// </summary>
    private IEnumerator RunCutsceneSequence()
    {
        // 1) Fade in TV panel + DialoguePanel BARENG
        yield return StartCoroutine(FadeCanvasGroupsTogether(
            new[] { tvPanelCanvasGroup, dialoguePanelCanvasGroup }, 0f, 1f, introFadeInDuration));

        // 2) Mainkan semua baris dialog
        if (storyLines != null && storyLines.Length > 0)
        {
            yield return StartCoroutine(PlayAllLines());
        }

        // 3) Fade out TV panel + DialoguePanel BARENG
        yield return StartCoroutine(FadeCanvasGroupsTogether(
            new[] { tvPanelCanvasGroup, dialoguePanelCanvasGroup }, 1f, 0f, outroFadeOutDuration));

        // 4) Fade in teks tengah
        yield return StartCoroutine(FadeCanvasGroupsTogether(
            new[] { middleTextCanvasGroup }, 0f, 1f, middleTextFadeInDuration));

        AsyncOperation loadOp = null;

        if (!testMode)
        {
            // 5) Begitu teks tengah sudah muncul, mulai load Gameplay secara additive di belakang
            loadOp = SceneManager.LoadSceneAsync(gameplaySceneName, LoadSceneMode.Additive);
        }

        // Tahan sebentar biar pemain sempat baca teks tengah (bisa di-skip via tombol Next)
        yield return StartCoroutine(WaitOrSkip(middleTextHoldDuration));

        // Pastikan scene Gameplay benar-benar selesai di-load sebelum lanjut fade out,
        // supaya pas PanelBackground fade out, Gameplay sudah 100% siap (tidak nge-lag/blink)
        while (loadOp != null && !loadOp.isDone)
        {
            yield return null;
        }

        // 6) Fade out teks tengah
        yield return StartCoroutine(FadeCanvasGroupsTogether(
            new[] { middleTextCanvasGroup }, 1f, 0f, middleTextFadeOutDuration));

        // 7) Fade out PanelBackground (menampilkan Gameplay yang sudah ada di belakang)
        if (!testMode)
        {
            yield return StartCoroutine(FadeCanvasGroupsTogether(
                new[] { panelBackgroundCanvasGroup }, 1f, 0f, finalPanelFadeOutDuration));

            // 8) Tunggu beberapa detik SETELAH fade out selesai, baru destroy/unload PrologScene
            yield return new WaitForSeconds(destroyDelayAfterFadeOut);

            if (!string.IsNullOrEmpty(prologSceneName))
            {
                SceneManager.UnloadSceneAsync(prologSceneName);
            }
        }
        else
        {
            Debug.Log("[CutsceneController] Cutscene SELESAI. (Test Mode aktif, tidak load Gameplay, tidak fade out PanelBackground, tidak destroy PrologScene). " +
                      "Matikan 'Test Mode' di Inspector jika sudah siap untuk rilis.");
        }
    }

    private IEnumerator PlayAllLines()
    {
        for (int i = 0; i < storyLines.Length; i++)
        {
            currentLine = i;
            yield return StartCoroutine(TypeLine(storyLines[i]));
            yield return StartCoroutine(WaitOrSkip(holdDuration));

            // Kosongkan teks sebelum baris berikutnya, kecuali baris terakhir
            // (baris terakhir dibiarkan terlihat supaya fade out bareng TV panel di step 3)
            if (i < storyLines.Length - 1)
            {
                storyText.text = "";
            }
        }
    }

    private IEnumerator TypeLine(string line)
    {
        if (storyText == null) yield break;

        isTyping = true;
        skipTyping = false;
        storyText.text = "";

        foreach (char c in line)
        {
            if (skipTyping)
            {
                storyText.text = line;
                break;
            }
            storyText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    /// <summary>
    /// Dipanggil oleh tombol Next. Ada 2 mode skip:
    /// - Kalau teks masih dalam proses mengetik -> langsung tampilkan seluruh kalimat (skip animasi ketik).
    /// - Kalau teks sudah selesai diketik dan sedang dalam masa jeda (hold) -> langsung lewati jeda tersebut
    ///   supaya cepat lanjut ke baris berikutnya / step berikutnya.
    /// </summary>
    public void OnNextPressed()
    {
        if (isTyping)
        {
            skipTyping = true;
        }
        else
        {
            skipHold = true;
        }
    }

    /// <summary>
    /// Menunggu selama 'duration' detik, TAPI bisa dipotong lebih awal kalau skipHold di-trigger
    /// (misalnya lewat tombol Next).
    /// </summary>
    private IEnumerator WaitOrSkip(float duration)
    {
        skipHold = false;
        float time = 0f;

        while (time < duration && !skipHold)
        {
            time += Time.deltaTime;
            yield return null;
        }

        skipHold = false;
    }

    /// <summary>
    /// Fade beberapa CanvasGroup sekaligus secara BARENG (nilai alpha berjalan sinkron).
    /// Aman dipanggil walau salah satu elemen di array null (akan di-skip).
    /// </summary>
    private IEnumerator FadeCanvasGroupsTogether(CanvasGroup[] groups, float from, float to, float duration)
    {
        foreach (var g in groups)
        {
            if (g != null) g.alpha = from;
        }

        if (duration <= 0f)
        {
            foreach (var g in groups)
            {
                if (g != null) g.alpha = to;
            }
            yield break;
        }

        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);
            float value = Mathf.Lerp(from, to, t);

            foreach (var g in groups)
            {
                if (g != null) g.alpha = value;
            }

            yield return null;
        }

        foreach (var g in groups)
        {
            if (g != null) g.alpha = to;
        }
    }
}
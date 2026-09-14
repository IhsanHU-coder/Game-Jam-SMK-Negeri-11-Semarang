using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class CutsceneController : MonoBehaviour
{
    [Header("Panel Background (Wadah Semua Konten)")]
    [Tooltip("CanvasGroup paling luar yang membungkus SEMUA konten cutscene.")]
    public CanvasGroup panelBackgroundCanvasGroup;

    [Header("TV Panel (Video + Frame TV)")]
    [Tooltip("CanvasGroup yang membungkus panel TV (video + frame). Fade in & fade out BARENG dengan dialog.")]
    public CanvasGroup tvPanelCanvasGroup;

    [Header("Dialog / Story Text")]
    public TMP_Text storyText;

    [Tooltip("CanvasGroup yang membungkus DialoguePanel (parent dari storyText).")]
    public CanvasGroup dialoguePanelCanvasGroup;

    [Tooltip("Klik untuk skip animasi ketik atau skip jeda.")]
    public Button nextButton;

    [Header("Isi Cerita")]
    [TextArea(2, 4)]
    public string[] storyLines;

    [Header("Teks Tengah (Transisi setelah Dialog Selesai)")]
    public TMP_Text middleText;
    public CanvasGroup middleTextCanvasGroup;

    [TextArea(1, 2)]
    public string middleTextContent = "Player sudah diterjunkan ke lokasi";

    [Header("Animasi Ketik (Typewriter)")]

    [Tooltip(
        "Jeda antar tick mengetik dalam detik. " +
        "Semakin kecil semakin cepat. Characters Per Tick menentukan " +
        "berapa karakter muncul sekaligus."
    )]
    public float typingSpeed = 0.05f;

    [Tooltip(
        "Jumlah karakter yang muncul setiap tick. " +
        "1 = normal, 2-5 = cepat, 10+ = sangat cepat."
    )]
    public int charactersPerTick = 3;

    [Tooltip(
        "Berapa lama teks diam penuh sebelum lanjut/hilang, setelah selesai diketik."
    )]
    public float holdDuration = 1.5f;

    [Header("Durasi Fade")]
    public float introFadeInDuration = 0.8f;
    public float outroFadeOutDuration = 0.8f;
    public float middleTextFadeInDuration = 0.6f;
    public float middleTextHoldDuration = 1.5f;
    public float middleTextFadeOutDuration = 0.6f;
    public float finalPanelFadeOutDuration = 1.0f;

    [Header("Tujuan Scene Berikutnya")]
    public string gameplaySceneName = "Gameplay";

    [Header("Cleanup Scene Prolog")]
    [Tooltip(
        "Nama scene cutscene/prolog ini sendiri, yang akan di-unload/destroy " +
        "setelah PanelBackground selesai fade out."
    )]
    public string prologSceneName = "PrologScene";

    [Tooltip(
        "Jeda dalam detik SETELAH PanelBackground selesai fade out, " +
        "sebelum PrologScene di-unload/destroy."
    )]
    public float destroyDelayAfterFadeOut = 2f;

    [Header("Mode Testing")]
    [Tooltip(
        "Jika dicentang, di akhir cerita TIDAK akan load scene Gameplay " +
        "ataupun fade out PanelBackground."
    )]
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

        // PanelBackground full opaque sejak awal
        if (panelBackgroundCanvasGroup != null)
            panelBackgroundCanvasGroup.alpha = 1f;

        // TV panel & DialoguePanel mulai transparan
        if (tvPanelCanvasGroup != null)
            tvPanelCanvasGroup.alpha = 0f;

        if (dialoguePanelCanvasGroup != null)
            dialoguePanelCanvasGroup.alpha = 0f;

        // Teks tengah mulai transparan
        if (middleTextCanvasGroup != null)
            middleTextCanvasGroup.alpha = 0f;

        if (middleText != null)
            middleText.text = middleTextContent;

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
    /// 1) Fade in TV panel + dialog
    /// 2) Mainkan semua baris dialog
    /// 3) Fade out TV panel + dialog
    /// 4) Fade in teks tengah
    /// 5) Load scene Gameplay secara additive
    /// 6) Fade out teks tengah
    /// 7) Fade out PanelBackground
    /// 8) Unload PrologScene
    /// </summary>
    private IEnumerator RunCutsceneSequence()
    {
        // =========================================================
        // 1. FADE IN TV PANEL + DIALOG
        // =========================================================

        yield return StartCoroutine(
            FadeCanvasGroupsTogether(
                new[] { tvPanelCanvasGroup, dialoguePanelCanvasGroup },
                0f,
                1f,
                introFadeInDuration
            )
        );


        // =========================================================
        // 2. MAINKAN SEMUA BARIS DIALOG
        // =========================================================

        if (storyLines != null && storyLines.Length > 0)
        {
            yield return StartCoroutine(PlayAllLines());
        }


        // =========================================================
        // 3. FADE OUT TV PANEL + DIALOG
        // =========================================================

        yield return StartCoroutine(
            FadeCanvasGroupsTogether(
                new[] { tvPanelCanvasGroup, dialoguePanelCanvasGroup },
                1f,
                0f,
                outroFadeOutDuration
            )
        );


        // =========================================================
        // 4. FADE IN TEKS TENGAH
        // =========================================================

        yield return StartCoroutine(
            FadeCanvasGroupsTogether(
                new[] { middleTextCanvasGroup },
                0f,
                1f,
                middleTextFadeInDuration
            )
        );


        AsyncOperation loadOp = null;


        // =========================================================
        // 5. LOAD GAMEPLAY ADDITIVE
        // =========================================================

        if (!testMode)
        {
            loadOp = SceneManager.LoadSceneAsync(
                gameplaySceneName,
                LoadSceneMode.Additive
            );
        }


        // =========================================================
        // TAHAN TEKS TENGAH
        // =========================================================

        yield return StartCoroutine(
            WaitOrSkip(middleTextHoldDuration)
        );


        // =========================================================
        // PASTIKAN GAMEPLAY SELESAI LOAD
        // =========================================================

        while (loadOp != null && !loadOp.isDone)
        {
            yield return null;
        }


        // =========================================================
        // 6. FADE OUT TEKS TENGAH
        // =========================================================

        yield return StartCoroutine(
            FadeCanvasGroupsTogether(
                new[] { middleTextCanvasGroup },
                1f,
                0f,
                middleTextFadeOutDuration
            )
        );


        // =========================================================
        // 7. FADE OUT PANEL BACKGROUND
        // =========================================================

        if (!testMode)
        {
            yield return StartCoroutine(
                FadeCanvasGroupsTogether(
                    new[] { panelBackgroundCanvasGroup },
                    1f,
                    0f,
                    finalPanelFadeOutDuration
                )
            );


            // =====================================================
            // 8. TUNGGU SEBELUM UNLOAD PROLOG
            // =====================================================

            yield return new WaitForSeconds(
                destroyDelayAfterFadeOut
            );


            if (!string.IsNullOrEmpty(prologSceneName))
            {
                SceneManager.UnloadSceneAsync(
                    prologSceneName
                );
            }
        }
        else
        {
            Debug.Log(
                "[CutsceneController] Cutscene SELESAI. " +
                "(Test Mode aktif, tidak load Gameplay, " +
                "tidak fade out PanelBackground, " +
                "tidak destroy PrologScene)."
            );
        }
    }


    // =============================================================
    // PLAY SEMUA DIALOG
    // =============================================================

    private IEnumerator PlayAllLines()
    {
        for (int i = 0; i < storyLines.Length; i++)
        {
            currentLine = i;

            // Ketik satu kalimat
            yield return StartCoroutine(
                TypeLine(storyLines[i])
            );


            // Tunggu setelah kalimat selesai
            yield return StartCoroutine(
                WaitOrSkip(holdDuration)
            );


            // Kosongkan teks sebelum kalimat berikutnya
            if (i < storyLines.Length - 1)
            {
                storyText.text = "";
            }
        }
    }


    // =============================================================
    // TYPEWRITER
    // =============================================================

    private IEnumerator TypeLine(string line)
    {
        if (storyText == null)
            yield break;

        isTyping = true;
        skipTyping = false;

        storyText.text = "";

        int index = 0;


        // Jika charactersPerTick kurang dari 1,
        // otomatis dianggap 1.
        int charsPerTick = Mathf.Max(
            1,
            charactersPerTick
        );


        // =========================================================
        // LOOP TYPEWRITER
        // =========================================================

        while (index < line.Length)
        {
            // -----------------------------------------------------
            // SKIP TYPEWRITER
            // -----------------------------------------------------

            if (skipTyping)
            {
                storyText.text = line;
                break;
            }


            // -----------------------------------------------------
            // HITUNG JUMLAH KARAKTER
            // -----------------------------------------------------

            int remainingCharacters =
                line.Length - index;

            int count =
                Mathf.Min(
                    charsPerTick,
                    remainingCharacters
                );


            // -----------------------------------------------------
            // TAMBAHKAN KARAKTER SEKALIGUS
            // -----------------------------------------------------

            storyText.text += line.Substring(
                index,
                count
            );

            index += count;


            // -----------------------------------------------------
            // WAIT
            // -----------------------------------------------------

            if (index < line.Length)
            {
                // Jika typingSpeed <= 0,
                // langsung lanjut tanpa delay.
                if (typingSpeed > 0f)
                {
                    yield return new WaitForSeconds(
                        typingSpeed
                    );
                }
                else
                {
                    // Tetap kasih satu frame supaya
                    // coroutine tidak membuat loop berat.
                    yield return null;
                }
            }
        }


        isTyping = false;
    }


    // =============================================================
    // NEXT BUTTON
    // =============================================================

    /// <summary>
    /// Jika teks sedang diketik:
    /// -> langsung tampilkan seluruh kalimat.
    ///
    /// Jika teks sudah selesai:
    /// -> skip hold duration.
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


    // =============================================================
    // WAIT OR SKIP
    // =============================================================

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


    // =============================================================
    // FADE CANVAS GROUP
    // =============================================================

    private IEnumerator FadeCanvasGroupsTogether(
        CanvasGroup[] groups,
        float from,
        float to,
        float duration
    )
    {
        // Set nilai awal
        foreach (var g in groups)
        {
            if (g != null)
            {
                g.alpha = from;
            }
        }


        // Kalau durasi 0 atau negatif,
        // langsung set ke nilai akhir.
        if (duration <= 0f)
        {
            foreach (var g in groups)
            {
                if (g != null)
                {
                    g.alpha = to;
                }
            }

            yield break;
        }


        float time = 0f;


        while (time < duration)
        {
            time += Time.deltaTime;


            float t =
                Mathf.Clamp01(
                    time / duration
                );


            float value =
                Mathf.Lerp(
                    from,
                    to,
                    t
                );


            foreach (var g in groups)
            {
                if (g != null)
                {
                    g.alpha = value;
                }
            }


            yield return null;
        }


        // Pastikan nilai akhir benar-benar tercapai
        foreach (var g in groups)
        {
            if (g != null)
            {
                g.alpha = to;
            }
        }
    }
}
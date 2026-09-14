using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Hapus baris ini dan ganti Text jika tidak pakai TextMeshPro
public class CutsceneController : MonoBehaviour
{
    [Header("Referensi UI")]
    public Image cutsceneImage;
    public TMP_Text storyText; // Jika pakai UI Text biasa, ganti jadi: public Text storyText;
    public CanvasGroup textCanvasGroup; // CanvasGroup pembungkus storyText, untuk efek fade out
    public Button nextButton; // opsional: klik untuk skip animasi ketik atau lanjut manual

    [Header("Isi Cerita")]
    [TextArea(2, 4)]
    public string[] storyLines;

    [Header("Animasi Ketik (Typewriter)")]
    [Tooltip("Jeda antar huruf saat mengetik, dalam detik")]
    public float typingSpeed = 0.04f;
    [Tooltip("Berapa lama teks diam penuh sebelum mulai menghilang, setelah selesai diketik")]
    public float holdDuration = 1.5f;
    [Tooltip("Lama durasi animasi teks menghilang (fade out)")]
    public float fadeOutDuration = 0.6f;

    [Header("Mode Lanjut")]
    [Tooltip("Jika true, baris berikutnya muncul otomatis setelah teks menghilang. Jika false, pemain klik NextButton untuk lanjut ke baris berikutnya.")]
    public bool autoAdvance = true;

    [Header("Tujuan Scene Berikutnya")]
    public string gameplaySceneName = "Gameplay";

    [Header("Mode Testing")]
    [Tooltip("Jika dicentang, di akhir cerita TIDAK akan pindah scene, hanya muncul log di Console. Gunakan untuk mengetes animasi ketik & fade tanpa mengganggu scene lain. Matikan sebelum build/rilis.")]
    public bool testMode = false;

    private int currentLine = 0;
    private Coroutine lineRoutine;
    private bool isTyping = false;

    private void Start()
    {
        if (nextButton != null)
        {
            nextButton.onClick.AddListener(OnNextPressed);
        }

        if (storyLines.Length > 0)
        {
            lineRoutine = StartCoroutine(PlayLine(0));
        }
    }

    private void OnDestroy()
    {
        if (nextButton != null)
        {
            nextButton.onClick.RemoveListener(OnNextPressed);
        }
    }
    private IEnumerator PlayLine(int index)
    {
        if (storyText == null || index < 0 || index >= storyLines.Length) yield break;

        // Pastikan teks terlihat penuh (alpha 1) sebelum mulai mengetik baris baru
        if (textCanvasGroup != null) textCanvasGroup.alpha = 1f;

        // --- Efek ketik (typewriter) ---
        isTyping = true;
        storyText.text = "";
        string fullLine = storyLines[index];

        foreach (char c in fullLine)
        {
            storyText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;

        // --- Diam sejenak supaya pemain sempat baca ---
        yield return new WaitForSeconds(holdDuration);

        // --- Teks menghilang (fade out) ---
        yield return StartCoroutine(FadeTextOut());

        // --- Lanjut otomatis ke baris berikutnya / atau selesai ---
        if (autoAdvance)
        {
            AdvanceToNextLine();
        }
    }

    private IEnumerator FadeTextOut()
    {
        if (textCanvasGroup == null) yield break;

        float startAlpha = textCanvasGroup.alpha;
        float time = 0f;

        while (time < fadeOutDuration)
        {
            time += Time.deltaTime;
            textCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, time / fadeOutDuration);
            yield return null;
        }

        textCanvasGroup.alpha = 0f;
    }

    /// <summary>
    /// Dipanggil oleh tombol Lanjut (opsional). Jika teks masih dalam proses mengetik,
    /// klik akan langsung menampilkan seluruh kalimat (skip animasi ketik).
    /// Jika teks sudah selesai diketik, klik akan memaksa lanjut ke baris berikutnya.
    /// </summary>
    public void OnNextPressed()
    {
        if (isTyping)
        {
            // Skip animasi ketik: langsung tampilkan kalimat penuh
            if (lineRoutine != null) StopCoroutine(lineRoutine);
            storyText.text = storyLines[currentLine];
            isTyping = false;
            lineRoutine = StartCoroutine(FinishLineThenAdvance());
        }
        else if (!autoAdvance)
        {
            // Mode manual: paksa lanjut ke baris berikutnya
            if (lineRoutine != null) StopCoroutine(lineRoutine);
            StartCoroutine(SkipHoldThenAdvance());
        }
    }

    private IEnumerator FinishLineThenAdvance()
    {
        yield return new WaitForSeconds(holdDuration);
        yield return StartCoroutine(FadeTextOut());
        AdvanceToNextLine();
    }

    private IEnumerator SkipHoldThenAdvance()
    {
        yield return StartCoroutine(FadeTextOut());
        AdvanceToNextLine();
    }

    private void AdvanceToNextLine()
    {
        currentLine++;

        if (currentLine < storyLines.Length)
        {
            lineRoutine = StartCoroutine(PlayLine(currentLine));
        }
        else
        {
            // Cerita selesai -> pindah ke gameplay dengan fade
            GoToGameplay();
        }
    }

    private void GoToGameplay()
    {
        if (testMode)
        {
            // Mode testing: jangan pindah scene, cukup tampilkan log
            Debug.Log("[CutsceneController] Cutscene SELESAI. (Test Mode aktif, tidak pindah scene). " +
                      "Matikan 'Test Mode' di Inspector jika sudah siap pindah ke scene gameplay.");
            return;
        }

        if (SceneFader.Instance != null)
        {
            SceneFader.Instance.LoadScene(gameplaySceneName);
        }
        else
        {
            // Fallback jika SceneFader belum ada di scene manapun
            UnityEngine.SceneManagement.SceneManager.LoadScene(gameplaySceneName);
        }
    }
}
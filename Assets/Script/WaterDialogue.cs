using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WaterDialogue : MonoBehaviour
{
    [Header("Popup")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private CanvasGroup textCanvasGroup;
    [SerializeField] private Button continueButton;

    // =========================================================
    // OPENING DIALOGUE
    // =========================================================

    [Header("Isi Dialog Opening")]
    [TextArea(2, 4)]
    [SerializeField]
    private string[] openingDialogueLines =
    {
        "Tanah ini akhirnya kembali pulih.",
        "Setelah sekian lama, tanah ini telah kembali memiliki kondisi yang layak untuk kehidupan baru.",
        "Kini saatnya menanam kembali pohon-pohon yang akan menghidupkan tanah ini."
    };

    // =========================================================
    // DIALOGUE SETELAH SELESAI MENANAM
    // =========================================================

    [Header("Isi Dialog Awal")]
    [TextArea(2, 4)]
    [SerializeField]
    private string[] dialogueLines =
    {
        "Semua benih sudah ditanam.",
        "Sekarang waktunya menyiram tanaman."
    };

    // =========================================================
    // ENDING DIALOGUE
    // =========================================================

    [Header("Isi Dialog Ending")]
    [TextArea(2, 4)]
    [SerializeField]
    private string[] endingDialogueLines =
    {
        "Akhirnya tugasku selesai",
        "Sekarang giliran alam untuk memperbaiki dirinya sendiri"
    };

    // =========================================================
    // TYPEWRITER
    // =========================================================

    [Header("Animasi Ketik (Typewriter)")]
    [SerializeField] private float typingSpeed = 0.04f;
    [SerializeField] private float holdDuration = 1.5f;
    [SerializeField] private float fadeOutDuration = 0.6f;

    // =========================================================
    // MODE
    // =========================================================

    [Header("Mode Lanjut")]
    [SerializeField] private bool autoAdvance = false;

    public bool IsShowing { get; private set; }

    public event Action OnDialogueFinished;

    private int currentLine = 0;
    private Coroutine lineRoutine;
    private bool isTyping = false;

    // Dialog yang sedang dimainkan
    private string[] normalDialogueLines;

    private void Awake()
    {
        // Simpan dialog setelah planting
        normalDialogueLines = dialogueLines;

        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnNextPressed);
        }

        Hide();
    }

    private void OnDestroy()
    {
        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(OnNextPressed);
        }
    }

    // =========================================================
    // OPENING DIALOGUE
    // Dipanggil saat scene farming baru dimulai
    // =========================================================

    public void ShowOpening()
    {
        dialogueLines = openingDialogueLines;

        IsShowing = true;
        currentLine = 0;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (textCanvasGroup != null)
            textCanvasGroup.alpha = 1f;

        if (lineRoutine != null)
        {
            StopCoroutine(lineRoutine);
            lineRoutine = null;
        }

        if (dialogueLines != null &&
            dialogueLines.Length > 0)
        {
            lineRoutine = StartCoroutine(
                PlayLine(currentLine)
            );
        }
        else
        {
            FinishDialogue();
        }
    }

    // =========================================================
    // DIALOG AWAL
    // Dipanggil setelah semua benih selesai ditanam
    // =========================================================

    public void Show()
    {
        // Pastikan menggunakan dialog normal
        dialogueLines = normalDialogueLines;

        IsShowing = true;
        currentLine = 0;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (textCanvasGroup != null)
            textCanvasGroup.alpha = 1f;

        if (lineRoutine != null)
        {
            StopCoroutine(lineRoutine);
            lineRoutine = null;
        }

        if (dialogueLines != null &&
            dialogueLines.Length > 0)
        {
            lineRoutine = StartCoroutine(
                PlayLine(currentLine)
            );
        }
        else
        {
            FinishDialogue();
        }
    }

    // =========================================================
    // DIALOG ENDING
    // Dipanggil setelah semua tanaman selesai disiram
    // =========================================================

    public void ShowEnding()
    {
        dialogueLines = endingDialogueLines;

        IsShowing = true;
        currentLine = 0;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (textCanvasGroup != null)
            textCanvasGroup.alpha = 1f;

        if (lineRoutine != null)
        {
            StopCoroutine(lineRoutine);
            lineRoutine = null;
        }

        if (dialogueLines != null &&
            dialogueLines.Length > 0)
        {
            lineRoutine = StartCoroutine(
                PlayLine(currentLine)
            );
        }
        else
        {
            FinishDialogue();
        }
    }

    // =========================================================
    // HIDE
    // =========================================================

    public void Hide()
    {
        IsShowing = false;
        isTyping = false;

        if (lineRoutine != null)
        {
            StopCoroutine(lineRoutine);
            lineRoutine = null;
        }

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    // =========================================================
    // PLAY LINE
    // =========================================================

    private IEnumerator PlayLine(int index)
    {
        if (dialogueText == null)
            yield break;

        if (dialogueLines == null ||
            index < 0 ||
            index >= dialogueLines.Length)
            yield break;

        if (textCanvasGroup != null)
            textCanvasGroup.alpha = 1f;

        string fullLine = dialogueLines[index];

        dialogueText.text = "";

        isTyping = true;

        // =====================================================
        // TYPEWRITER
        // =====================================================

        foreach (char c in fullLine)
        {
            dialogueText.text += c;

            yield return new WaitForSeconds(
                typingSpeed
            );
        }

        isTyping = false;

        // =====================================================
        // TUNGGU SETELAH TEKS SELESAI
        // =====================================================

        yield return new WaitForSeconds(
            holdDuration
        );

        // =====================================================
        // FADE OUT
        // =====================================================

        yield return StartCoroutine(
            FadeTextOut()
        );

        // =====================================================
        // AUTO ADVANCE
        // =====================================================

        if (autoAdvance)
        {
            AdvanceToNextLine();
        }
    }

    // =========================================================
    // FADE OUT
    // =========================================================

    private IEnumerator FadeTextOut()
    {
        if (textCanvasGroup == null)
            yield break;

        float startAlpha =
            textCanvasGroup.alpha;

        float time = 0f;

        while (time < fadeOutDuration)
        {
            time += Time.deltaTime;

            textCanvasGroup.alpha =
                Mathf.Lerp(
                    startAlpha,
                    0f,
                    time / fadeOutDuration
                );

            yield return null;
        }

        textCanvasGroup.alpha = 0f;
    }

    // =========================================================
    // NEXT / SKIP BUTTON
    // =========================================================

    public void OnNextPressed()
    {
        if (!IsShowing)
            return;

        // =====================================================
        // JIKA MASIH MENGETIK
        // LANGSUNG TAMPILKAN SELURUH KALIMAT
        // =====================================================

        if (isTyping)
        {
            if (lineRoutine != null)
            {
                StopCoroutine(lineRoutine);
            }

            dialogueText.text =
                dialogueLines[currentLine];

            isTyping = false;

            lineRoutine = StartCoroutine(
                FinishLineThenAdvance()
            );

            return;
        }

        // =====================================================
        // JIKA SUDAH SELESAI MENGETIK
        // =====================================================

        if (!autoAdvance)
        {
            if (lineRoutine != null)
            {
                StopCoroutine(lineRoutine);
            }

            lineRoutine = StartCoroutine(
                SkipHoldThenAdvance()
            );
        }
    }

    // =========================================================
    // SETELAH SKIP TYPEWRITER
    // =========================================================

    private IEnumerator FinishLineThenAdvance()
    {
        yield return new WaitForSeconds(
            holdDuration
        );

        yield return StartCoroutine(
            FadeTextOut()
        );

        AdvanceToNextLine();
    }

    // =========================================================
    // NEXT SAAT TEKS SUDAH SELESAI
    // =========================================================

    private IEnumerator SkipHoldThenAdvance()
    {
        yield return StartCoroutine(
            FadeTextOut()
        );

        AdvanceToNextLine();
    }

    // =========================================================
    // PINDAH KE BARIS BERIKUTNYA
    // =========================================================

    private void AdvanceToNextLine()
    {
        currentLine++;

        if (dialogueLines != null &&
            currentLine < dialogueLines.Length)
        {
            lineRoutine = StartCoroutine(
                PlayLine(currentLine)
            );
        }
        else
        {
            FinishDialogue();
        }
    }

    // =========================================================
    // SELESAI SEMUA DIALOG
    // =========================================================

    public void FinishDialogue()
    {
        if (!IsShowing)
            return;

        if (lineRoutine != null)
        {
            StopCoroutine(lineRoutine);
            lineRoutine = null;
        }

        isTyping = false;

        if (textCanvasGroup != null)
            textCanvasGroup.alpha = 1f;

        Hide();

        // Beritahu FarmingManager
        OnDialogueFinished?.Invoke();
    }
}
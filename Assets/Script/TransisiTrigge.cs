using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class EndingCutsceneTrigger : MonoBehaviour
{
    [Header("Referensi Player")]
    [Tooltip("Komponen LogInventory yang ada di GameObject player. Kalau kosong, akan dicari otomatis lewat tag 'Player'.")]
    public LogInventory playerLogInventory;

    [Tooltip("Komponen PlayerMovement pada Player.")]
    public PlayerMovement playerMovement;

    [Header("Dialogue")]
    [Tooltip("WaterDialogue yang digunakan untuk dialog opening dan ending.")]
    public WaterDialogue waterDialogue;

    [Header("Freeze Awal / Akhir")]
    [Tooltip("Waktu freeze saat scene baru dimulai sebelum dialog opening.")]
    public float openingFreezeDuration = 3f;

    [Tooltip("Waktu freeze setelah dialog ending selesai sebelum cutscene 2 tahun kemudian.")]
    public float endingFreezeDuration = 3f;

    [Header("UI Panel Hitam")]
    [Tooltip("Panel/Canvas full screen warna hitam. Pastikan tidak aktif di awal.")]
    public GameObject blackPanel;

    [Tooltip("Text di dalam panel hitam.")]
    public TMP_Text messageText;

    [Tooltip("CanvasGroup pada panel hitam untuk fade in.")]
    public CanvasGroup panelCanvasGroup;

    [Tooltip("Lama waktu fade in panel.")]
    public float fadeInDuration = 1f;

    [TextArea(2, 4)]
    public string message = "2 tahun kemudian...";

    [Tooltip("Berapa lama panel hitam tampil sebelum pindah scene.")]
    public float displayDuration = 3f;

    [Header("Sembunyikan HUD")]
    [Tooltip("HUD yang disembunyikan saat panel '2 tahun kemudian' muncul.")]
    public GameObject[] hudCanvasesToHide;

    [Header("Pindah Scene")]
    public string nextSceneName = "FarmGameplay";

    private bool hasTriggered = false;

    // Menentukan dialog mana yang sedang aktif
    private bool isOpeningDialogue = false;
    private bool isEndingDialogue = false;

    private void Awake()
    {
        if (blackPanel != null)
            blackPanel.SetActive(false);

        if (playerLogInventory == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

            if (playerObj != null)
            {
                playerLogInventory = playerObj.GetComponent<LogInventory>();

                if (playerMovement == null)
                    playerMovement = playerObj.GetComponent<PlayerMovement>();
            }
        }

        if (waterDialogue == null)
            waterDialogue = FindFirstObjectByType<WaterDialogue>();
    }

    private void OnEnable()
    {
        TreeTracker.OnAllTreesChopped += HandleAllTreesChopped;

        if (playerLogInventory != null)
            playerLogInventory.OnLogCountChanged += HandleLogCountChanged;

        if (waterDialogue != null)
            waterDialogue.OnDialogueFinished += HandleDialogueFinished;
    }

    private void OnDisable()
    {
        TreeTracker.OnAllTreesChopped -= HandleAllTreesChopped;

        if (playerLogInventory != null)
            playerLogInventory.OnLogCountChanged -= HandleLogCountChanged;

        if (waterDialogue != null)
            waterDialogue.OnDialogueFinished -= HandleDialogueFinished;
    }

    private void Start()
    {
        StartCoroutine(StartOpeningSequence());
    }

    // =========================================================
    // OPENING
    // =========================================================

    private IEnumerator StartOpeningSequence()
    {
        LockPlayer(true);

        Debug.Log(
            $"[EndingCutsceneTrigger] Freeze awal {openingFreezeDuration} detik."
        );

        yield return new WaitForSeconds(openingFreezeDuration);

        if (waterDialogue == null)
        {
            Debug.LogWarning(
                "[EndingCutsceneTrigger] WaterDialogue belum diassign. Opening dialogue dilewati."
            );

            LockPlayer(false);
            yield break;
        }

        isOpeningDialogue = true;
        isEndingDialogue = false;

        Debug.Log("[EndingCutsceneTrigger] Menampilkan dialog opening.");

        waterDialogue.ShowOpening();
    }

    // =========================================================
    // DIALOGUE SELESAI
    // =========================================================

    private void HandleDialogueFinished()
    {
        // Opening selesai
        if (isOpeningDialogue)
        {
            isOpeningDialogue = false;

            Debug.Log(
                "[EndingCutsceneTrigger] Dialog opening selesai. Player kembali bisa bergerak."
            );

            LockPlayer(false);
            return;
        }

        // Ending selesai
        if (isEndingDialogue)
        {
            isEndingDialogue = false;

            Debug.Log(
                "[EndingCutsceneTrigger] Dialog ending selesai. Memulai freeze akhir."
            );

            LockPlayer(true);

            StartCoroutine(StartEndingSequence());
        }
    }

    // =========================================================
    // CEK POHON + LOG
    // =========================================================

    private void HandleAllTreesChopped()
    {
        CheckCondition();
    }

    private void HandleLogCountChanged(int newCount)
    {
        CheckCondition();
    }

    private void CheckCondition()
{
    if (hasTriggered)
        return;

    if (playerLogInventory == null)
        return;

    bool allTreesChopped =
        TreeTracker.RemainingTrees <= 0 &&
        TreeTracker.TotalTrees > 0;

    bool logInventoryEmpty =
        playerLogInventory.CurrentLogs <= 0;

    bool noLogsLeftOnGround =
        LogPickupTracker.RemainingOnGround <= 0;

    Debug.Log(
        $"[ENDING CHECK] " +
        $"Trees: {TreeTracker.RemainingTrees}/{TreeTracker.TotalTrees} | " +
        $"Inventory Logs: {playerLogInventory.CurrentLogs} | " +
        $"Ground Logs: {LogPickupTracker.RemainingOnGround}"
    );

    if (allTreesChopped &&
        logInventoryEmpty &&
        noLogsLeftOnGround)
    {
        hasTriggered = true;
        StartEndingDialogue();
    }
}

    // =========================================================
    // ENDING DIALOGUE
    // =========================================================

    private void StartEndingDialogue()
    {
        LockPlayer(true);

        if (waterDialogue == null)
        {
            Debug.LogWarning(
                "[EndingCutsceneTrigger] WaterDialogue belum diassign. Langsung ke freeze akhir."
            );

            StartCoroutine(StartEndingSequence());
            return;
        }

        isEndingDialogue = true;
        isOpeningDialogue = false;

        Debug.Log(
            "[EndingCutsceneTrigger] Semua pekerjaan selesai. Menampilkan dialog ending."
        );

        waterDialogue.ShowEnding();
    }

    // =========================================================
    // FREEZE AKHIR → PANEL → SCENE
    // =========================================================

    private IEnumerator StartEndingSequence()
    {
        Debug.Log(
            $"[EndingCutsceneTrigger] Freeze akhir {endingFreezeDuration} detik."
        );

        yield return new WaitForSeconds(endingFreezeDuration);

        StartCoroutine(ShowTimeSkipThenChangeScene());
    }

    // =========================================================
    // PANEL "2 TAHUN KEMUDIAN"
    // =========================================================

    private IEnumerator ShowTimeSkipThenChangeScene()
    {
        Debug.Log(
            "[EndingCutsceneTrigger] Menampilkan panel '2 tahun kemudian'..."
        );

        foreach (GameObject hud in hudCanvasesToHide)
        {
            if (hud != null)
                hud.SetActive(false);
        }

        Time.timeScale = 0f;

        if (messageText != null)
            messageText.text = message;

        if (blackPanel != null)
            blackPanel.SetActive(true);

        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = 0f;

            float t = 0f;

            while (t < fadeInDuration)
            {
                t += Time.unscaledDeltaTime;

                panelCanvasGroup.alpha =
                    Mathf.Clamp01(t / fadeInDuration);

                yield return null;
            }

            panelCanvasGroup.alpha = 1f;
        }

        yield return new WaitForSecondsRealtime(displayDuration);

        Time.timeScale = 1f;

        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning(
                "[EndingCutsceneTrigger] Next Scene Name kosong."
            );

            LockPlayer(false);
            yield break;
        }

        Debug.Log(
            $"[EndingCutsceneTrigger] Pindah ke scene: {nextSceneName}"
        );

        if (SceneFader.Instance != null)
        {
            SceneFader.Instance.LoadScene(nextSceneName);
        }
        else
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }

    // =========================================================
    // PLAYER LOCK
    // =========================================================

    private void LockPlayer(bool locked)
    {
        if (playerMovement != null)
            playerMovement.SetMovementLocked(locked);
    }
}
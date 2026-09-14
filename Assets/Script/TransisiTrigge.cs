using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class EndingCutsceneTrigger : MonoBehaviour
{
    [Header("Referensi Player")]
    [Tooltip("Komponen LogInventory yang ada di GameObject player. Kalau kosong, akan dicari otomatis lewat tag 'Player'.")]
    public LogInventory playerLogInventory;

    [Header("UI Panel Hitam (sudah dibuat di scene ini)")]
    [Tooltip("Panel/Canvas full screen warna hitam yang SUDAH kamu buat. Pastikan tidak aktif di awal.")]
    public GameObject blackPanel;

    [Tooltip("Text di dalam panel hitam itu")]
    public TMP_Text messageText;

    [Tooltip("CanvasGroup pada panel hitam (WAJIB, untuk animasi fade in). Kalau kosong, panel langsung muncul instan tanpa fade.")]
    public CanvasGroup panelCanvasGroup;

    [Tooltip("Lama waktu fade in panel dari transparan ke terlihat penuh")]
    public float fadeInDuration = 1f;

    [TextArea(2, 4)]
    public string message = "2 tahun kemudian...";

    [Tooltip("Berapa lama panel hitam + text ini tampil PENUH sebelum pindah scene (tidak termasuk waktu fade in)")]
    public float displayDuration = 3f;

    [Header("Sembunyikan HUD & Freeze Game")]
    [Tooltip("Semua Canvas/GameObject HUD gameplay 1 yang perlu disembunyikan saat panel cutscene muncul (misal: UI Logs, UI Storage, dsb)")]
    public GameObject[] hudCanvasesToHide;

    [Header("Pindah ke Scene Gameplay 2 (Menanam)")]
    public string nextSceneName = "FarmGameplay";

    private bool hasTriggered = false;

    private void Awake()
    {
        if (blackPanel != null) blackPanel.SetActive(false);

        if (playerLogInventory == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerLogInventory = playerObj.GetComponent<LogInventory>();
            }
        }
    }

    private void OnEnable()
    {
        TreeTracker.OnAllTreesChopped += HandleAllTreesChopped;

        if (playerLogInventory != null)
        {
            playerLogInventory.OnLogCountChanged += HandleLogCountChanged;
        }
    }

    private void OnDisable()
    {
        TreeTracker.OnAllTreesChopped -= HandleAllTreesChopped;

        if (playerLogInventory != null)
        {
            playerLogInventory.OnLogCountChanged -= HandleLogCountChanged;
        }
    }

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
        if (hasTriggered) return;
        if (playerLogInventory == null) return;

        bool allTreesChopped = TreeTracker.RemainingTrees <= 0 && TreeTracker.TotalTrees > 0;
        bool logInventoryEmpty = playerLogInventory.CurrentLogs <= 0;
        bool noLogsLeftOnGround = LogPickupTracker.RemainingOnGround <= 0;

        if (allTreesChopped && logInventoryEmpty && noLogsLeftOnGround)
        {
            hasTriggered = true;
            StartCoroutine(ShowTimeSkipThenChangeScene());
        }
    }

    private IEnumerator ShowTimeSkipThenChangeScene()
    {
        Debug.Log("[EndingCutsceneTrigger] Semua pohon ditebang & log sudah disetor. Menampilkan panel '2 tahun kemudian'...");

        // --- Sembunyikan semua HUD gameplay 1 ---
        foreach (GameObject hud in hudCanvasesToHide)
        {
            if (hud != null) hud.SetActive(false);
        }

        // --- Freeze game (player, pohon, dsb berhenti total) ---
        Time.timeScale = 0f;

        if (messageText != null) messageText.text = message;
        if (blackPanel != null) blackPanel.SetActive(true);


        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = 0f;
            float t = 0f;
            while (t < fadeInDuration)
            {
                t += Time.unscaledDeltaTime;
                panelCanvasGroup.alpha = Mathf.Clamp01(t / fadeInDuration);
                yield return null;
            }
            panelCanvasGroup.alpha = 1f;
        }

       
        yield return new WaitForSecondsRealtime(displayDuration);

       
        Time.timeScale = 1f;

        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning("[EndingCutsceneTrigger] Next Scene Name kosong, tidak jadi pindah scene.");
            yield break;
        }

        Debug.Log($"[EndingCutsceneTrigger] Pindah ke scene gameplay 2: {nextSceneName}");

    
        if (SceneFader.Instance != null)
        {
            SceneFader.Instance.LoadScene(nextSceneName);
        }
        else
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
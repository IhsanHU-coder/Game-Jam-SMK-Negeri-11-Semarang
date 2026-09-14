using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class LogStorage : MonoBehaviour
{
    [Header("Storage")]
    [SerializeField] private int storedLogs = 0;

    [Header("Interaction")]
    [SerializeField] private float interactionRange = 2f;

    [Header("Info")]
    [SerializeField] private GameObject infoLogging;

    [Header("Storage UI")]
    [SerializeField] private GameObject storageLogsUI;
    [SerializeField] private TMP_Text logsText;

    [Header("Player")]
    [SerializeField] private Transform player;

    [Header("Progress Slider")]
    [SerializeField] private Slider progressSlider;

    private LogInventory inventory;

    private bool isStoring = false;

    private float storeTimer = 0f;
    private float storeDuration = 0f;

    private void Awake()
    {
        // Hold E awalnya mati
        if (infoLogging != null)
        {
            infoLogging.SetActive(false);
        }

        // UI jumlah storage selalu menyala
        if (storageLogsUI != null)
        {
            storageLogsUI.SetActive(true);
        }

        // Slider awalnya mati
        if (progressSlider != null)
        {
            progressSlider.gameObject.SetActive(false);

            progressSlider.minValue = 0f;
            progressSlider.maxValue = 1f;
            progressSlider.value = 0f;
        }
    }

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            if (player == null)
            {
                player = playerObject.transform;
            }

            inventory = playerObject.GetComponent<LogInventory>();
        }

        UpdateStorageText();
    }

    private void Update()
    {
        if (player == null || inventory == null)
            return;

        CheckPlayerDistance();
        HandleStoreInput();
    }

    private void CheckPlayerDistance()
    {
        float distance = Vector2.Distance(
            transform.position,
            player.position
        );

        bool inRange = distance <= interactionRange;

        // Player keluar ketika sedang menyimpan
        if (!inRange && isStoring)
        {
            CancelStoring();
        }

        // Kalau sedang menyimpan,
        // jangan ubah UI di sini
        if (isStoring)
            return;

        // Player dekat
        if (inRange)
        {
            if (infoLogging != null)
            {
                infoLogging.SetActive(true);
            }
        }
        // Player jauh
        else
        {
            if (infoLogging != null)
            {
                infoLogging.SetActive(false);
            }
        }
    }

    private void HandleStoreInput()
    {
        if (Keyboard.current == null)
            return;

        float distance = Vector2.Distance(
            transform.position,
            player.position
        );

        // Kalau keluar range
        if (distance > interactionRange)
        {
            if (isStoring)
            {
                CancelStoring();
            }

            return;
        }

        // Tekan E untuk mulai
        if (!isStoring &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            StartStoring();
        }

        if (!isStoring)
            return;

        // E masih ditahan
        if (Keyboard.current.eKey.isPressed)
        {
            storeTimer += Time.deltaTime;

            UpdateProgress();

            if (storeTimer >= storeDuration)
            {
                CompleteStoring();
            }
        }
        else
        {
            // E dilepas sebelum selesai
            CancelStoring();
        }
    }

    private void StartStoring()
    {
        if (inventory.CurrentLogs <= 0)
        {
            Debug.Log("Player tidak membawa logs.");
            return;
        }

        // Contoh:
        // 1 log = 1 detik
        // 3 log = 3 detik
        // 5 log = 5 detik
        storeDuration = inventory.CurrentLogs;

        storeTimer = 0f;
        isStoring = true;

        // Hold E dimatikan
        if (infoLogging != null)
        {
            infoLogging.SetActive(false);
        }

        // Slider dinyalakan
        if (progressSlider != null)
        {
            progressSlider.gameObject.SetActive(true);

            progressSlider.minValue = 0f;
            progressSlider.maxValue = 1f;
            progressSlider.value = 0f;
        }
    }

    private void UpdateProgress()
    {
        if (progressSlider == null)
            return;

        float progress =
            storeTimer / storeDuration;

        progressSlider.value =
            Mathf.Clamp01(progress);
    }

    private void CompleteStoring()
    {
        isStoring = false;

        int logsToStore =
            inventory.RemoveAllLogs();

        storedLogs += logsToStore;

        storeTimer = 0f;
        storeDuration = 0f;

        // Update angka storage
        UpdateStorageText();

        // Slider mati
        if (progressSlider != null)
        {
            progressSlider.value = 0f;
            progressSlider.gameObject.SetActive(false);
        }

        // Hold E muncul lagi
        if (infoLogging != null)
        {
            infoLogging.SetActive(true);
        }

        Debug.Log(
            "Berhasil menyimpan " +
            logsToStore +
            " logs."
        );
    }

    private void CancelStoring()
    {
        isStoring = false;

        storeTimer = 0f;
        storeDuration = 0f;

        // Slider mati
        if (progressSlider != null)
        {
            progressSlider.value = 0f;
            progressSlider.gameObject.SetActive(false);
        }

        // Kalau masih dekat, Hold E muncul lagi
        if (player != null)
        {
            float distance = Vector2.Distance(
                transform.position,
                player.position
            );

            if (distance <= interactionRange)
            {
                if (infoLogging != null)
                {
                    infoLogging.SetActive(true);
                }
            }
        }

        Debug.Log("Penyimpanan dibatalkan.");
    }

    private void UpdateStorageText()
    {
        if (logsText != null)
        {
            logsText.text = storedLogs.ToString();
        }
    }

    public int GetStoredLogs()
    {
        return storedLogs;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(
            transform.position,
            interactionRange
        );
    }
}
using UnityEngine;
using UnityEngine.InputSystem;

public class CraftingStation : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private float interactionRange = 2f;

    [Header("UI")]
    [SerializeField] private GameObject infoCrafting;
    [SerializeField] private GameObject craftingPanel;

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private LogStorage logStorage;

    [Header("Audio")]
    [SerializeField] private string openSoundId = "CraftingOpen";
    [SerializeField] private string closeSoundId = "CraftingClose";
    [SerializeField] private string upgradeSuccessSoundId = "UpgradeSuccess";
    [SerializeField] private string upgradeFailSoundId = "UpgradeFail";

    private PlayerStats playerStats;
    private bool isPlayerInRange = false;
    private bool isCraftingOpen = false;

    private void Awake()
    {
        if (infoCrafting != null) infoCrafting.SetActive(false);
        if (craftingPanel != null) craftingPanel.SetActive(false);
    }

    private void Start()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
                player = playerObject.transform;
        }

        if (player != null)
        {
            playerStats = player.GetComponent<PlayerStats>();
        }

        if (logStorage == null)
        {
            logStorage = FindObjectOfType<LogStorage>();
        }
    }

    private void Update()
    {
        if (player == null) return;

        CheckPlayerDistance();
        HandleInput();
    }

    private void CheckPlayerDistance()
    {
        float distance = Vector2.Distance(transform.position, player.position);
        isPlayerInRange = distance <= interactionRange;

        if (!isPlayerInRange && isCraftingOpen)
        {
            CloseCrafting();
        }

        if (isCraftingOpen)
        {
            if (infoCrafting != null) infoCrafting.SetActive(false);
            return;
        }

        if (infoCrafting != null) infoCrafting.SetActive(isPlayerInRange);
    }

    private void HandleInput()
    {
        if (Keyboard.current == null || !isPlayerInRange) return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (isCraftingOpen) CloseCrafting();
            else OpenCrafting();
        }
    }

    private void OpenCrafting()
    {
        isCraftingOpen = true;
        if (craftingPanel != null) craftingPanel.SetActive(true);
        if (infoCrafting != null) infoCrafting.SetActive(false);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(openSoundId);
        }
    }

    public void CloseCrafting()
    {
        isCraftingOpen = false;
        if (craftingPanel != null) craftingPanel.SetActive(false);
        if (infoCrafting != null) infoCrafting.SetActive(isPlayerInRange);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(closeSoundId);
        }
    }

    // ==========================================
    // FUNGSI PEMBELIAN UPGRADE MULTI-PARAMETER
    // ==========================================
    // Ubah dari 'public void' menjadi 'public bool'
    public bool BuyDamageUpgrade(int cost, int newMinDamage, int newMaxDamage)
    {
        if (logStorage == null) return false;

        // Cek apakah kayu/log mencukupi
        if (logStorage.TryConsumeLogs(cost))
        {
            if (playerStats != null)
            {
                playerStats.SetDamage(newMinDamage, newMaxDamage);
            }
            Debug.Log($"Upgrade Berhasil! Min Damage: {newMinDamage}, Max Damage: {newMaxDamage}");

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(upgradeSuccessSoundId);
            }

            return true; // Pembelian BERHASIL
        }

        Debug.Log("Upgrade Gagal: Log tidak cukup!");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(upgradeFailSoundId);
        }

        return false; // Pembelian GAGAL
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
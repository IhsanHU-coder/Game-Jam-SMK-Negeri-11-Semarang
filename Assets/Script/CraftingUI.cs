using UnityEngine;
using UnityEngine.UI;

public class CraftingUI : MonoBehaviour
{
    [System.Serializable]
    public struct CraftingButton
    {
        public Button button;      // Tombol UI
        public int logCost;        // Biaya Log khusus tombol ini
    }

    [Header("References")]
    [SerializeField] private LogStorage logStorage;
    [SerializeField] private CraftingButton[] craftingButtons;

    private void Start()
    {
        if (logStorage == null)
        {
            logStorage = FindObjectOfType<LogStorage>();
        }
    }

    private void Update()
    {
        UpdateButtonsInteractable();
    }

    private void UpdateButtonsInteractable()
    {
        if (logStorage == null)
            return;

        int currentLogs = logStorage.GetStoredLogs();

        // Cek setiap tombol satu per satu
        foreach (var item in craftingButtons)
        {
            // Pastikan GameObject tombol tidak null dan masih aktif (belum dibeli)
            if (item.button != null && item.button.gameObject.activeSelf)
            {
                // Tombol BISA diinteraksi HANYA JIKA log saat ini >= biaya log tombol tersebut
                item.button.interactable = (currentLogs >= item.logCost);
            }
        }
    }
}
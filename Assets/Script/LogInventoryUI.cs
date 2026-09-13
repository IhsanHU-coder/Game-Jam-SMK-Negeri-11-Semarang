using TMPro;
using UnityEngine;

public class LogInventoryUI : MonoBehaviour
{
    [SerializeField] private LogInventory inventory;
    [SerializeField] private TMP_Text logsText;

    private void Update()
    {
        if (inventory == null)
            return;

        logsText.text = "Logs " +
                        inventory.CurrentLogs +
                        "/" +
                        inventory.MaxLogs;
    }
}
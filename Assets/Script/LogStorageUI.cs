using TMPro;
using UnityEngine;

public class LogStorageUI : MonoBehaviour
{
    [SerializeField] private LogStorage storage;
    [SerializeField] private TMP_Text logsText;

    private void Update()
    {
        if (storage == null || logsText == null)
            return;

        logsText.text = "Logs: " + storage.GetStoredLogs();
    }
}
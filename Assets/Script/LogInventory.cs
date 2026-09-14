using System;
using UnityEngine;

public class LogInventory : MonoBehaviour
{
    [Header("Inventory")]
    [SerializeField] private int maxLogs = 5;

    private int currentLogs = 0;

    public int CurrentLogs => currentLogs;
    public int MaxLogs => maxLogs;

    /// <summary>
    /// Dipanggil setiap kali currentLogs berubah (nambah atau di-reset ke 0).
    /// Parameter int = jumlah log terbaru.
    /// </summary>
    public event Action<int> OnLogCountChanged;

    public bool IsFull()
    {
        return currentLogs >= maxLogs;
    }

    public bool AddLog()
    {
        if (IsFull())
        {
            Debug.Log("Inventory log penuh!");
            return false;
        }

        currentLogs++;

        Debug.Log("Player Logs: " + currentLogs + "/" + maxLogs);

        // === TAMBAHAN ===
        OnLogCountChanged?.Invoke(currentLogs);
        // === akhir tambahan ===

        return true;
    }

    public int RemoveAllLogs()
    {
        int logsToStore = currentLogs;

        currentLogs = 0;

        Debug.Log("Player menyimpan " + logsToStore + " logs.");

        // === TAMBAHAN ===
        OnLogCountChanged?.Invoke(currentLogs);
        // === akhir tambahan ===

        return logsToStore;
    }
}
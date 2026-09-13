using UnityEngine;

public class LogInventory : MonoBehaviour
{
    [Header("Inventory")]
    [SerializeField] private int maxLogs = 5;

    private int currentLogs = 0;

    public int CurrentLogs => currentLogs;
    public int MaxLogs => maxLogs;

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

        Debug.Log("Logs: " + currentLogs + "/" + maxLogs);

        return true;
    }

    public void RemoveAllLogs()
    {
        currentLogs = 0;

        Debug.Log("Semua log sudah ditaruh. Logs: 0/" + maxLogs);
    }
}
using System;
using UnityEngine;

public static class TreeTracker
{
    public static int TotalTrees { get; private set; }
    public static int RemainingTrees { get; private set; }

    //
    public static event Action OnAllTreesChopped;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnGameStart()
    {
        ResetTracker();
    }

    /// <summary>Dipanggil oleh Tree.cs saat pohon itu aktif/muncul di scene.</summary>
    public static void RegisterTree()
    {
        TotalTrees++;
        RemainingTrees++;
    }

    /// <summary>Dipanggil oleh Tree.cs saat pohon itu selesai ditebang.</summary>
    public static void ReportTreeChopped()
    {
        if (RemainingTrees > 0) RemainingTrees--;

        if (RemainingTrees <= 0 && TotalTrees > 0)
        {
            OnAllTreesChopped?.Invoke();
        }
    }

  
    public static void ResetTracker()
    {
        TotalTrees = 0;
        RemainingTrees = 0;
    }
}
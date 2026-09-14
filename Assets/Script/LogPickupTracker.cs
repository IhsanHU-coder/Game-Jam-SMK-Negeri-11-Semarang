using System;
using UnityEngine;

public static class LogPickupTracker
{
    public static int TotalSpawned { get; private set; }
    public static int RemainingOnGround { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnGameStart()
    {
        ResetTracker();
    }

    // Dipanggil saat log muncul di tanah
    public static void RegisterSpawnedLog()
    {
        TotalSpawned++;
        RemainingOnGround++;

        Debug.Log(
            $"[LogPickupTracker] Log muncul. " +
            $"Total: {TotalSpawned}, " +
            $"Sisa di tanah: {RemainingOnGround}"
        );
    }

    // Dipanggil saat log diambil player
    public static void ReportPickedUp()
    {
        if (RemainingOnGround > 0)
            RemainingOnGround--;

        Debug.Log(
            $"[LogPickupTracker] Log diambil. " +
            $"Sisa di tanah: {RemainingOnGround}"
        );
    }

    public static void ResetTracker()
    {
        TotalSpawned = 0;
        RemainingOnGround = 0;
    }
}
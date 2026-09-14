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

    public static void RegisterPickup()
    {
        TotalSpawned++;
        RemainingOnGround++;
    }

    public static void ReportPickedUp()
    {
        if (RemainingOnGround > 0) RemainingOnGround--;
    }

    public static void ResetTracker()
    {
        TotalSpawned = 0;
        RemainingOnGround = 0;
    }
}
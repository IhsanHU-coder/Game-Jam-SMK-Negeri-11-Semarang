using System;
using UnityEngine;

public static class SeedInventory
{
    private static int seedCount = 0;

 
    public static int MaxSeedCount = 5;

    public static int SeedCount
    {
        get => seedCount;
        set
        {
            int clamped = Mathf.Clamp(value, 0, MaxSeedCount);
            if (clamped == seedCount) return; // tidak ada perubahan, tidak perlu invoke event

            seedCount = clamped;
            OnSeedCountChanged?.Invoke(seedCount);
        }
    }


    public static event Action<int> OnSeedCountChanged;

    
    public static bool IsFull => seedCount >= MaxSeedCount;
}
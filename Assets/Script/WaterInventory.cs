
using System;
using UnityEngine;

public static class WaterInventory
{
    public static int MaxWater = 10;

    private static int waterCount = 0;

    public static int WaterCount
    {
        get => waterCount;

        set
        {
            int clamped = Mathf.Clamp(value, 0, MaxWater);

            if (clamped == waterCount)
                return;

            waterCount = clamped;
            OnWaterCountChanged?.Invoke(waterCount);
        }
    }

    public static event Action<int> OnWaterCountChanged;

    public static bool IsEmpty => waterCount <= 0;
    public static bool IsFull => waterCount >= MaxWater;

    public static void ResetWater()
    {
        waterCount = 0;
        OnWaterCountChanged?.Invoke(waterCount);
    }

    public static void FillWater()
    {
        WaterCount = MaxWater;
    }
}


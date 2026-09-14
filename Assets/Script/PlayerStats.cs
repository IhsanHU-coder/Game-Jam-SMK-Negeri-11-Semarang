using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Current Damage Stats")]
    public int minDamage = 1;
    public int maxDamage = 3;

    // Fungsi untuk memperbarui damage saat upgrade dibeli
    public void SetDamage(int newMin, int newMax)
    {
        minDamage = newMin;
        maxDamage = newMax;
        Debug.Log($"Damage player meningkat! Sekarang: {minDamage} - {maxDamage}");
    }
}
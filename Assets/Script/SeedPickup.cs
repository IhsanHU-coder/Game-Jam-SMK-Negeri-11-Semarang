using UnityEngine;

public class SeedPickup : MonoBehaviour
{
    [Tooltip("Jumlah biji yang didapat setiap kali kotak ini diklik")]
    public int seedAmountPerClick = 1;

    [Tooltip("Jika true, kotak ini akan hilang/habis walaupun sumber biji ini biasanya berupa tumpukan tak terbatas. Biarkan false untuk kotak biji yang bisa diambil berkali-kali.")]
    public bool destroyAfterPickup = false;

    public void Collect()
    {
        if (SeedInventory.IsFull)
        {
            Debug.Log($"[SeedPickup] Biji sudah penuh! Maksimal {SeedInventory.MaxSeedCount} biji.");
            return; // tidak menambah apapun jika sudah penuh
        }

        SeedInventory.SeedCount += seedAmountPerClick; // otomatis ke-clamp ke MaxSeedCount di SeedInventory
        Debug.Log($"[SeedPickup] Mengambil biji. Total sekarang: {SeedInventory.SeedCount}/{SeedInventory.MaxSeedCount}");

        if (destroyAfterPickup)
        {
            Destroy(gameObject);
        }
    }
}
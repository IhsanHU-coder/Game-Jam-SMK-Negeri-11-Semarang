using UnityEngine;
using TMPro;

public class SeedUI : MonoBehaviour
{
    [Tooltip("Komponen TextMeshPro yang menampilkan jumlah biji")]
    public TMP_Text seedText;

    [Tooltip("Format teks yang ditampilkan. {0} = jumlah biji sekarang, {1} = jumlah maksimal")]
    public string format = "Biji: {0} / {1}";

    private void OnEnable()
    {
        SeedInventory.OnSeedCountChanged += UpdateSeedText;
        UpdateSeedText(SeedInventory.SeedCount); // langsung tampilkan nilai awal saat UI aktif
    }

    private void OnDisable()
    {
        SeedInventory.OnSeedCountChanged -= UpdateSeedText;
    }

    private void UpdateSeedText(int count)
    {
        if (seedText == null) return;
        seedText.text = string.Format(format, count, SeedInventory.MaxSeedCount);
    }
}
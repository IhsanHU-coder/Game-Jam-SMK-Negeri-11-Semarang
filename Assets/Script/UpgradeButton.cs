using UnityEngine;
using UnityEngine.UI;

public class UpgradeButton : MonoBehaviour
{
    [Header("Upgrade Settings")]
    public int logCost = 10;
    public int newMinDamage = 6;
    public int newMaxDamage = 10;

    [Header("References")]
    [SerializeField] private CraftingStation craftingStation;

    public GameObject upgradeButton;

    private void Start()
    {
        // Jika upgradeButton belum diisi di Inspector, gunakan GameObject ini sendiri
        if (upgradeButton == null)
            upgradeButton = gameObject;

        upgradeButton.SetActive(true);

        if (craftingStation == null)
            craftingStation = FindObjectOfType<CraftingStation>();

        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(OnClickUpgrade);
        }
    }

    private void OnClickUpgrade()
    {
        if (craftingStation != null)
        {
            // Cek apakah transaksi di CraftingStation berhasil
            bool isSuccess = craftingStation.BuyDamageUpgrade(logCost, newMinDamage, newMaxDamage);

            // Hanya set False JIKA pembelian berhasil
            if (isSuccess)
            {
                upgradeButton.SetActive(false);
            }
        }
    }
}
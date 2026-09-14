using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(InteractZone))]
public class SeedPickup : MonoBehaviour
{
    [Tooltip("Jumlah biji yang didapat setiap kali kotak ini diambil")]
    public int seedAmountPerClick = 1;

    [Tooltip("Jika true, kotak ini akan hilang/habis walaupun sumber biji ini biasanya berupa tumpukan tak terbatas. Biarkan false untuk kotak biji yang bisa diambil berkali-kali.")]
    public bool destroyAfterPickup = false;

    [Header("Pickup Icon")]
    [Tooltip("Icon yang muncul saat player berada dekat Seed Box.")]
    public GameObject pickupIcon;

    [Header("Audio")]
    [SerializeField] private string collectSoundId = "SeedCollect";
    [SerializeField] private string collectFailSoundId = "SeedCollectFail";

    private InteractZone interactZone;

    // Dikontrol oleh FarmingManager
    private bool interactionEnabled = true;

    private void Awake()
    {
        interactZone = GetComponent<InteractZone>();

        // Icon disembunyikan saat awal
        HidePickupIcon();
    }

    private void Update()
    {
        // Jika Seed Box sedang dikunci oleh FarmingManager
        if (!interactionEnabled)
        {
            HidePickupIcon();
            return;
        }

        // Tekan E saat player berada di dalam InteractZone kotak biji ini
        if (interactZone != null && interactZone.IsPlayerInRange)
        {
            // Tampilkan icon pickup
            ShowPickupIcon();

            if (Keyboard.current != null &&
                Keyboard.current.eKey.wasPressedThisFrame)
            {
                Collect();
            }
        }
        else
        {
            // Player keluar dari area
            HidePickupIcon();
        }
    }

    /// <summary>
    /// Bisa juga dipanggil manual dari script lain kalau perlu.
    /// </summary>
    public void Collect()
    {
        if (!interactionEnabled)
            return;

        if (SeedInventory.IsFull)
        {
            Debug.Log($"[SeedPickup] Biji sudah penuh! Maksimal {SeedInventory.MaxSeedCount} biji.");

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(collectFailSoundId);
            }

            return; // tidak menambah apapun jika sudah penuh
        }

        SeedInventory.SeedCount += seedAmountPerClick; // otomatis ke-clamp ke MaxSeedCount di SeedInventory

        Debug.Log($"[SeedPickup] Mengambil biji. Total sekarang: {SeedInventory.SeedCount}/{SeedInventory.MaxSeedCount}");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(collectSoundId);
        }

        if (destroyAfterPickup)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Dipanggil FarmingManager untuk mengaktifkan / menonaktifkan interaksi Seed Box.
    /// </summary>
    public void SetInteractionEnabled(bool enabled)
    {
        interactionEnabled = enabled;

        if (!enabled)
        {
            HidePickupIcon();
        }
    }

    private void ShowPickupIcon()
    {
        if (pickupIcon != null)
        {
            pickupIcon.SetActive(true);
        }
    }

    private void HidePickupIcon()
    {
        if (pickupIcon != null)
        {
            pickupIcon.SetActive(false);
        }
    }
}
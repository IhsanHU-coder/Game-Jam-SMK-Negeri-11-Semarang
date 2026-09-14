using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(InteractZone))]
public class WellRefill : MonoBehaviour
{
    [Header("Refill")]
    [SerializeField] private float refillDuration = 2f;

    [Header("Pickup Icon")]
    [SerializeField] private GameObject pickupIcon;

    [Header("Slider Refill")]
    [SerializeField] private GameObject refillSliderPrefab;
    [SerializeField] private float sliderYOffset = 1f;

    [Header("Player Movement")]
    [SerializeField] private PlayerMovement playerMovement;

    private InteractZone interactZone;

    private GameObject refillSliderInstance;
    private Slider refillSlider;

    private bool isRefilling = false;
    private float refillTime = 0f;

    // Dikontrol oleh FarmingManager
    private bool enabledByPhase = true;

    private void Awake()
    {
        interactZone = GetComponent<InteractZone>();

        // Icon tidak langsung muncul
        if (pickupIcon != null)
            pickupIcon.SetActive(false);

        // Slider tidak langsung muncul
        if (refillSliderPrefab != null)
        {
            refillSliderInstance = Instantiate(
                refillSliderPrefab,
                transform
            );

            refillSlider = refillSliderInstance
                .GetComponentInChildren<Slider>(true);

            refillSliderInstance.SetActive(false);
        }
    }

    private void Update()
    {
        // =====================================================
        // INTERACTION DIKUNCI OLEH FARMING MANAGER
        // =====================================================
        if (!enabledByPhase)
        {
            CancelRefill();
            HidePickupIcon();
            return;
        }

        if (interactZone == null)
            return;

        // Kalau player keluar dari area
        if (!interactZone.IsPlayerInRange)
        {
            CancelRefill();
            HidePickupIcon();
            return;
        }

        // Kalau air sudah penuh
        if (WaterInventory.IsFull)
        {
            CancelRefill();
            HidePickupIcon();
            return;
        }

        // Player berada di dekat sumur
        ShowPickupIcon();

        if (Keyboard.current == null)
            return;

        // =====================================================
        // BELUM REFILL → TEKAN E
        // =====================================================
        if (!isRefilling)
        {
            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                StartRefill();
            }

            return;
        }

        // =====================================================
        // SEDANG REFILL → HARUS TAHAN E
        // =====================================================
        if (!Keyboard.current.eKey.isPressed)
        {
            CancelRefill();
            return;
        }

        refillTime += Time.deltaTime;

        UpdateRefillSlider();

        if (refillTime >= refillDuration)
        {
            CompleteRefill();
        }
    }

    // =========================================================
    // START REFILL
    // =========================================================
    private void StartRefill()
    {
        if (!enabledByPhase)
            return;

        if (WaterInventory.IsFull)
            return;

        isRefilling = true;
        refillTime = 0f;

        // KUNCI GERAK PLAYER
        if (playerMovement != null)
            playerMovement.SetMovementLocked(true);

        HidePickupIcon();

        ShowRefillSlider();
        UpdateRefillSlider();
    }

    // =========================================================
    // CANCEL REFILL
    // =========================================================
    private void CancelRefill()
    {
        if (!isRefilling)
            return;

        isRefilling = false;

        // Reset dari awal
        refillTime = 0f;

        HideRefillSlider();

        // BUKA GERAK PLAYER LAGI
        if (playerMovement != null)
            playerMovement.SetMovementLocked(false);
    }

    // =========================================================
    // COMPLETE REFILL
    // =========================================================
    private void CompleteRefill()
    {
        isRefilling = false;
        refillTime = 0f;

        // Isi air langsung sampai MAX
        WaterInventory.FillWater();

        HideRefillSlider();

        // BUKA GERAK PLAYER LAGI
        if (playerMovement != null)
            playerMovement.SetMovementLocked(false);
    }

    // =========================================================
    // PICKUP ICON
    // =========================================================
    private void ShowPickupIcon()
    {
        if (pickupIcon == null)
            return;

        // Jangan tampilkan icon ketika sedang refill
        if (isRefilling)
            return;

        pickupIcon.SetActive(true);
    }

    private void HidePickupIcon()
    {
        if (pickupIcon != null)
            pickupIcon.SetActive(false);
    }

    // =========================================================
    // SLIDER
    // =========================================================
    private void ShowRefillSlider()
    {
        if (refillSliderInstance == null)
            return;

        refillSliderInstance.transform.localPosition =
            new Vector3(
                0f,
                sliderYOffset,
                0f
            );

        refillSliderInstance.SetActive(true);

        if (refillSlider != null)
        {
            refillSlider.minValue = 0f;
            refillSlider.maxValue = 1f;
            refillSlider.value = 0f;
        }
    }

    private void HideRefillSlider()
    {
        if (refillSliderInstance != null)
            refillSliderInstance.SetActive(false);
    }

    private void UpdateRefillSlider()
    {
        if (refillSlider == null)
            return;

        if (refillDuration <= 0f)
        {
            refillSlider.value = 1f;
            return;
        }

        refillSlider.value =
            Mathf.Clamp01(
                refillTime / refillDuration
            );
    }

    // =========================================================
    // ENABLE / DISABLE INTERACTION
    // =========================================================
    public void SetInteractionEnabled(bool enabled)
    {
        enabledByPhase = enabled;

        if (!enabled)
        {
            CancelRefill();
            HidePickupIcon();
        }
    }
}
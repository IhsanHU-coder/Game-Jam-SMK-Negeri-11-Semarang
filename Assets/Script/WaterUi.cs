
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WaterUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider waterSlider;

    [Tooltip("Opsional. Menampilkan Air: 7 / 10")]
    [SerializeField] private TMP_Text waterText;

    [SerializeField] private string waterTextFormat = "Air: {0} / {1}";

    private void OnEnable()
    {
        WaterInventory.OnWaterCountChanged += UpdateWaterUI;
        UpdateWaterUI(WaterInventory.WaterCount);
    }

    private void OnDisable()
    {
        WaterInventory.OnWaterCountChanged -= UpdateWaterUI;
    }

    private void UpdateWaterUI(int amount)
    {
        if (waterSlider != null)
        {
            waterSlider.minValue = 0;
            waterSlider.maxValue = WaterInventory.MaxWater;
            waterSlider.value = amount;
        }

        if (waterText != null)
        {
            waterText.text = string.Format(
                waterTextFormat,
                amount,
                WaterInventory.MaxWater
            );
        }
    }
}


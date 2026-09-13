using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Pasang script ini di tiap Slider (Main Vol, SFX, Music).
/// TIDAK menyimpan referensi AudioMixer sendiri — semua diteruskan lewat
/// SettingsApplier.Instance (singleton persisten), supaya seluruh game
/// selalu pakai satu Mixer reference yang sama persis, di scene manapun.
/// </summary>
[RequireComponent(typeof(Slider))]
public class VolumeSliderControl : MonoBehaviour
{
    [Tooltip("Nama Exposed Parameter di Audio Mixer, harus PERSIS sama (contoh: MainVolume, SFXVolume, MusicVolume)")]
    [SerializeField] private string exposedParameter = "MainVolume";

    [Header("UI")]
    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text valueText;

    [Header("Range Slider")]
    [Tooltip("Nilai maksimum slider di Inspector (sesuai screenshot: 0-10)")]
    [SerializeField] private float maxSliderValue = 10f;

    private void Awake()
    {
        if (slider == null)
            slider = GetComponent<Slider>();
    }

    private void Start()
    {
        if (SettingsApplier.Instance == null)
        {
            Debug.LogError("[VolumeSliderControl] SettingsApplier.Instance belum ada! " +
                            "Pastikan scene Bootstrap dengan SettingsApplier dijalankan lebih dulu.");
            return;
        }

        float saved = SettingsApplier.Instance.GetSavedSliderValue(exposedParameter);

        slider.minValue = 0f;
        slider.maxValue = maxSliderValue;
        slider.value = saved;

        UpdateText(saved);

        slider.onValueChanged.AddListener(OnSliderChanged);
    }

    private void OnDestroy()
    {
        slider.onValueChanged.RemoveListener(OnSliderChanged);
    }

    private void OnSliderChanged(float value)
    {
        SettingsApplier.Instance.SetVolume(exposedParameter, value);
        UpdateText(value);
    }

    private void UpdateText(float value)
    {
        if (valueText != null)
            valueText.text = Mathf.RoundToInt(value).ToString();
    }
}
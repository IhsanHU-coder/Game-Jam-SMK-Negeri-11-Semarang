using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Pasang script ini di tiap Slider (Main Vol, SFX, Music).
/// Mengubah nilai slider (0-10) menjadi dB untuk Exposed Parameter di Audio Mixer,
/// menampilkan angka akurat di TMP_Text, dan menyimpan pilihan user ke PlayerPrefs.
/// </summary>
[RequireComponent(typeof(Slider))]
public class VolumeSliderControl : MonoBehaviour
{
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Tooltip("Nama Exposed Parameter di Audio Mixer, harus PERSIS sama (contoh: MainVolume, SFXVolume, MusicVolume)")]
    [SerializeField] private string exposedParameter = "MainVolume";

    [Header("UI")]
    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text valueText;

    [Header("Range Slider")]
    [Tooltip("Nilai maksimum slider di Inspector (sesuai screenshot: 0-10)")]
    [SerializeField] private float maxSliderValue = 10f;
    [SerializeField] private float defaultValue = 10f;

    private const string PrefKeyPrefix = "VOL_";

    private void Awake()
    {
        if (slider == null)
            slider = GetComponent<Slider>();
    }

    private void Start()
    {
        // Ambil nilai tersimpan (kalau belum pernah diset, pakai default)
        float saved = PlayerPrefs.GetFloat(PrefKeyPrefix + exposedParameter, defaultValue);

        slider.minValue = 0f;
        slider.maxValue = maxSliderValue;
        slider.value = saved;

        ApplyVolume(saved);
        UpdateText(saved);

        slider.onValueChanged.AddListener(OnSliderChanged);
    }

    private void OnDestroy()
    {
        slider.onValueChanged.RemoveListener(OnSliderChanged);
    }

    private void OnSliderChanged(float value)
    {
        ApplyVolume(value);
        UpdateText(value);
        PlayerPrefs.SetFloat(PrefKeyPrefix + exposedParameter, value);
    }

    private void ApplyVolume(float sliderValue)
    {
        // Slider 0-10 dinormalisasi ke 0-1, lalu dikonversi ke dB (log scale)
        float normalized = Mathf.Clamp(sliderValue / maxSliderValue, 0.0001f, 1f);
        float dB = Mathf.Log10(normalized) * 20f;
        audioMixer.SetFloat(exposedParameter, dB);
    }

    private void UpdateText(float value)
    {
        if (valueText != null)
            valueText.text = Mathf.RoundToInt(value).ToString();
    }
}
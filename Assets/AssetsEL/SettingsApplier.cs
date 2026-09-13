using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Bootstrap: taruh di GameObject pada scene paling awal (misal scene "Bootstrap"/"Init"),
/// atau di object yang DontDestroyOnLoad sebelum masuk ke scene utama.
/// Tugasnya: baca setting volume yang tersimpan di PlayerPrefs lalu apply ke Audio Mixer
/// SEBELUM slider di menu pause sempat dibuka, supaya volume game langsung sesuai
/// preferensi terakhir user sejak awal.
/// </summary>
public class SettingsApplier : MonoBehaviour
{
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Tooltip("Nama-nama Exposed Parameter di Audio Mixer, harus PERSIS sama dengan yang dipakai VolumeSliderControl")]
    [SerializeField] private string[] exposedParameters = { "MainVolume", "SFXVolume", "MusicVolume" };

    [SerializeField] private float maxSliderValue = 10f;
    [SerializeField] private float defaultValue = 10f;

    private const string PrefKeyPrefix = "VOL_";

    private void Awake()
    {
        ApplyAllSettings();
    }

    private void ApplyAllSettings()
    {
        foreach (var param in exposedParameters)
        {
            float saved = PlayerPrefs.GetFloat(PrefKeyPrefix + param, defaultValue);
            float normalized = Mathf.Clamp(saved / maxSliderValue, 0.0001f, 1f);
            float dB = Mathf.Log10(normalized) * 20f;
            audioMixer.SetFloat(param, dB);
        }
    }
}
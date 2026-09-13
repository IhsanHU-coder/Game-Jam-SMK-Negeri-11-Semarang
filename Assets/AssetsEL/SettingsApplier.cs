using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Bootstrap + satu-satunya "pemilik" referensi AudioMixer di seluruh game.
/// - Singleton persisten (DontDestroyOnLoad): dibuat sekali di scene paling awal,
///   lalu ikut terbawa ke semua scene berikutnya (Main Menu, Gameplay, dst).
/// - Saat start: apply semua volume tersimpan dari PlayerPrefs ke Mixer.
/// - Selama main: dipanggil oleh VolumeSliderControl setiap kali slider digeser,
///   supaya perubahan langsung ke-apply real-time ke Mixer yang sama persis,
///   di scene manapun slider itu berada.
///
/// PENTING: taruh script ini di GameObject pada scene PALING AWAL (mis. scene "Bootstrap"),
/// dan JANGAN taruh lagi di scene lain — cukup satu, karena dia akan bertahan
/// (DontDestroyOnLoad) ke semua scene lain secara otomatis.
/// </summary>
public class SettingsApplier : MonoBehaviour
{
    public static SettingsApplier Instance { get; private set; }

    [Header("Audio Mixer (satu-satunya reference di seluruh game)")]
    [SerializeField] private AudioMixer audioMixer;

    [Tooltip("Nama-nama Exposed Parameter di Audio Mixer, harus PERSIS sama dengan yang dipakai di UI")]
    [SerializeField] private string[] exposedParameters = { "MainVolume", "SFXVolume", "MusicVolume" };

    [SerializeField] private float maxSliderValue = 10f;
    [SerializeField] private float defaultValue = 10f;

    private const string PrefKeyPrefix = "VOL_";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // Sudah ada instance persisten dari scene sebelumnya, hancurkan yang duplikat
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // JANGAN panggil ApplyAllSettings() di sini.
        // AudioMixer.SetFloat() yang dipanggil sebelum ada AudioListener aktif
        // di scene bisa return true (nama parameter valid) TAPI nilainya gak
        // beneran ke-latch ke DSP graph mixer, karena audio engine belum
        // punya jalur aktif buat diproses. Kalau GameObject ini ada di scene
        // paling awal sebelum Main Camera/Player (pembawa AudioListener)
        // sempat spawn, panggilan di Awake() bakal "hilang" walau
        // audioMixer.SetFloat() bilang sukses lewat return value-nya.
        // Ditunda ke Start() supaya Awake() semua object lain di scene
        // (termasuk yang men-spawn AudioListener) sempat selesai duluan.
    }

    private void Start()
    {
        ApplyAllSettings();
    }

    /// <summary>
    /// Apply semua parameter dari PlayerPrefs ke Mixer. Dipanggil otomatis saat game start.
    /// </summary>
    private void ApplyAllSettings()
    {
        foreach (var param in exposedParameters)
        {
            float saved = GetSavedSliderValue(param);
            ApplyToMixer(param, saved);
        }
    }

    /// <summary>
    /// Ambil nilai slider tersimpan (0-maxSliderValue) untuk parameter tertentu.
    /// Dipanggil oleh VolumeSliderControl saat slider di-Start() untuk inisialisasi posisi & teks.
    /// </summary>
    public float GetSavedSliderValue(string exposedParameter)
    {
        return PlayerPrefs.GetFloat(PrefKeyPrefix + exposedParameter, defaultValue);
    }

    /// <summary>
    /// Set volume + simpan ke PlayerPrefs. Dipanggil oleh VolumeSliderControl setiap kali
    /// nilai slider berubah (real-time), di scene manapun.
    /// </summary>
    public void SetVolume(string exposedParameter, float sliderValue)
    {
        ApplyToMixer(exposedParameter, sliderValue);
        PlayerPrefs.SetFloat(PrefKeyPrefix + exposedParameter, sliderValue);
    }

    private void ApplyToMixer(string exposedParameter, float sliderValue)
    {
        float normalized = Mathf.Clamp(sliderValue / maxSliderValue, 0.0001f, 1f);
        float dB = Mathf.Log10(normalized) * 20f;

        bool applied = audioMixer.SetFloat(exposedParameter, dB);
        Debug.Log($"[SettingsApplier] param='{exposedParameter}' sliderValue={sliderValue} dB={dB:F1} success={applied}");
    }
}
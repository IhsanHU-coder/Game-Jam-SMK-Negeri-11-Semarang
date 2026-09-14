using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Serializable]
    public class SoundEntry
    {
        public string id;          // nama unik, misal "ButtonHover", "ButtonClick"
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
    }

    [Header("Output")]
    [SerializeField] private AudioMixerGroup sfxMixerGroup;

    [Header("Sound Library")]
    [Tooltip("Daftar semua sound yang bisa dipanggil lewat ID dari mana saja")]
    [SerializeField] private List<SoundEntry> sounds = new List<SoundEntry>();

    [Header("Source Pool")]
    [SerializeField] private int poolSize = 8;

    private Dictionary<string, SoundEntry> library;
    private AudioSource[] sourcePool;
    private int nextIndex = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Build dictionary dari list biar lookup by ID cepat
        library = new Dictionary<string, SoundEntry>();
        foreach (var entry in sounds)
        {
            if (!library.ContainsKey(entry.id))
            {
                library.Add(entry.id, entry);
            }
            else
            {
                Debug.LogWarning($"[AudioManager] ID duplikat ditemukan: {entry.id}");
            }
        }

        // Siapkan pool AudioSource
        sourcePool = new AudioSource[poolSize];
        for (int i = 0; i < poolSize; i++)
        {
            AudioSource src = gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.outputAudioMixerGroup = sfxMixerGroup;
            sourcePool[i] = src;
        }
    }

    /// <summary>
    /// Mainkan sound berdasarkan ID yang terdaftar di library.
    /// </summary>
    public void PlaySFX(string id)
    {
        if (!library.TryGetValue(id, out SoundEntry entry))
        {
            Debug.LogWarning($"[AudioManager] Sound dengan ID '{id}' tidak ditemukan.");
            return;
        }

        if (entry.clip == null) return;

        AudioSource src = sourcePool[nextIndex];
        nextIndex = (nextIndex + 1) % poolSize;

        src.PlayOneShot(entry.clip, entry.volume);
    }
}
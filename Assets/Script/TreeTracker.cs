using System;
using UnityEngine;

/// <summary>
/// Melacak berapa banyak pohon (Tree) yang ada di scene dan berapa yang sudah
/// ditebang. Setiap GameObject Tree otomatis mendaftar ke sini saat aktif
/// (lihat Tree.cs), jadi tidak perlu setup manual jumlah pohon di Inspector.
/// </summary>
public static class TreeTracker
{
    public static int TotalTrees { get; private set; }
    public static int RemainingTrees { get; private set; }

    /// <summary>Dipanggil sekali, tepat saat pohon TERAKHIR selesai ditebang.</summary>
    public static event Action OnAllTreesChopped;

    /// <summary>
    /// PENTING: static field di C# TIDAK otomatis ter-reset setiap kali kamu
    /// tekan Play di Editor (kalau "Domain Reload" dinonaktifkan di Project
    /// Settings > Editor, demi mempercepat waktu masuk Play Mode). Tanpa reset
    /// ini, sisa hitungan dari sesi Play sebelumnya akan "nempel" dan bikin
    /// panel ending muncul terlalu cepat. Attribute di bawah memaksa reset
    /// terjadi setiap kali game BENAR-BENAR mulai (baik di Editor maupun build),
    /// SEBELUM scene manapun sempat menjalankan Awake().
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnGameStart()
    {
        ResetTracker();
    }

    /// <summary>Dipanggil oleh Tree.cs saat pohon itu aktif/muncul di scene.</summary>
    public static void RegisterTree()
    {
        TotalTrees++;
        RemainingTrees++;
    }

    /// <summary>Dipanggil oleh Tree.cs saat pohon itu selesai ditebang.</summary>
    public static void ReportTreeChopped()
    {
        if (RemainingTrees > 0) RemainingTrees--;

        if (RemainingTrees <= 0 && TotalTrees > 0)
        {
            OnAllTreesChopped?.Invoke();
        }
    }

    /// <summary>
    /// Panggil ini di awal scene (misal dari sebuah GameManager) kalau perlu reset
    /// hitungan, contoh saat scene di-reload/replay.
    /// </summary>
    public static void ResetTracker()
    {
        TotalTrees = 0;
        RemainingTrees = 0;
    }
}
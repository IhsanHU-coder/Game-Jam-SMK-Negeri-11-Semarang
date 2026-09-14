using UnityEngine;

public class InteractZone : MonoBehaviour
{
    [Tooltip("Jari-jari area interaksi (dalam unit world). Ditampilkan sebagai lingkaran putih di Scene view.")]
    public float radius = 1.5f;

    [Tooltip("Tag GameObject player, dipakai untuk mencari & mengecek jaraknya ke zona ini")]
    public string playerTag = "Player";

    [Tooltip("Warna lingkaran gizmo di Scene view")]
    public Color gizmoColor = Color.white;

    /// <summary>
    /// True jika player saat ini berada di dalam radius zona ini.
    /// </summary>
    public bool IsPlayerInRange { get; private set; }

    private Transform playerTransform;

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
        else
        {
            Debug.LogWarning($"[InteractZone] Tidak menemukan GameObject dengan tag '{playerTag}'. Pastikan player sudah diberi tag yang benar.");
        }
    }

    private void Update()
    {
        if (playerTransform == null)
        {
            // Coba cari lagi player, siapa tahu baru muncul (misal scene player dimuat belakangan)
            GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObj != null) playerTransform = playerObj.transform;
            return;
        }

        float distance = Vector2.Distance(transform.position, playerTransform.position);
        IsPlayerInRange = distance <= radius;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
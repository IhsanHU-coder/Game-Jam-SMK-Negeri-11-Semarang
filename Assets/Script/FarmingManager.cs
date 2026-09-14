using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

public class FarmingManager : MonoBehaviour
{
    [Header("Tilemap Lahan Tanam")]
    public Tilemap plantableTilemap;
    public TileBase emptySoilTile;
    public TileBase plantedTile;

    [Header("Deteksi Kotak Biji")]
    [Tooltip("Layer khusus untuk kotak biji, supaya klik kotak biji tidak tertukar dengan klik tile")]
    public LayerMask seedLayerMask;

    [Header("Jarak Interaksi")]
    [Tooltip("InteractZone yang menaungi seluruh area lahan tanam (opsional, pengecekan umum). Player harus berada di dalam lingkaran ini untuk bisa menanam di manapun dalam lahan.")]
    public InteractZone farmlandZone;

    [Tooltip("Jarak maksimal dari player ke TILE SPESIFIK yang diklik, supaya tidak bisa menanam di tile yang jauh walau masih dalam farmlandZone")]
    public float maxPlantDistance = 1.5f;

    [Tooltip("Tag GameObject player, dipakai untuk mengecek jarak ke tile spesifik")]
    public string playerTag = "Player";

    private Transform playerTransform;

    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;

        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null) playerTransform = playerObj.transform;
    }

    private void Update()
    {
        // Menggunakan Input System (baru), bukan UnityEngine.Input (lama)
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            HandleLeftClick();
        }
    }

    private void HandleLeftClick()
    {
        Vector2 screenPos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(screenPos);
        mouseWorldPos.z = 0f;

        // 1. Cek dulu apakah yang diklik adalah kotak biji
        Collider2D seedHit = Physics2D.OverlapPoint(mouseWorldPos, seedLayerMask);
        if (seedHit != null && seedHit.TryGetComponent(out SeedPickup seedPickup))
        {
            InteractZone seedZone = seedHit.GetComponent<InteractZone>();
            bool inRange = seedZone == null || seedZone.IsPlayerInRange; // kalau tidak ada InteractZone, lewati pengecekan

            if (!inRange)
            {
                Debug.Log("[FarmingManager] Terlalu jauh dari kotak biji, dekati dulu.");
                return;
            }

            seedPickup.Collect();
            return; // sudah ambil biji, jangan lanjut ke logika menanam
        }

        // 2. Jika bukan kotak biji, cek apakah yang diklik adalah tile yang bisa ditanami
        TryPlantAt(mouseWorldPos);
    }

    private void TryPlantAt(Vector3 worldPosition)
    {
        if (plantableTilemap == null) return;

        Vector3Int cellPosition = plantableTilemap.WorldToCell(worldPosition);
        TileBase currentTile = plantableTilemap.GetTile(cellPosition);

        // Hanya boleh menanam jika tile di posisi itu adalah tanah kosong
        if (currentTile != emptySoilTile)
        {
            // Bukan tile yang bisa ditanami (mungkin di luar lahan, atau sudah ada tanaman)
            return;
        }

        // Cek apakah player berada di dalam zona lahan tanam secara umum (opsional)
        if (farmlandZone != null && !farmlandZone.IsPlayerInRange)
        {
            Debug.Log("[FarmingManager] Terlalu jauh dari area lahan tanam, dekati dulu.");
            return;
        }

        // Cek jarak player ke TILE SPESIFIK yang diklik, supaya tidak bisa menanam
        // di tile yang jauh walaupun masih berada di dalam farmlandZone
        Vector3 tileCenterWorld = plantableTilemap.GetCellCenterWorld(cellPosition);
        if (playerTransform != null)
        {
            float distToTile = Vector2.Distance(playerTransform.position, tileCenterWorld);
            if (distToTile > maxPlantDistance)
            {
                Debug.Log($"[FarmingManager] Tile ini terlalu jauh dari player (jarak: {distToTile:F2}, maksimal: {maxPlantDistance}).");
                return;
            }

            // Cek apakah tile yang mau ditanam adalah tile yang SEDANG diinjak player sendiri
            Vector3Int playerCell = plantableTilemap.WorldToCell(playerTransform.position);
            if (cellPosition == playerCell)
            {
                Debug.Log("[FarmingManager] Tidak bisa menanam di tile yang sedang kamu injak. Geser dulu.");
                return;
            }
        }

        // Cek apakah pemain punya biji
        if (SeedInventory.SeedCount <= 0)
        {
            Debug.Log("[FarmingManager] Tidak ada biji! Tidak bisa menanam.");
            // Di sini bisa ditambahkan: mainkan suara "gagal", munculkan teks peringatan di UI, dsb.
            return;
        }

        // Semua syarat terpenuhi -> tanam
        plantableTilemap.SetTile(cellPosition, plantedTile);
        SeedInventory.SeedCount -= 1;

        Debug.Log($"[FarmingManager] Berhasil menanam di {cellPosition}. Sisa biji: {SeedInventory.SeedCount}/{SeedInventory.MaxSeedCount}");
    }
}
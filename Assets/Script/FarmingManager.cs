using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FarmingManager : MonoBehaviour
{
    private enum FarmingPhase
    {
        OpeningDialogue,
        Planting,
        WaitingForWaterDialogue,
        Watering,
        Finished
    }

    [Header("Farming State")]
    [SerializeField]
    private FarmingPhase currentPhase =
        FarmingPhase.OpeningDialogue;

    public string CurrentPhaseName => currentPhase.ToString();

    // =========================================================
    // PHASE OBJECTS
    // =========================================================

    [Header("Phase Objects")]
    [Tooltip("SeedBox yang sudah ada di scene sejak awal.")]
    public GameObject seedObject;

    [Tooltip("Sumur yang sudah ada di scene sejak awal.")]
    public GameObject wateringObject;

    // =========================================================
    // INTERACTION OBJECTS
    // =========================================================

    [Header("Interaction Objects")]
    [Tooltip("Script SeedPickup pada SeedBox.")]
    public SeedPickup seedBoxInteraction;

    [Tooltip("Script WellRefill pada Sumur.")]
    public WellRefill wellInteraction;

    // =========================================================
    // PLAYER MOVEMENT
    // =========================================================

    [Header("Player Movement")]
    public PlayerMovement playerMovement;

    // =========================================================
    // TILEMAP
    // =========================================================

    [Header("Tilemap Lahan Tanam")]
    public Tilemap plantableTilemap;
    public TileBase emptySoilTile;
    public TileBase plantedTile;
    public TileBase wateredTile;

    // =========================================================
    // JARAK INTERAKSI
    // =========================================================

    [Header("Jarak Interaksi")]
    public InteractZone farmlandZone;
    public float maxPlantDistance = 1.5f;
    public float minPlantDistance = 0.4f;
    public string playerTag = "Player";
    public Transform playerFeet;

    // =========================================================
    // ICON PLANTING
    // =========================================================

    [Header("Icon Interact - Planting")]
    public GameObject soilInteractIconPrefab;
    public Transform soilIconPoolParent;
    public float soilIconYOffset = 0.5f;
    public int soilSearchRadiusCells = 2;

    private readonly List<GameObject> soilIconPool =
        new List<GameObject>();

    private readonly List<Vector3Int> validTilesInRange =
        new List<Vector3Int>();

    // =========================================================
    // ICON WATERING
    // =========================================================

    [Header("Icon Interact - Watering")]
    public GameObject waterInteractIconPrefab;
    public Transform waterIconPoolParent;
    public float waterIconYOffset = 0.5f;

    private readonly List<GameObject> waterIconPool =
        new List<GameObject>();

    // =========================================================
    // HOLD TO PLANT
    // =========================================================

    [Header("Hold to Plant")]
    public float plantHoldDuration = 2f;
    public GameObject holdIndicatorPrefab;
    public float holdIndicatorYOffset = 0.5f;

    private GameObject holdIndicatorInstance;
    private Slider holdIndicatorSlider;
    private Image holdIndicatorFillImage;

    private bool isHolding = false;
    private float currentHoldTime = 0f;
    private Vector3Int? currentHoldCell = null;

    public static bool IsHoldingPlant { get; private set; } = false;

    // =========================================================
    // WATER DIALOGUE
    // =========================================================

    [Header("Water Dialogue")]
    public WaterDialogue waterDialogue;
    public GameObject[] gameplayUIToHide;

    [Header("Freeze Awal / Akhir")]
    [SerializeField] private float openingFreezeDuration = 3f;
    [SerializeField] private float endingFreezeDuration = 3f;

    // =========================================================
    // UI PLANTING / WATERING
    // =========================================================

    [Header("UI Planting / Watering")]
    public GameObject seedUI;
    public GameObject waterUI;

    // =========================================================
    // WATERING
    // =========================================================

    [Header("Watering")]
    public bool resetWaterWhenWateringStarts = true;

    private readonly HashSet<Vector3Int> wateredTiles =
        new HashSet<Vector3Int>();

    private int wateredTileCount = 0;

    // =========================================================
    // ENDING
    // =========================================================

    [Header("Pindah ke Scene Ending")]
    public string endingSceneName = "Ending";

    private int totalPlantableTiles = 0;
    private int plantedTileCount = 0;
    private bool hasTriggeredEnding = false;

    // Menandakan bahwa dialogue yang sedang berjalan
    // adalah dialogue ending.
    private bool isEndingDialogue = false;

    private Transform playerTransform;
    private Camera mainCamera;

    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        mainCamera = Camera.main;

        GameObject playerObj =
            GameObject.FindGameObjectWithTag(playerTag);

        if (playerObj != null)
        {
            playerTransform = playerObj.transform;

            if (playerFeet == null)
            {
                Transform autoFeet =
                    playerObj.transform.Find("FeetPoint");

                if (autoFeet != null)
                    playerFeet = autoFeet;
            }

            if (playerMovement == null)
            {
                playerMovement =
                    playerObj.GetComponent<PlayerMovement>();
            }
        }

        CountTotalPlantableTiles();
    }

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        if (waterDialogue != null)
        {
            waterDialogue.OnDialogueFinished +=
                OnWaterDialogueFinished;
        }

        StartCoroutine(StartOpeningSequence());
    }

    private IEnumerator StartOpeningSequence()
    {
        ApplyOpeningDialoguePhase();

        Debug.Log($"[FarmingManager] Freeze awal {openingFreezeDuration} detik.");

        yield return new WaitForSeconds(openingFreezeDuration);

        if (currentPhase != FarmingPhase.OpeningDialogue)
            yield break;

        if (waterDialogue != null)
        {
            waterDialogue.ShowOpening();
        }
        else
        {
            Debug.LogError("[FarmingManager] WaterDialogue belum diassign!");
            ApplySeedPhase();
        }
    }

    // =========================================================
    // DESTROY
    // =========================================================

    private void OnDestroy()
    {
        if (waterDialogue != null)
        {
            waterDialogue.OnDialogueFinished -=
                OnWaterDialogueFinished;
        }
    }

    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        switch (currentPhase)
        {
            // =================================================
            // OPENING DIALOGUE
            // =================================================

            case FarmingPhase.OpeningDialogue:

                HideAllSoilIcons();
                HideAllWaterIcons();

                break;

            // =================================================
            // PLANTING
            // =================================================

            case FarmingPhase.Planting:

                RefreshValidTilesInRange();
                HandlePlantHold();
                UpdateSoilInteractIcon();

                break;

            // =================================================
            // DIALOGUE SEBELUM WATERING
            // =================================================

            case FarmingPhase.WaitingForWaterDialogue:

                HideAllSoilIcons();
                HideAllWaterIcons();

                break;

            // =================================================
            // WATERING
            // =================================================

            case FarmingPhase.Watering:

                RefreshValidTilesInRange();
                HandleWateringClick();
                UpdateWaterInteractIcon();

                break;

            // =================================================
            // FINISHED
            // =================================================

            case FarmingPhase.Finished:

                HideAllSoilIcons();
                HideAllWaterIcons();

                break;
        }
    }

    // =========================================================
    // PHASE - OPENING DIALOGUE
    // =========================================================

    private void ApplyOpeningDialoguePhase()
    {
        currentPhase =
            FarmingPhase.OpeningDialogue;

        CancelHold();

        HideAllSoilIcons();
        HideAllWaterIcons();

        // Gameplay UI disembunyikan
        HidePlantingUI();

        // =====================================================
        // OBJECT TETAP ADA DI SCENE
        // =====================================================

        if (seedObject != null)
            seedObject.SetActive(true);

        if (wateringObject != null)
            wateringObject.SetActive(true);

        // =====================================================
        // KEDUA INTERACTION DIKUNCI
        // =====================================================

        if (seedBoxInteraction != null)
            seedBoxInteraction.SetInteractionEnabled(false);

        if (wellInteraction != null)
            wellInteraction.SetInteractionEnabled(false);

        // =====================================================
        // PLAYER DIKUNCI
        // =====================================================

        LockPlayerMovement();

        Debug.Log(
            "[FarmingManager] OPENING DIALOGUE DIMULAI."
        );

        // Opening dialogue ditampilkan setelah freeze awal selesai.
    }

    // =========================================================
    // PHASE - PLANTING
    // =========================================================

    private void ApplySeedPhase()
    {
        currentPhase =
            FarmingPhase.Planting;

        // Object tetap ada di scene
        if (seedObject != null)
            seedObject.SetActive(true);

        if (wateringObject != null)
            wateringObject.SetActive(true);

        // SeedBox bisa interact
        if (seedBoxInteraction != null)
            seedBoxInteraction.SetInteractionEnabled(true);

        // Sumur belum bisa interact
        if (wellInteraction != null)
            wellInteraction.SetInteractionEnabled(false);

        UnlockPlayerMovement();

        ShowPlantingUI();

        Debug.Log(
            "[FarmingManager] SEED PHASE DIMULAI. " +
            "SeedBox aktif, Well terkunci."
        );
    }

    // =========================================================
    // PHASE - DIALOGUE
    // =========================================================

    private void ApplyDialoguePhase()
    {
        currentPhase =
            FarmingPhase.WaitingForWaterDialogue;

        CancelHold();

        HideAllSoilIcons();
        HideAllWaterIcons();
        HidePlantingUI();

        // =====================================================
        // OBJECT TETAP ADA
        // =====================================================

        if (seedObject != null)
            seedObject.SetActive(true);

        if (wateringObject != null)
            wateringObject.SetActive(true);

        // =====================================================
        // KEDUANYA DIKUNCI SELAMA DIALOGUE
        // =====================================================

        if (seedBoxInteraction != null)
            seedBoxInteraction.SetInteractionEnabled(false);

        if (wellInteraction != null)
            wellInteraction.SetInteractionEnabled(false);

        LockPlayerMovement();

        Debug.Log(
            "[FarmingManager] DIALOGUE PHASE DIMULAI. " +
            "SeedBox dan Well terkunci."
        );

        if (waterDialogue != null)
        {
            waterDialogue.Show();
        }
        else
        {
            Debug.LogError(
                "[FarmingManager] WATER DIALOGUE BELUM DIASSIGN DI INSPECTOR!"
            );
        }
    }

    // =========================================================
    // PHASE - WATERING
    // =========================================================

    private void ApplyWateringPhase()
    {
        currentPhase =
            FarmingPhase.Watering;

        // =====================================================
        // OBJECT TETAP ADA DI SCENE
        // =====================================================

        if (seedObject != null)
            seedObject.SetActive(true);

        if (wateringObject != null)
            wateringObject.SetActive(true);

        // =====================================================
        // SEEDBOX DIKUNCI
        // SUMUR DIBUKA
        // =====================================================

        if (seedBoxInteraction != null)
            seedBoxInteraction.SetInteractionEnabled(false);

        if (wellInteraction != null)
            wellInteraction.SetInteractionEnabled(true);

        wateredTiles.Clear();
        wateredTileCount = 0;

        if (resetWaterWhenWateringStarts)
            WaterInventory.ResetWater();

        ShowWateringUI();

        UnlockPlayerMovement();

        Debug.Log(
            "[FarmingManager] WATERING PHASE DIMULAI. " +
            "SeedBox terkunci, Well bisa digunakan."
        );
    }

    // =========================================================
    // PLAYER LOCK
    // =========================================================

    private void LockPlayerMovement()
    {
        if (playerMovement == null)
            return;

        playerMovement.SetMovementLocked(true);

        Debug.Log(
            "[FarmingManager] Player movement LOCKED."
        );
    }

    private void UnlockPlayerMovement()
    {
        if (playerMovement == null)
            return;

        playerMovement.SetMovementLocked(false);

        Debug.Log(
            "[FarmingManager] Player movement UNLOCKED."
        );
    }

    // =========================================================
    // COUNT TILE
    // =========================================================

    private void CountTotalPlantableTiles()
    {
        totalPlantableTiles = 0;
        plantedTileCount = 0;

        if (plantableTilemap == null)
        {
            Debug.LogWarning(
                "[FarmingManager] Plantable Tilemap belum diassign."
            );

            return;
        }

        BoundsInt bounds =
            plantableTilemap.cellBounds;

        foreach (Vector3Int cell in bounds.allPositionsWithin)
        {
            TileBase tile =
                plantableTilemap.GetTile(cell);

            if (tile == emptySoilTile)
            {
                totalPlantableTiles++;
            }
            else if (tile == plantedTile ||
                     tile == wateredTile)
            {
                totalPlantableTiles++;
                plantedTileCount++;
            }
        }

        Debug.Log(
            $"[FarmingManager] Total lahan tanam: {totalPlantableTiles}"
        );
    }

    // =========================================================
    // PLAYER POSITION
    // =========================================================

    private Vector3 PlayerReferencePosition()
    {
        if (playerFeet != null)
            return playerFeet.position;

        if (playerTransform != null)
            return playerTransform.position;

        return Vector3.zero;
    }

    // =========================================================
    // VALID TILE
    // =========================================================

    private void RefreshValidTilesInRange()
    {
        validTilesInRange.Clear();

        if (plantableTilemap == null)
            return;

        Vector3 playerPos =
            PlayerReferencePosition();

        Vector3Int centerCell =
            plantableTilemap.WorldToCell(playerPos);

        int radius =
            Mathf.CeilToInt(maxPlantDistance) + 1;

        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                Vector3Int cell =
                    centerCell +
                    new Vector3Int(x, y, 0);

                if (!plantableTilemap.HasTile(cell))
                    continue;

                TileBase tile =
                    plantableTilemap.GetTile(cell);

                if (currentPhase ==
                    FarmingPhase.Planting)
                {
                    if (tile != emptySoilTile)
                        continue;
                }
                else if (currentPhase ==
                         FarmingPhase.Watering)
                {
                    if (tile != plantedTile)
                        continue;

                    if (wateredTiles.Contains(cell))
                        continue;
                }

                Vector3 worldPos =
                    plantableTilemap.GetCellCenterWorld(cell);

                float distance =
                    Vector2.Distance(
                        playerPos,
                        worldPos
                    );

                if (distance <= maxPlantDistance &&
                    distance >= minPlantDistance)
                {
                    validTilesInRange.Add(cell);
                }
            }
        }
    }

    // =========================================================
    // PLANT HOLD
    // =========================================================

    private void HandlePlantHold()
    {
        if (Mouse.current == null)
            return;

        if (SeedInventory.SeedCount <= 0)
        {
            CancelHold();
            return;
        }

        Vector3Int? mouseCell =
            GetCellUnderMouse();

        if (!mouseCell.HasValue)
        {
            CancelHold();
            return;
        }

        Vector3Int cell =
            mouseCell.Value;

        if (!validTilesInRange.Contains(cell))
        {
            CancelHold();
            return;
        }

        if (!isHolding)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                TryStartHold(cell);
            }

            return;
        }

        if (!Mouse.current.leftButton.isPressed)
        {
            CancelHold();
            return;
        }

        if (!currentHoldCell.HasValue ||
            currentHoldCell.Value != cell)
        {
            CancelHold();
            return;
        }

        currentHoldTime += Time.deltaTime;

        UpdateHoldIndicatorVisual();

        if (currentHoldTime >= plantHoldDuration)
        {
            CompleteHold();
        }
    }

    // =========================================================
    // MOUSE CELL
    // =========================================================

    private Vector3Int? GetCellUnderMouse()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null)
            return null;

        Vector2 mouseWorld =
            mainCamera.ScreenToWorldPoint(
                Mouse.current.position.ReadValue()
            );

        return plantableTilemap.WorldToCell(
            mouseWorld
        );
    }

    // =========================================================
    // START HOLD
    // =========================================================

    private void TryStartHold(Vector3Int cell)
    {
        if (SeedInventory.SeedCount <= 0)
            return;

        TileBase tile =
            plantableTilemap.GetTile(cell);

        if (tile != emptySoilTile)
            return;

        isHolding = true;
        IsHoldingPlant = true;

        currentHoldTime = 0f;
        currentHoldCell = cell;

        ShowHoldIndicator(cell);
        UpdateHoldIndicatorVisual();

        Debug.Log(
            $"[FarmingManager] Mulai menanam di cell {cell}."
        );
    }

    // =========================================================
    // CANCEL HOLD
    // =========================================================

    private void CancelHold()
    {
        isHolding = false;
        IsHoldingPlant = false;

        currentHoldTime = 0f;
        currentHoldCell = null;

        HideHoldIndicator();
    }

    // =========================================================
    // COMPLETE HOLD
    // =========================================================

    private void CompleteHold()
    {
        if (!currentHoldCell.HasValue)
        {
            CancelHold();
            return;
        }

        Vector3Int cell =
            currentHoldCell.Value;

        CancelHold();

        PlantAt(cell);
    }

    // =========================================================
    // HOLD INDICATOR
    // =========================================================

    private void ShowHoldIndicator(Vector3Int cell)
    {
        if (holdIndicatorPrefab == null)
            return;

        if (holdIndicatorInstance == null)
        {
            holdIndicatorInstance =
                Instantiate(
                    holdIndicatorPrefab
                );

            holdIndicatorSlider =
                holdIndicatorInstance
                    .GetComponentInChildren<Slider>(
                        true
                    );

            holdIndicatorFillImage =
                holdIndicatorInstance
                    .GetComponentInChildren<Image>(
                        true
                    );
        }

        Vector3 worldPos =
            plantableTilemap.GetCellCenterWorld(cell);

        holdIndicatorInstance.transform.position =
            worldPos +
            Vector3.up * holdIndicatorYOffset;

        holdIndicatorInstance.SetActive(true);

        if (holdIndicatorSlider != null)
        {
            holdIndicatorSlider.minValue = 0f;
            holdIndicatorSlider.maxValue = 1f;
            holdIndicatorSlider.value = 0f;
        }

        if (holdIndicatorFillImage != null)
        {
            holdIndicatorFillImage.fillAmount = 0f;
        }
    }

    private void HideHoldIndicator()
    {
        if (holdIndicatorInstance != null)
        {
            holdIndicatorInstance.SetActive(false);
        }
    }

    private void UpdateHoldIndicatorVisual()
    {
        if (plantHoldDuration <= 0f)
        {
            SetHoldFillAmount(1f);
            return;
        }

        float progress =
            Mathf.Clamp01(
                currentHoldTime /
                plantHoldDuration
            );

        SetHoldFillAmount(progress);
    }

    private void SetHoldFillAmount(float value)
    {
        if (holdIndicatorSlider != null)
        {
            holdIndicatorSlider.value = value;
        }

        if (holdIndicatorFillImage != null)
        {
            holdIndicatorFillImage.fillAmount = value;
        }
    }

    // =========================================================
    // PLANT ICON
    // =========================================================

    private void UpdateSoilInteractIcon()
    {
        if (soilInteractIconPrefab == null)
            return;

        DeactivateAllSoilIcons();

        int iconIndex = 0;

        foreach (Vector3Int cell in validTilesInRange)
        {
            if (isHolding &&
                currentHoldCell.HasValue &&
                currentHoldCell.Value == cell)
            {
                continue;
            }

            if (iconIndex >= soilIconPool.Count)
            {
                GameObject newIcon =
                    GetPooledSoilIcon();

                if (newIcon == null)
                    break;
            }

            GameObject icon =
                soilIconPool[iconIndex];

            Vector3 worldPos =
                plantableTilemap.GetCellCenterWorld(cell);

            icon.transform.position =
                worldPos +
                Vector3.up * soilIconYOffset;

            icon.SetActive(true);

            iconIndex++;
        }
    }

    private GameObject GetPooledSoilIcon()
    {
        if (soilInteractIconPrefab == null)
            return null;

        GameObject icon =
            Instantiate(
                soilInteractIconPrefab,
                soilIconPoolParent
            );

        icon.SetActive(false);

        soilIconPool.Add(icon);

        return icon;
    }

    private void DeactivateAllSoilIcons()
    {
        foreach (GameObject icon in soilIconPool)
        {
            if (icon != null)
                icon.SetActive(false);
        }
    }

    private void HideAllSoilIcons()
    {
        DeactivateAllSoilIcons();
    }

    // =========================================================
    // PLANT
    // =========================================================

    private void PlantAt(Vector3Int cellPosition)
    {
        if (SeedInventory.SeedCount <= 0)
        {
            Debug.Log(
                "[FarmingManager] Biji habis."
            );

            return;
        }

        TileBase tile =
            plantableTilemap.GetTile(cellPosition);

        if (tile != emptySoilTile)
            return;

        plantableTilemap.SetTile(
            cellPosition,
            plantedTile
        );

        SeedInventory.SeedCount -= 1;
        plantedTileCount++;

        Debug.Log(
            $"[FarmingManager] Berhasil menanam. " +
            $"Progress: {plantedTileCount}/{totalPlantableTiles}"
        );

        if (plantedTileCount >= totalPlantableTiles)
        {
            BeginWateringDialogue();
        }
    }

    // =========================================================
    // DIALOGUE
    // =========================================================

    private void BeginWateringDialogue()
    {
        if (currentPhase !=
            FarmingPhase.Planting)
            return;

        ApplyDialoguePhase();
    }

    private void OnWaterDialogueFinished()
    {
        // =====================================================
        // ENDING DIALOGUE SELESAI
        // =====================================================

        if (isEndingDialogue)
        {
            isEndingDialogue = false;

            Debug.Log(
                "[FarmingManager] Ending dialogue selesai."
            );

            StartCoroutine(
                GoToEndingAfterDelay()
            );

            return;
        }

        // =====================================================
        // OPENING DIALOGUE SELESAI
        // =====================================================

        if (currentPhase ==
            FarmingPhase.OpeningDialogue)
        {
            Debug.Log(
                "[FarmingManager] Opening dialogue selesai. " +
                "Gameplay dimulai."
            );

            ApplySeedPhase();

            return;
        }

        // =====================================================
        // DIALOGUE SEBELUM WATERING SELESAI
        // =====================================================

        if (currentPhase !=
            FarmingPhase.WaitingForWaterDialogue)
            return;

        Debug.Log(
            "[FarmingManager] Dialogue selesai. " +
            "Masuk Watering Phase."
        );

        ApplyWateringPhase();
    }

    // =========================================================
    // WATERING CLICK
    // =========================================================

    private void HandleWateringClick()
    {
        if (Mouse.current == null)
            return;

        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        Vector3Int? mouseCell =
            GetCellUnderMouse();

        if (!mouseCell.HasValue)
            return;

        Vector3Int cell =
            mouseCell.Value;

        if (!validTilesInRange.Contains(cell))
            return;

        WaterAt(cell);
    }

    // =========================================================
    // WATER AT TILE
    // =========================================================

    private void WaterAt(Vector3Int cellPosition)
    {
        if (WaterInventory.IsEmpty)
        {
            Debug.Log(
                "[FarmingManager] Air habis."
            );

            return;
        }

        if (wateredTiles.Contains(cellPosition))
            return;

        TileBase tile =
            plantableTilemap.GetTile(
                cellPosition
            );

        if (tile != plantedTile)
            return;

        plantableTilemap.SetTile(
            cellPosition,
            wateredTile
        );

        WaterInventory.WaterCount -= 1;

        wateredTiles.Add(cellPosition);
        wateredTileCount++;

        Debug.Log(
            $"[FarmingManager] Tanaman disiram. " +
            $"Progress: {wateredTileCount}/{totalPlantableTiles}. " +
            $"Air: {WaterInventory.WaterCount}/{WaterInventory.MaxWater}"
        );

        if (wateredTileCount >= totalPlantableTiles)
        {
            CompleteWatering();
        }
    }

    // =========================================================
    // WATER ICON
    // =========================================================

    private void UpdateWaterInteractIcon()
    {
        if (waterInteractIconPrefab == null)
            return;

        if (plantableTilemap == null)
            return;

        DeactivateAllWaterIcons();

        int iconIndex = 0;

        BoundsInt bounds =
            plantableTilemap.cellBounds;

        foreach (Vector3Int cell in bounds.allPositionsWithin)
        {
            TileBase tile =
                plantableTilemap.GetTile(cell);

            if (tile != plantedTile)
                continue;

            if (wateredTiles.Contains(cell))
                continue;

            if (iconIndex >= waterIconPool.Count)
            {
                GameObject newIcon =
                    GetPooledWaterIcon();

                if (newIcon == null)
                    break;
            }

            GameObject icon =
                waterIconPool[iconIndex];

            Vector3 worldPos =
                plantableTilemap.GetCellCenterWorld(cell);

            icon.transform.position =
                worldPos +
                Vector3.up * waterIconYOffset;

            icon.SetActive(true);

            iconIndex++;
        }
    }

    private GameObject GetPooledWaterIcon()
    {
        if (waterInteractIconPrefab == null)
            return null;

        GameObject icon =
            Instantiate(
                waterInteractIconPrefab,
                waterIconPoolParent
            );

        icon.SetActive(false);

        waterIconPool.Add(icon);

        return icon;
    }

    private void DeactivateAllWaterIcons()
    {
        foreach (GameObject icon in waterIconPool)
        {
            if (icon != null)
                icon.SetActive(false);
        }
    }

    private void HideAllWaterIcons()
    {
        DeactivateAllWaterIcons();
    }

    // =========================================================
    // UI
    // =========================================================

    private void HidePlantingUI()
    {
        if (seedUI != null)
            seedUI.SetActive(false);

        if (waterUI != null)
            waterUI.SetActive(false);

        if (gameplayUIToHide != null)
        {
            foreach (GameObject ui in gameplayUIToHide)
            {
                if (ui != null)
                    ui.SetActive(false);
            }
        }
    }

    private void ShowPlantingUI()
    {
        if (seedUI != null)
            seedUI.SetActive(true);

        if (waterUI != null)
            waterUI.SetActive(false);

        if (gameplayUIToHide != null)
        {
            foreach (GameObject ui in gameplayUIToHide)
            {
                if (ui != null)
                    ui.SetActive(true);
            }
        }
    }

    private void ShowWateringUI()
    {
        if (seedUI != null)
            seedUI.SetActive(false);

        if (waterUI != null)
            waterUI.SetActive(true);

        if (gameplayUIToHide != null)
        {
            foreach (GameObject ui in gameplayUIToHide)
            {
                if (ui != null)
                    ui.SetActive(true);
            }
        }
    }

    private void ShowGameplayUI()
    {
        if (gameplayUIToHide == null)
            return;

        foreach (GameObject ui in gameplayUIToHide)
        {
            if (ui != null)
                ui.SetActive(true);
        }
    }

    // =========================================================
    // FINISH WATERING
    // =========================================================

    private void CompleteWatering()
    {
        if (hasTriggeredEnding)
            return;

        hasTriggeredEnding = true;

        currentPhase =
            FarmingPhase.Finished;

        HideAllSoilIcons();
        HideAllWaterIcons();

        // =====================================================
        // KEDUA INTERACTION DIMATIKAN
        // =====================================================

        if (seedBoxInteraction != null)
            seedBoxInteraction.SetInteractionEnabled(false);

        if (wellInteraction != null)
            wellInteraction.SetInteractionEnabled(false);

        // =====================================================
        // PLAYER TETAP DIKUNCI
        // =====================================================

        LockPlayerMovement();

        Debug.Log(
            "[FarmingManager] SEMUA TANAMAN SUDAH DISIRAM."
        );

        // =====================================================
        // TAMPILKAN ENDING DIALOGUE
        // =====================================================

        if (waterDialogue != null)
        {
            isEndingDialogue = true;

            waterDialogue.ShowEnding();

            Debug.Log(
                "[FarmingManager] Ending dialogue dimulai."
            );
        }
        else
        {
            Debug.LogWarning(
                "[FarmingManager] WaterDialogue tidak tersedia. " +
                "Langsung menuju Ending Scene."
            );

            StartCoroutine(
                GoToEndingAfterDelay()
            );
        }
    }

    // =========================================================
    // MENUJU ENDING SCENE
    // =========================================================

    private IEnumerator GoToEndingAfterDelay()
    {
        // Player tetap terkunci sampai pindah scene
        LockPlayerMovement();

        Debug.Log(
            $"[FarmingManager] Freeze akhir {endingFreezeDuration} detik."
        );

        yield return new WaitForSeconds(
            endingFreezeDuration
        );

        if (string.IsNullOrEmpty(endingSceneName))
        {
            Debug.LogWarning(
                "[FarmingManager] Ending Scene Name kosong."
            );

            yield break;
        }

        Debug.Log(
            $"[FarmingManager] Pindah ke Ending Scene: {endingSceneName}"
        );

        SceneManager.LoadScene(
            endingSceneName
        );
    }
}
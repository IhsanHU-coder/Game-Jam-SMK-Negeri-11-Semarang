using UnityEngine;
using TMPro;

public class SeedMissionWaypoint : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;

    [Tooltip("Target SeedBox")]
    [SerializeField] private Transform seedBoxObjective;

    [Tooltip("Target Well / Sumur")]
    [SerializeField] private Transform wellObjective;

    [SerializeField] private Camera playerCamera;

    [Tooltip("FarmingManager yang mengatur phase")]
    [SerializeField] private FarmingManager farmingManager;

    [Header("UI")]
    [SerializeField] private RectTransform waypoint;
    [SerializeField] private RectTransform arrow;
    [SerializeField] private TMP_Text distanceText;

    [Header("Settings")]
    [SerializeField] private float heightAboveObjective = 2f;

    [Tooltip("Jarak dari pinggir layar")]
    [SerializeField] private float screenEdgePadding = 100f;

    [Tooltip("Saat jarak lebih dekat dari ini, marker akan menempel di atas object")]
    [SerializeField] private float objectiveDistance = 10f;

    [Tooltip("Ukuran minimum direction agar tidak error ketika target tepat di tengah")]
    [SerializeField] private float minimumDirection = 0.01f;

    private RectTransform canvasRect;

    // =========================================================
    // STATE
    // =========================================================

    private bool shouldShowWaypoint = false;
    private Transform currentObjective = null;

    // =========================================================
    // ENABLE / DISABLE
    // =========================================================

    private void OnEnable()
    {
        SeedInventory.OnSeedCountChanged += HandleSeedCountChanged;
        WaterInventory.OnWaterCountChanged += HandleWaterCountChanged;

        HandleSeedCountChanged(SeedInventory.SeedCount);
        HandleWaterCountChanged(WaterInventory.WaterCount);
    }

    private void OnDisable()
    {
        SeedInventory.OnSeedCountChanged -= HandleSeedCountChanged;
        WaterInventory.OnWaterCountChanged -= HandleWaterCountChanged;
    }

    // =========================================================
    // SEED STATE
    // =========================================================

    private void HandleSeedCountChanged(int count)
    {
        UpdateWaypointState();
    }

    // =========================================================
    // WATER STATE
    // =========================================================

    private void HandleWaterCountChanged(int count)
    {
        UpdateWaypointState();
    }

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        Canvas canvas = GetComponentInParent<Canvas>();

        if (canvas != null)
        {
            canvasRect =
                canvas.GetComponent<RectTransform>();
        }

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        if (farmingManager == null)
        {
            farmingManager =
                FindFirstObjectByType<FarmingManager>();
        }

        UpdateWaypointState();
    }

    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (player == null ||
            playerCamera == null ||
            waypoint == null ||
            farmingManager == null)
        {
            return;
        }

        UpdateWaypointState();

        if (!shouldShowWaypoint ||
            currentObjective == null)
        {
            return;
        }

        UpdateWaypoint();
        UpdateDistance();
    }

    // =========================================================
    // CHECK PHASE
    // =========================================================

    private void UpdateWaypointState()
    {
        if (farmingManager == null)
            return;

        string phase =
            farmingManager.CurrentPhaseName;

        // =====================================================
        // PLANTING
        // =====================================================

        if (phase == "Planting")
        {
            currentObjective = seedBoxObjective;

            // SeedBox dituju hanya kalau seed habis
            shouldShowWaypoint =
                SeedInventory.SeedCount <= 0;
        }

        // =====================================================
        // WATERING
        // =====================================================

        else if (phase == "Watering")
        {
            currentObjective = wellObjective;

            // Well dituju hanya kalau air habis
            shouldShowWaypoint =
                WaterInventory.IsEmpty;
        }

        // =====================================================
        // DIALOGUE / FINISHED
        // =====================================================

        else
        {
            currentObjective = null;
            shouldShowWaypoint = false;
        }

        if (!shouldShowWaypoint)
        {
            HideWaypointUI();
        }
        else
        {
            ShowWaypointUI();
        }
    }

    // =========================================================
    // SHOW / HIDE
    // =========================================================

    private void ShowWaypointUI()
    {
        if (waypoint != null &&
            !waypoint.gameObject.activeSelf)
        {
            waypoint.gameObject.SetActive(true);
        }

        if (distanceText != null &&
            !distanceText.gameObject.activeSelf)
        {
            distanceText.gameObject.SetActive(true);
        }
    }

    private void HideWaypointUI()
    {
        if (waypoint != null &&
            waypoint.gameObject.activeSelf)
        {
            waypoint.gameObject.SetActive(false);
        }

        if (arrow != null &&
            arrow.gameObject.activeSelf)
        {
            arrow.gameObject.SetActive(false);
        }

        if (distanceText != null &&
            distanceText.gameObject.activeSelf)
        {
            distanceText.gameObject.SetActive(false);
        }
    }

    // =========================================================
    // UPDATE WAYPOINT
    // =========================================================

    private void UpdateWaypoint()
    {
        float distance =
            Vector3.Distance(
                player.position,
                currentObjective.position
            );

        Vector3 targetPosition =
            currentObjective.position +
            Vector3.up * heightAboveObjective;

        Vector3 screenPosition =
            playerCamera.WorldToScreenPoint(
                targetPosition
            );

        bool targetBehindCamera =
            screenPosition.z < 0f;

        bool targetOnScreen =
            IsTargetOnScreen(screenPosition);

        if (distance <= objectiveDistance &&
            targetOnScreen &&
            !targetBehindCamera)
        {
            ShowAboveObjective(screenPosition);
        }
        else
        {
            ShowArrow(
                screenPosition,
                targetBehindCamera
            );
        }
    }

    // =========================================================
    // SCREEN CHECK
    // =========================================================

    private bool IsTargetOnScreen(
        Vector3 screenPosition)
    {
        return
            screenPosition.x > 0 &&
            screenPosition.x < Screen.width &&
            screenPosition.y > 0 &&
            screenPosition.y < Screen.height;
    }

    // =========================================================
    // OBJECTIVE MARKER
    // =========================================================

    private void ShowAboveObjective(
        Vector3 screenPosition)
    {
        waypoint.position = screenPosition;

        if (arrow != null)
            arrow.gameObject.SetActive(false);
    }

    // =========================================================
    // ARROW
    // =========================================================

    private void ShowArrow(
        Vector3 screenPosition,
        bool targetBehindCamera)
    {
        if (arrow == null)
            return;

        arrow.gameObject.SetActive(true);

        Vector2 screenCenter =
            new Vector2(
                Screen.width * 0.5f,
                Screen.height * 0.5f
            );

        Vector2 targetScreenPosition =
            new Vector2(
                screenPosition.x,
                screenPosition.y
            );

        Vector2 direction =
            targetScreenPosition -
            screenCenter;

        if (targetBehindCamera)
        {
            direction = -direction;
        }

        if (direction.magnitude < minimumDirection)
        {
            direction = Vector2.up;
        }

        direction.Normalize();

        float halfWidth =
            Screen.width * 0.5f -
            screenEdgePadding;

        float halfHeight =
            Screen.height * 0.5f -
            screenEdgePadding;

        float scaleX =
            Mathf.Abs(direction.x) > minimumDirection
            ? halfWidth / Mathf.Abs(direction.x)
            : Mathf.Infinity;

        float scaleY =
            Mathf.Abs(direction.y) > minimumDirection
            ? halfHeight / Mathf.Abs(direction.y)
            : Mathf.Infinity;

        float scale =
            Mathf.Min(
                scaleX,
                scaleY
            );

        Vector2 finalScreenPosition =
            screenCenter +
            direction * scale;

        waypoint.position =
            finalScreenPosition;

        float angle =
            Mathf.Atan2(
                direction.y,
                direction.x
            ) * Mathf.Rad2Deg;

        arrow.rotation =
            Quaternion.Euler(
                0f,
                0f,
                angle - 90f
            );
    }

    // =========================================================
    // DISTANCE
    // =========================================================

    private void UpdateDistance()
    {
        if (distanceText == null ||
            currentObjective == null)
        {
            return;
        }

        float distance =
            Vector3.Distance(
                player.position,
                currentObjective.position
            );

        distanceText.text =
            Mathf.RoundToInt(distance) + "m";
    }
}
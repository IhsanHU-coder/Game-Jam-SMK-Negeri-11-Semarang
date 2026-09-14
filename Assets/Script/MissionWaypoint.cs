using UnityEngine;
using TMPro;

public class MissionWaypoint : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform objective;
    [SerializeField] private Camera playerCamera;

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

    private void Start()
    {
        Canvas canvas = GetComponentInParent<Canvas>();

        if (canvas != null)
        {
            canvasRect = canvas.GetComponent<RectTransform>();
        }

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (player == null ||
            objective == null ||
            playerCamera == null ||
            waypoint == null)
        {
            return;
        }

        UpdateWaypoint();
        UpdateDistance();
    }

    private void UpdateWaypoint()
    {
        float distance = Vector3.Distance(
            player.position,
            objective.position
        );

        Vector3 targetPosition =
            objective.position +
            Vector3.up * heightAboveObjective;

        Vector3 screenPosition =
            playerCamera.WorldToScreenPoint(targetPosition);

        bool targetBehindCamera =
            screenPosition.z < 0f;

        bool targetOnScreen =
            IsTargetOnScreen(screenPosition);

        // ==========================================
        // OBJECTIVE SUDAH DEKAT + TERLIHAT
        // ==========================================

        if (distance <= objectiveDistance &&
            targetOnScreen &&
            !targetBehindCamera)
        {
            ShowAboveObjective(screenPosition);
        }
        else
        {
            ShowArrow(screenPosition, targetBehindCamera);
        }
    }

    private bool IsTargetOnScreen(Vector3 screenPosition)
    {
        return
            screenPosition.x > 0 &&
            screenPosition.x < Screen.width &&
            screenPosition.y > 0 &&
            screenPosition.y < Screen.height;
    }

    private void ShowAboveObjective(Vector3 screenPosition)
    {
        waypoint.position = screenPosition;

        arrow.gameObject.SetActive(false);
    }

    private void ShowArrow(
        Vector3 screenPosition,
        bool targetBehindCamera)
    {
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
            targetScreenPosition - screenCenter;

        // ==========================================
        // TARGET DI BELAKANG CAMERA
        // ==========================================

        if (targetBehindCamera)
        {
            direction = -direction;
        }

        // ==========================================
        // TARGET TEPAT DI TENGAH
        // ==========================================

        if (direction.magnitude < minimumDirection)
        {
            direction = Vector2.up;
        }

        direction.Normalize();

        // ==========================================
        // HITUNG POSISI EDGE
        // ==========================================

        float halfWidth =
            Screen.width * 0.5f -
            screenEdgePadding;

        float halfHeight =
            Screen.height * 0.5f -
            screenEdgePadding;

        float x = direction.x * halfWidth;
        float y = direction.y * halfHeight;

        float scaleX =
            Mathf.Abs(direction.x) > minimumDirection
            ? halfWidth / Mathf.Abs(direction.x)
            : Mathf.Infinity;

        float scaleY =
            Mathf.Abs(direction.y) > minimumDirection
            ? halfHeight / Mathf.Abs(direction.y)
            : Mathf.Infinity;

        float scale =
            Mathf.Min(scaleX, scaleY);

        Vector2 finalScreenPosition =
            screenCenter +
            direction * scale;

        // ==========================================
        // SET POSITION
        // ==========================================

        waypoint.position =
            finalScreenPosition;

        // ==========================================
        // ROTATE ARROW
        // ==========================================

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

    private void UpdateDistance()
    {
        float distance =
            Vector3.Distance(
                player.position,
                objective.position
            );

        distanceText.text =
            Mathf.RoundToInt(distance) + "m";
    }
}
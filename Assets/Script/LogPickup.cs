using System.Collections;
using UnityEngine;

public class LogPickup : MonoBehaviour
{
    [Header("Pickup")]
    [SerializeField] private float pickupRange = 2.5f;
    [SerializeField] private float moveSpeed = 8f;

    [Header("Gravity")]
    [SerializeField] private float gravityDuration = 0.5f;

    [Header("Audio")]
    [SerializeField] private string pickupSoundId = "LogPickup";

    private Transform player;
    private LogInventory inventory;

    private bool isMovingToPlayer = false;

    private Rigidbody rb;

    private void Start()
    {
        // === TAMBAHAN ===
        LogPickupTracker.RegisterPickup();
        // === akhir tambahan ===

        rb = GetComponent<Rigidbody>();

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
            inventory = playerObject.GetComponent<LogInventory>();
        }

        StartCoroutine(StopGravity());
    }

    private IEnumerator StopGravity()
    {
        yield return new WaitForSeconds(gravityDuration);

        if (rb != null)
        {
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
        }
    }

    private void Update()
    {
        if (player == null || inventory == null)
            return;

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        if (!isMovingToPlayer && distance <= pickupRange)
        {
            if (inventory.IsFull())
            {
                return;
            }

            isMovingToPlayer = true;

            if (rb != null)
            {
                rb.useGravity = false;
                rb.linearVelocity = Vector3.zero;
            }
        }

        if (isMovingToPlayer)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                player.position,
                moveSpeed * Time.deltaTime
            );

            if (Vector3.Distance(transform.position, player.position) < 0.2f)
            {
                bool success = inventory.AddLog();

                if (success)
                {
                    // Mainkan suara pickup lewat AudioManager
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlaySFX(pickupSoundId);
                    }

                    // === TAMBAHAN ===
                    LogPickupTracker.ReportPickedUp();
                    // === akhir tambahan ===

                    Destroy(gameObject);
                }
                else
                {
                    isMovingToPlayer = false;
                }
            }
        }
    }
}
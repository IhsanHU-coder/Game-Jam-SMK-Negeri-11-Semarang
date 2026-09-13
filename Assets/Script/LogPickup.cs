using UnityEngine;

public class LogPickup : MonoBehaviour
{
    [Header("Pickup")]
    [SerializeField] private float pickupRange = 2.5f;
    [SerializeField] private float moveSpeed = 8f;

    private Transform player;
    private LogInventory inventory;

    private bool isMovingToPlayer = false;

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
            inventory = playerObject.GetComponent<LogInventory>();
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
            // Cek inventory sebelum mengambil
            if (inventory.IsFull())
            {
                return;
            }

            isMovingToPlayer = true;
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
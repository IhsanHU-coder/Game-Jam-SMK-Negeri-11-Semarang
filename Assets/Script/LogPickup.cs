using UnityEngine;

public class LogPickup : MonoBehaviour
{
    [Header("Pickup")]
    [SerializeField] private float pickupRange = 2.5f;
    [SerializeField] private float moveSpeed = 8f;

    private Transform player;
    private bool isMovingToPlayer = false;

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    private void Update()
    {
        if (player == null)
            return;

        float distance = Vector3.Distance(transform.position, player.position);

        // Player is close enough
        if (!isMovingToPlayer && distance <= pickupRange)
        {
            isMovingToPlayer = true;
        }

        // Move toward player
        if (isMovingToPlayer)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                player.position,
                moveSpeed * Time.deltaTime
            );

            // Reached player
            if (Vector3.Distance(transform.position, player.position) < 0.2f)
            {
                Destroy(gameObject);
            }
        }
    }
}
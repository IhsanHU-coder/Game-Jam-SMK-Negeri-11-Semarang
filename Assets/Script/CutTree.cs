using UnityEngine;

public class CutTree : MonoBehaviour
{
    [Header("Tree Settings")]
    [SerializeField] private int treeHealth = 10;

    [Header("Damage Per Attack")]
    [SerializeField] private int minDamage = 1;
    [SerializeField] private int maxDamage = 3;

    [Header("Player Settings")]
    [SerializeField] private Transform player;
    [SerializeField] private float cutRange = 3f;

    [Header("Range Visualization")]
    [SerializeField] private bool showCutRange = true;

    [Header("Log Settings")]
    [SerializeField] private GameObject logPrefab;
    [SerializeField] private int minLogs = 2;
    [SerializeField] private int maxLogs = 4;

    [Header("Tree Visual")]
    [SerializeField] private GameObject normalTree;
    [SerializeField] private GameObject cutTree;

    public void TakeDamage()
    {
        int damage = Random.Range(minDamage, maxDamage + 1);

        treeHealth -= damage;

        Debug.Log(
            "Tree took " + damage +
            " damage. HP: " + treeHealth
        );

        if (treeHealth <= 0)
        {
            CutDownTree();
        }
    }

    private void CutDownTree()
    {
        normalTree.SetActive(false);
        cutTree.SetActive(true);
        int logAmount = Random.Range(
            minLogs,
            maxLogs + 1
        );

        for (int i = 0; i < logAmount; i++)
        {
            Vector3 spawnPosition = transform.position;

            spawnPosition += new Vector3(
                Random.Range(-1.5f, 1.5f),
                Random.Range(0.2f, 0.8f),
                Random.Range(0f, 0f)
            );

            Quaternion randomRotation = Quaternion.Euler(
                Random.Range(-15f, 15f),
                Random.Range(0f, 360f),
                Random.Range(-15f, 15f)
            );

            GameObject log = Instantiate(
                logPrefab,
                spawnPosition,
                randomRotation
            );

            Rigidbody rb = log.GetComponent<Rigidbody>();

            if (rb != null)
            {
                Vector3 randomForce = new Vector3(
                    Random.Range(-1f, 1f),
                    Random.Range(1f, 2f),
                    Random.Range(-1f, 1f)
                );

                rb.AddForce(randomForce, ForceMode.Impulse);
            }
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        if (!showCutRange)
            return;

        Gizmos.DrawWireSphere(
            transform.position,
            cutRange
        );
    }
}
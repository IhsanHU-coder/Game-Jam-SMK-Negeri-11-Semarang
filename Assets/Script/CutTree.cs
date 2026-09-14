using UnityEngine;

public class CutTree : MonoBehaviour
{
    [Header("Tree Settings")]
    [SerializeField] private int treeHealth = 100;

    // minDamage & maxDamage bawaan di CutTree diabaikan / dijadikan fallback

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

    [Header("Info")]
    [SerializeField] private GameObject infoLogging;

    [Header("Particle")]
    public ParticleSystem[] cutParticles;

    public GameObject Axe;
    public Animator axeAnimator;

    private PlayerStats playerStats;

    private void Awake()
    {
        if (infoLogging != null)
        {
            if (Axe != null) Axe.SetActive(false);
            infoLogging.SetActive(false);
        }
    }

    private void Start()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        if (player != null)
        {
            playerStats = player.GetComponent<PlayerStats>();
        }
    }

    private void Update()
    {
        CheckPlayerDistance();
    }

    private void CheckPlayerDistance()
    {
        if (player == null)
        {
            if (infoLogging != null) infoLogging.SetActive(false);
            if (Axe != null) Axe.SetActive(false);
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= cutRange)
        {
            if (infoLogging != null)
            {
                if (Axe != null) Axe.SetActive(true);
                infoLogging.SetActive(true);
            }
        }
        else
        {
            if (infoLogging != null)
            {
                if (Axe != null) Axe.SetActive(false);
                infoLogging.SetActive(false);
            }
        }
    }

    public void TakeDamage()
    {
        if (axeAnimator != null)
            axeAnimator.Play("AxeClick");

        // Ambil min/max damage dari PlayerStats jika ada, kalau tidak gunakan default (1-3)
        int min = (playerStats != null) ? playerStats.minDamage : 1;
        int max = (playerStats != null) ? playerStats.maxDamage : 3;

        int damage = Random.Range(min, max + 1);
        treeHealth -= damage;

        foreach (var particle in cutParticles)
        {
            if (particle != null)
                particle.Play();
        }

        Debug.Log("Tree took " + damage + " damage. HP: " + treeHealth);

        if (treeHealth <= 0)
        {
            if (Axe != null) Axe.SetActive(false);
            if (infoLogging != null) infoLogging.SetActive(false);
            CutDownTree();
        }
    }

    private void CutDownTree()
    {
        if (normalTree != null) normalTree.SetActive(false);
        if (cutTree != null) cutTree.SetActive(true);

        int logAmount = Random.Range(minLogs, maxLogs + 1);

        for (int i = 0; i < logAmount; i++)
        {
            Vector3 spawnPosition = transform.position + new Vector3(
                Random.Range(-1.5f, 1.5f),
                Random.Range(0.2f, 0.8f),
                0f
            );

            Quaternion randomRotation = Quaternion.Euler(
                Random.Range(-15f, 15f),
                Random.Range(0f, 360f),
                Random.Range(-15f, 15f)
            );

            GameObject log = Instantiate(logPrefab, spawnPosition, randomRotation);
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
        if (!showCutRange) return;
        Gizmos.DrawWireSphere(transform.position, cutRange);
    }
}
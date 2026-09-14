using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 3f;

    public InputManager inputManager;
    private Camera mainCamera;

    public GameObject infoLogging;

    private void Awake()
    {
        mainCamera = Camera.main;

        if (infoLogging != null)
            infoLogging.SetActive(false);
    }

    private void Update()
    {
        CheckTreeRange();

        if (inputManager.AttackPressed)
        {
            Attack();

            inputManager.ResetAttack();
        }
    }

    private void CheckTreeRange()
    {
        // Cari semua Collider2D di sekitar Player
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            attackRange
        );

        bool treeInRange = false;

        foreach (Collider2D hit in hits)
        {
            CutTree tree = hit.GetComponentInParent<CutTree>();

            if (tree != null)
            {
                treeInRange = true;
                break;
            }
        }

        // Nyalakan/matikan info
        if (infoLogging != null)
        {
            infoLogging.SetActive(treeInRange);
        }
    }

    private void Attack()
    {
        if (Mouse.current == null)
            return;

        if (mainCamera == null)
        {
            mainCamera = Camera.main;

            if (mainCamera == null)
                return;
        }

        Vector2 mousePosition = Mouse.current.position.ReadValue();

        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);

        Collider2D hit = Physics2D.OverlapPoint(worldPosition);

        if (hit == null)
            return;

        CutTree tree = hit.GetComponentInParent<CutTree>();

        if (tree == null)
            return;

        float distance = Vector2.Distance(
            transform.position,
            tree.transform.position
        );

        if (distance > attackRange)
        {
            Debug.Log("Tree is too far away!");
            return;
        }

        // Player benar-benar menebang pohon
        tree.TakeDamage();

        // Camera shake
        inputManager.PlayHitShake();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(
            transform.position,
            attackRange
        );
    }
}
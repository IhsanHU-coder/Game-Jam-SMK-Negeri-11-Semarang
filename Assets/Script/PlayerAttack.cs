using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 2.5f;

    private InputManager inputManager;
    private Camera mainCamera;

    private void Awake()
    {
        inputManager = GetComponent<InputManager>();
        mainCamera = Camera.main;
    }

    void Update()
    {
        Debug.Log("update");
        if (inputManager.AttackPressed)
        {
            Attack();
            Debug.Log("Attack");
            inputManager.ResetAttack();
        }
    }

    private void Attack()
    {
        // Ambil posisi mouse
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        // Ubah posisi mouse dari Screen Space ke World Space
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);

        // Cari Collider2D yang tepat di posisi mouse
        Collider2D hit = Physics2D.OverlapPoint(worldPosition);

        if (hit == null)
            return;

        // Cari CutTree, termasuk kalau Collider ada di child object
        CutTree tree = hit.GetComponentInParent<CutTree>();

        if (tree == null)
            return;

        // Cek jarak player dengan pohon
        float distance = Vector2.Distance(
            transform.position,
            tree.transform.position
        );

        if (distance <= attackRange)
        {
            tree.TakeDamage();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
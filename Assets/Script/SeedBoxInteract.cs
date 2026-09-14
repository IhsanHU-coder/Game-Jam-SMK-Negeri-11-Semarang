using UnityEngine;

[RequireComponent(typeof(InteractZone))]
public class SeedBoxInteractIcon : MonoBehaviour
{
    [Tooltip("GameObject icon yang muncul/hilang di atas kotak biji ini")]
    public GameObject iconObject;

    private InteractZone interactZone;

    private void Awake()
    {
        interactZone = GetComponent<InteractZone>();
    }

    private void Start()
    {
        if (iconObject != null) iconObject.SetActive(false);
    }

    private void Update()
    {
        if (iconObject == null || interactZone == null) return;

        bool shouldShow = interactZone.IsPlayerInRange;

        if (iconObject.activeSelf != shouldShow)
        {
            iconObject.SetActive(shouldShow);
        }
    }
}
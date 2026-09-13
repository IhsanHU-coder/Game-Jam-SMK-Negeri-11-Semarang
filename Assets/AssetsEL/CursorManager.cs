using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance;

    [SerializeField] private Texture2D cursorTexture;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SetCustomCursor();
    }

    private void SetCustomCursor()
    {
        Cursor.SetCursor(
            cursorTexture,
            new Vector2(2, 2), // hotspot
            CursorMode.Auto
        );
    }
}
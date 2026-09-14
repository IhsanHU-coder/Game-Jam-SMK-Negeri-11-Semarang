using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Text;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class CreditsScroller : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [System.Serializable]
    public class CreditEntry
    {
        public string role;
        [TextArea] public string[] names;
    }

    [Header("Title")]
    [SerializeField] private string title = "Keturon";
    [SerializeField] private int titleSizePercent = 250;

    [Header("Credit Data")]
    [SerializeField] private List<CreditEntry> creditEntries = new List<CreditEntry>();

    [Header("Spacing")]
    [SerializeField] private int spaceRoleToNames = 0;
    [SerializeField] private int spaceBetweenGroups = 2;
    [SerializeField] private int spaceBetweenNames = 0;

    [Header("References")]
    [SerializeField] private TMP_Text creditsText;
    [SerializeField] private RectTransform textRect;
    [SerializeField] private RectTransform panel;

    [Header("Manual Size Settings")]
    [SerializeField] private float textWidth = 1200f;
    [SerializeField] private float extraPaddingStart = 0f;
    [SerializeField] private float extraPaddingEnd = 0f;

    [Header("Scroll Settings")]
    [SerializeField] private float scrollSpeed = 60f;
    [SerializeField] private bool useUnscaledTime = true;
    [SerializeField] private bool loop = false;

    [Header("Hold-to-FastForward")]
    [Tooltip("Kelipatan kecepatan scroll saat tombol/area ditahan.")]
    [SerializeField] private float fastForwardMultiplier = 3f;
    [Tooltip("Lama tekan (detik) sebelum dianggap 'hold' (fast-forward) bukan 'klik' (toggle pause).")]
    [SerializeField] private float holdThreshold = 0.2f;

    private float startY, endY;
    private bool running;
    private bool paused;
    private bool initialized;

    private bool isHolding;      // true selama pointer ditekan & sudah melewati holdThreshold
    private bool isPointerDown;  // true selama pointer masih ditekan (belum dilepas)
    private float pointerDownTime;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnEnable()
    {
        paused = false;
        initialized = false;
        running = false;
        isHolding = false;
        isPointerDown = false;

        textRect.anchoredPosition = new Vector2(textRect.anchoredPosition.x, -9999f);

        BuildText();
        StartCoroutine(InitPosition());
    }

    private void BuildText()
    {
        StringBuilder sb = new StringBuilder();

        if (!string.IsNullOrEmpty(title))
        {
            sb.AppendLine($"<size={titleSizePercent}%><b>{title}</b></size>");
            sb.AppendLine();
            sb.AppendLine();
        }

        for (int e = 0; e < creditEntries.Count; e++)
        {
            var entry = creditEntries[e];

            sb.AppendLine($"<size=140%><b>{entry.role}</b></size>");

            for (int i = 0; i < spaceRoleToNames; i++)
                sb.AppendLine();

            for (int n = 0; n < entry.names.Length; n++)
            {
                sb.AppendLine(entry.names[n]);

                if (n < entry.names.Length - 1)
                {
                    for (int i = 0; i < spaceBetweenNames; i++)
                        sb.AppendLine();
                }
            }

            if (e < creditEntries.Count - 1)
            {
                for (int i = 0; i < spaceBetweenGroups; i++)
                    sb.AppendLine();
            }
        }

        creditsText.text = sb.ToString();
    }

    private System.Collections.IEnumerator InitPosition()
    {
        yield return null;
        yield return null;

        creditsText.ForceMeshUpdate();

        textRect.sizeDelta = new Vector2(textWidth, textRect.sizeDelta.y);

        Vector2 preferredSize = creditsText.GetPreferredValues(textWidth, 0f);
        float textHeight = preferredSize.y;

        textRect.sizeDelta = new Vector2(textWidth, textHeight);

        float panelHeight = panel.rect.height;

        startY = -textHeight - extraPaddingStart;
        endY = panelHeight + extraPaddingEnd;

        textRect.anchoredPosition = new Vector2(textRect.anchoredPosition.x, startY);
        running = true;
        initialized = true;
    }

    private void Update()
    {
        // Deteksi apakah tekanan sudah cukup lama untuk dianggap "hold"
        if (isPointerDown && !isHolding)
        {
            float heldTime = (useUnscaledTime ? Time.unscaledTime : Time.time) - pointerDownTime;
            if (heldTime >= holdThreshold)
                isHolding = true;
        }

        if (!running) return;

        // Selama hold, paksa tetap jalan (abaikan status paused) supaya fast-forward selalu bisa dipakai
        if (!isHolding && paused) return;

        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        float speed = scrollSpeed * (isHolding ? fastForwardMultiplier : 1f);

        Vector2 pos = textRect.anchoredPosition;
        pos.y += speed * dt;
        textRect.anchoredPosition = pos;

        if (pos.y >= endY)
        {
            if (loop)
                textRect.anchoredPosition = new Vector2(pos.x, startY);
            else
            {
                running = false;
                EndCredits();
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!initialized) return;
        isPointerDown = true;
        pointerDownTime = useUnscaledTime ? Time.unscaledTime : Time.time;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!initialized) return;

        // Kalau BELUM sempat jadi "hold" (dilepas cepat), berarti ini klik biasa -> toggle pause
        if (isPointerDown && !isHolding)
        {
            paused = !paused;
        }

        isPointerDown = false;
        isHolding = false;
    }

    public void TogglePause()
    {
        if (!initialized) return;
        paused = !paused;
    }

    private void EndCredits()
    {
        if (MainMenuManager.Instance != null)
            MainMenuManager.Instance.CloseCredits();
    }

    public void BackToMenu()
    {
        running = false;
        paused = false;
        isHolding = false;
        isPointerDown = false;
        EndCredits();
    }

    public void BackToMainMenu()
    {
        EndCredits();
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }
}
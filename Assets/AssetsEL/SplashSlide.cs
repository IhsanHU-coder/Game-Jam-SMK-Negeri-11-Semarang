using UnityEngine;

[System.Serializable]
public class SplashSlide
{
    [Tooltip("CanvasGroup untuk LOGO saja (child dari PanelBackground), bukan background")]
    public CanvasGroup logoCanvasGroup;

    public float fadeInDuration = 0.5f;
    public float holdDuration = 1.2f;
    public float fadeOutDuration = 0.5f;
}
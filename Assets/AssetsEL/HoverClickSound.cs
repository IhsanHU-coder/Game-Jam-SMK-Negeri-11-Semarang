using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class HoverClickSound : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("Sound ID (harus sama persis dengan ID di AudioManager)")]
    [SerializeField] private string hoverSoundId = "ButtonHover";
    [SerializeField] private string clickSoundId = "ButtonClick";

    public UnityEvent OnHover;
    public UnityEvent OnClick;

    public void OnPointerEnter(PointerEventData eventData)
    {
        AudioManager.Instance.PlaySFX(hoverSoundId);
        OnHover?.Invoke();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        AudioManager.Instance.PlaySFX(clickSoundId);
        OnClick?.Invoke();
    }
}
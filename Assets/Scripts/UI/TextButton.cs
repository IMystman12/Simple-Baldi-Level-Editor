using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class TextButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public TMP_Text text;
    public UnityEvent onClick;
    private void Reset() => text = GetComponentInChildren<TMP_Text>();
    public void OnPointerEnter(PointerEventData eventData) => text.fontStyle = FontStyles.Underline;
    public void OnPointerExit(PointerEventData eventData) => OnDisable();
    void OnDisable() => text.fontStyle = FontStyles.Normal;
    public void OnPointerClick(PointerEventData eventData) => onClick?.Invoke();
}

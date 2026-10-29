using UnityEngine;
using UnityEngine.EventSystems;

public class TouchPosition : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        if (InputManager.pointerEvent == null)
        {
            InputManager.pointerEvent = eventData;
            InputManager.Instance.phase = InputManager.ButtonPhase.Hold;
        }
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId == InputManager.pointerEvent.pointerId)
        {
            InputManager.Instance.phase = InputManager.ButtonPhase.Release;
            InputManager.pointerEvent = null;
        }
    }
}

using UnityEngine;
using UnityEngine.EventSystems;

public class TouchPosition : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    bool activated;
    int id;
    public static Vector3 point;
    private void Start()
    {
        if (!Input.touchSupported&&Application.isMobilePlatform)
        {
            Destroy(this);
        }
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (activated)
        {
            return;
        }
        activated = true;
        id = eventData.pointerId;
        point = eventData.position;
    }
    void Update()
    {
        if (!activated)
        {
            return;
        }
        foreach (var a in Input.touches)
        {
            if (a.fingerId == id)
            {
                point = a.position;
                return;
            }
        }
        activated = false;
    }
    public void OnPointerUp(PointerEventData eventData) => activated &= !(eventData.pointerId == id);
}

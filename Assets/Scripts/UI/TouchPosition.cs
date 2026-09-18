using UnityEngine;
using UnityEngine.EventSystems;

public class TouchPosition : MonoBehaviour, IPointerEnterHandler, IPointerUpHandler
{
    public bool activated;
    public int id;
    public static Vector3 point;
    private void Start()
    {
        if (!Application.isMobilePlatform)
        {
            Destroy(this);
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
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

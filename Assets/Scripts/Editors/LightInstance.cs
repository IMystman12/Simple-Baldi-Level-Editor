using UnityEngine;

public class LightInstance : MonoBehaviour, IClickable
{
    public Coordinate position;
    bool available => this != null && EditorPad.Instance.tool.state == null && LightmapEditor.Instance && !LightmapEditor.Instance.isActiveAndEnabled;
    public void Clicked()
    {
        if (available)
        {
            LightmapEditor.Instance.Open(this);
        }
    }
    public void OnHighlight()
    {
        if (available)
        {
            transform.localScale = Vector3.one * 3;
        }
    }
    public void OffHighlight()
    {
        if (available)
        {
            transform.localScale = Vector3.one;
        }
    }
}

using UnityEngine;

public class LightInstance : MonoBehaviour, IClickable
{
    public Coordinate position;
    bool available => this != null && EditorPad.Instance.tool.state == null && EditorPad.Instance.editorState.state != null && EditorPad.Instance.editorState.state.GetType() == typeof(Categories.Editor_Lightmap) && LightmapEditor.Instance && !LightmapEditor.Instance.isActiveAndEnabled;
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

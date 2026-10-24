using UnityEngine;

public class LightInstance : MonoBehaviour, IClickable
{
    public Coordinate position;
    public void Clicked() => LightmapEditor.Instance.Open(this);
    public void OnHighlight() => transform.localScale = Vector3.one * 3;
    public void OffHighlight() => transform.localScale = Vector3.one;
    public bool IsAvailable() => this != null && EditorPad.Instance.tool.state == null && EditorPad.Instance.editorState.state != null && EditorPad.Instance.editorState.state.GetType() == typeof(Categories.Editor_Lightmap) && !LightmapEditor.Instance.isActiveAndEnabled;
}

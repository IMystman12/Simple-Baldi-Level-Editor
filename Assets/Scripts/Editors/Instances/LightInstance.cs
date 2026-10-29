using UnityEngine;

public class LightInstance : MonoBehaviour, IClickable
{
    public Coordinate position;
    public void Clicked() => EditorPad.Instance.Open(EditorPad.EditorType.Lightmap, this);
    public void OnHighlight() => transform.localScale = Vector3.one * 3;
    public void OffHighlight() => transform.localScale = Vector3.one;
    public bool IsAvailable() => EditorPad.Instance.tool.state == null && EditorPad.Instance.editorState.state != null && EditorPad.Instance.editorState.state.GetType() == typeof(Categories.Editor_Lightmap) && EditorPad.SubEditorClosed;
    public bool IsBaseAvailable() => this != null;
}

using UnityEngine;

public class BalPosable : MonoBehaviour, IClickable
{
    public SpriteRenderer spriteRenderer;
    public MaterialController mc;
    public void SetColor(Color color)
    {
        mc.OpenModifier();
        mc.mpb.SetColor(MaterialController.colorProperty, color);
        mc.CloseModifier();
    }
    public BalObject balObject = new BalObject();
    public void UpdateFromData()
    {
        transform.position = balObject.position;
        transform.rotation = Quaternion.Euler(0, 0, balObject.rotation);
    }
    public void Clicked() => EditorPad.Instance.Open(EditorPad.EditorType.BalObject, this);
    public void OffHighlight() => spriteRenderer.transform.localScale = Vector3.one;
    public void OnHighlight() => spriteRenderer.transform.localScale = Vector3.one * 3;
    public bool IsAvailable() => EditorPad.Instance.tool.state == null && EditorPad.Instance.editorState.state != null && EditorPad.Instance.editorState.state.GetType() == typeof(Categories.Editor_BaldiMan);
    public bool IsBaseAvailable() => spriteRenderer;
}

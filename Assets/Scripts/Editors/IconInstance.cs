using System.Linq;
using UnityEngine;

public class IconInstance : MonoBehaviour, IClickable
{
    public Icon icon;
    public SpriteRenderer spriteRenderer;
    private void Awake()
    {
        icon = new Icon()
        {
            position = transform.position,
            rotation = transform.eulerAngles.z,
            color = spriteRenderer.color,
            spriteName = spriteRenderer.sprite.name,
        };
        UpdateFromData();
    }
    public void UpdateFromData()
    {
        if (spriteRenderer)
        {
            spriteRenderer.color = icon.color;
            spriteRenderer.sprite = SpriteSelector.Instance.icons.FirstOrDefault(a => a.name == icon.spriteName) ?? spriteRenderer.sprite;
            transform.position = icon.position;
            transform.eulerAngles = Vector3.forward * icon.rotation;
        }
    }
    public void Clicked() => IconEditor.Instance.Open(this);
    public void OffHighlight() => spriteRenderer.transform.localScale = Vector3.one;
    public void OnHighlight() => spriteRenderer.transform.localScale = Vector3.one * 3;
    public bool IsAvailable() => spriteRenderer && EditorPad.Instance.tool.state == null && EditorPad.Instance.editorState.state != null && EditorPad.Instance.editorState.state.GetType() == typeof(Categories.Editor_Icon) && !IconEditor.Instance.isActiveAndEnabled;
}


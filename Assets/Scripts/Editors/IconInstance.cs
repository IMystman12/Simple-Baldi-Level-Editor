using System.Linq;
using UnityEngine;

public class IconInstance : MonoBehaviour, IClickable
{
    public Icon icon;
    public SpriteRenderer spriteRenderer;
    public IconMode iconMode;
    public enum IconMode
    {
        None,
        Notebook,
        Exit
    }
    bool available => EditorPad.Instance.tool.state == null && EditorPad.Instance.editorState.state != null && isIcon;
    bool isIcon => EditorPad.Instance.editorState.state.GetType() == typeof(Categories.Editor_Icon) && IconEditor.Instance && !IconEditor.Instance.isActiveAndEnabled;
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

    public void Clicked()
    {
        if (available)
        {
            if (isIcon)
            {
                IconEditor.Instance.Open(this);
            }
        }
    }

    public void OffHighlight()
    {
        if (available)
        {
            if (spriteRenderer)
            {
                spriteRenderer.transform.localScale = Vector3.one;
            }
        }
    }

    public void OnHighlight()
    {
        if (available)
        {
            if (spriteRenderer)
            {
                spriteRenderer.transform.localScale = Vector3.one * 3;
            }
        }
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
}

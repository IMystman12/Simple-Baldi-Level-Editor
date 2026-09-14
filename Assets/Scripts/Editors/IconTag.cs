using System.Linq;
using UnityEngine;

public class IconTag : MonoBehaviour, IClickable
{
    public Icon icon;
    public SpriteRenderer spriteRenderer;
    bool available => EditorPad.Instance.tool.state == null && IconEditor.Instance && !IconEditor.Instance.isActiveAndEnabled;
    private void Awake()
    {
        icon = new Icon()
        {
            spriteName = spriteRenderer.sprite.name,
            position = transform.position,
            rotation = transform.eulerAngles.z
        };
        UpdateFromData();
    }

    public void Clicked()
    {
        if (available)
        {
            IconEditor.Instance.Open(this);
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

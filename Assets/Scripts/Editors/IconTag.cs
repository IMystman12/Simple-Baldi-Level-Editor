using UnityEngine;

public class IconTag : MonoBehaviour, IClickable
{
    public Icon icon;
    public SpriteRenderer spriteRenderer;
    public bool destroyable => EditorPad.Instance.tool.state == null && EditorPad.Instance.removal;

    private void Awake()
    {
        icon = new Icon();
    }

    public void Clicked()
    {
        if (destroyable)
        {
            EditorPad.Instance.DestroyIcon(this);
        }
        else
        {

        }
    }

    public void OffHighlight()
    {
        if (spriteRenderer)
        {
            if (gameObject)
            {
                Categories.Editor_Icon.stop = false;
            }
            spriteRenderer.transform.localScale = Vector3.one;
            if (destroyable)
            {
                spriteRenderer.color = icon.color;
            }
        }
    }

    public void OnHighlight()
    {
        if (spriteRenderer)
        {
            spriteRenderer.transform.localScale = Vector3.one * 3;
            if (destroyable)
            {
                spriteRenderer.color = Color.red;
            }
            if (gameObject)
            {
                Categories.Editor_Icon.stop = true;
            }
        }
    }
}

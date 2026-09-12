using System.Linq;
using UnityEngine;

public class DoorTag : MonoBehaviour, IClickable
{
    public Door door;
    public SpriteRenderer spriteRenderer;
    void Start() => UpdateFromData();
    public void UpdateFromData()
    {
        if (spriteRenderer)
        {
            spriteRenderer.color = EditorPad.Instance.ec.CellFromPosition(door.position).room.color;
            spriteRenderer.sprite = EditorPad.Instance.sprites.FirstOrDefault(a => a.name == door.spriteName) ?? spriteRenderer.sprite;
        }
    }

    public void Clicked()
    {
    }

    public void OffHighlight()
    {
    }

    public void OnHighlight()
    {
    }
}

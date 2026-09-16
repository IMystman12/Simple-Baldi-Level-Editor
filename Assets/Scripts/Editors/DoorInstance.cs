using System.Linq;
using UnityEngine;

public class DoorInstance : MonoBehaviour
{
    public Door door;
    public SpriteRenderer spriteRenderer;
    public void UpdateFromData()
    {
        if (spriteRenderer)
        {
            spriteRenderer.color = EditorPad.Instance.ec.CellFromPosition(door.position).room.color;
            spriteRenderer.sprite = SpriteSelector.Instance.doors.FirstOrDefault(a => a.name == door.spriteName) ?? spriteRenderer.sprite;
        }
    }
}

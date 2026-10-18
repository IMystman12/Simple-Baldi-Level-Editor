using System.Linq;
using UnityEngine;

public class DoorInstance : MonoBehaviour
{
    public Door door;
    public SpriteRenderer spriteRenderer;
    public MaterialController mc;
    public void UpdateFromData()
    {
        if (spriteRenderer)
        {
            mc.OpenModifier();
            mc.mpb.SetColor("_Color", EditorPad.Instance.ec.CellFromPosition(door.position).room.color);
            mc.CloseModifier();
            spriteRenderer.sprite = SpriteSelector.Instance.doors.FirstOrDefault(a => a.name == door.spriteName) ?? spriteRenderer.sprite;
        }
    }
}

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
            var cell = EditorPad.Instance.ec.CellFromPosition(door.position);
            if (cell)
            {
                mc.mpb.SetColor(MaterialController.colorProperty, cell.room.color);
            }
            mc.CloseModifier();
            spriteRenderer.sprite = SpriteSelector.Instance.doors.FirstOrDefault(a => a.name == door.spriteName) ?? spriteRenderer.sprite;
        }
    }
}

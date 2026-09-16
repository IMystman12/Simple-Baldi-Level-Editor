using System.Linq;
using UnityEngine;

public class CellInstance : MonoBehaviour
{
    public Room room;
    public Cell data;
    public SpriteRenderer rendererBase, rendererBG;
    MaterialPropertyBlock materialPropertyBlock;
    public void ChangeColor()
    {
        if (rendererBase)
        {
            rendererBase.color = room.color;
        }

        if (rendererBG)
        {
            if (materialPropertyBlock == null)
            {
                materialPropertyBlock = new MaterialPropertyBlock();
            }
            rendererBG.GetPropertyBlock(materialPropertyBlock);
            materialPropertyBlock.Clear();
            rendererBG.SetPropertyBlock(materialPropertyBlock);

            var color = room.color;
            color.a = 0.25f;
            materialPropertyBlock.SetColor("_Color", color);


            if (SpriteSelector.Instance)
            {
                var sprite = SpriteSelector.Instance.rooms.FirstOrDefault(a => a.name == room.mapBGName);
                if (sprite)
                {
                    materialPropertyBlock.SetTexture("_BgTex", sprite.texture);
                }
            }

            rendererBG.SetPropertyBlock(materialPropertyBlock);
        }
    }
}


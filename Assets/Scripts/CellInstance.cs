using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CellInstance : MonoBehaviour
{
    public Room room;
    public Cell data;
    public SpriteRenderer rendererBase, rendererBG;
    MaterialPropertyBlock materialPropertyBlock;

    public int cost;
    public CellInstance parent;
    public List<LineRenderer> cables = new List<LineRenderer>();

    public EnvironmentController ec;
    public bool hasLight;
    public Light light;
    public List<CellInstance> cellsWithLight = new List<CellInstance>();
    public List<LightWithDistance> lights = new List<LightWithDistance>();
    public void CalculateLight()
    {
        Color c = Color.black;
        switch (ec.lightCalculateMode)
        {
            case LightCalculateMode.Cumulative:
                foreach (var l in lights)
                {
                    c += l.light.color * Mathf.Clamp01(1 - Mathf.Clamp01(l.distance / (float)l.light.strength)) * (Color.white - c);
                }
                break;
            case LightCalculateMode.Additive:
                foreach (var l in lights)
                {
                    c += l.light.color * Mathf.Clamp01(1 - Mathf.Clamp01(l.distance / (float)l.light.strength));
                }
                break;
            case LightCalculateMode.Greatest:
                foreach (var l in lights)
                {
                    c = Vector4.Max(c, l.light.color * Mathf.Clamp01(1 - Mathf.Clamp01(l.distance / (float)l.light.strength)));
                }
                break;
        }

        Color col = Color.white;
        for (int i = 0; i < 3; i++)
        {
            col[i] = Mathf.Lerp(ec.standardDark[i], 1f, c[i]);
        }
        col[3] = 1;
        ec.lightmap.SetPixel(data.position.x, data.position.z, col);
    }

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
public class LightWithDistance
{
    public Light light;
    public int distance;
}
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CellInstance : MonoBehaviour
{
    public Room room;
    public Cell data;
    public SpriteRenderer rendererBase, rendererBG;

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

    public MaterialController mc_Base, mc_BG;
    public void ChangeColor()
    {
        var color = room.color;
        if (rendererBase)
        {
            mc_Base.OpenModifier();
            mc_Base.mpb.SetColor(MaterialController.colorProperty, color);
            mc_Base.CloseModifier();
        }

        if (rendererBG)
        {
            mc_BG.OpenModifier();

            color.a = 0.25f;
            mc_BG.mpb.SetColor(MaterialController.colorProperty, color);


            if (SpriteSelector.Instance)
            {
                var sprite = SpriteSelector.Instance.rooms.FirstOrDefault(a => a.name == room.mapBGName);
                if (sprite)
                {
                    mc_BG.mpb.SetTexture("_BgTex", sprite.texture);
                }
            }

            mc_BG.CloseModifier();
        }
    }
    public Transform[] colliders = new Transform[4];
    public void UpdateCollider()
    {
        for (int i = 0; i < 4; i++)
        {
            colliders[i].gameObject.SetActive(true);
        }
        var list = TowardsExtension.OpenTowardsFromBin(data.id);
        for (int i = 0; i < 4; i++)
        {
            if (list.Contains((Towards)i))
            {
                colliders[i].gameObject.SetActive(false);
            }
        }
    }
}
public class LightWithDistance
{
    public Light light;
    public int distance;
}
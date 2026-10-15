using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialController : MonoBehaviour
{
    public static List<MaterialController> all = new List<MaterialController>();
    Renderer renderer;
    public MaterialPropertyBlock mpb;
    public static Texture2D lightmap;
    void Awake()
    {
        mpb = new MaterialPropertyBlock();
        renderer = GetComponent<Renderer>();
        all.Add(this);
        UpdateLightmap();
    }

    public void UpdateLightmap()
    {
        OpenModifier();
        mpb.SetTexture("_LightMap", lightmap);
        CloseModifier();
    }

    void OnDestroy()
    {
        all.Remove(this);
    }

    public void OpenModifier() => renderer.GetPropertyBlock(mpb);
    public void CloseModifier() => renderer.SetPropertyBlock(mpb);
}

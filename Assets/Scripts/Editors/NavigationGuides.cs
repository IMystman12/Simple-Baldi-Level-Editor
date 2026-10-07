using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class NavigationGuides : Singleton<NavigationGuides>
{
    public LineRenderer cablePref;
    public List<Path> paths = new List<Path>();
    public EnvironmentController ec;
    void Awake()
    {
        hue = Random.value;
        materialPropertyBlock = new MaterialPropertyBlock();
    }
    public void Show(bool val)
    {
        if (ec != EditorPad.Instance.ec)
        {
            while (paths.Count > 0)
            {
                paths[0].Dispose();
                paths.RemoveAt(0);
            }
            ec = EditorPad.Instance.ec;
        }
        gameObject.SetActive(val);
    }
    float hue;
    MaterialPropertyBlock materialPropertyBlock;
    public void MakePath(Coordinate a, Coordinate b)
    {
        var list = ec.FindPath(a, b);
        if (list.Count == 0)
        {
            return;
        }

        var r = Instantiate(cablePref, transform);
        hue = (hue + 0.61803398875f) % 1f;
        var col = Color.HSVToRGB(hue, (Random.value * 0.5f) + 0.5f, (Random.value * 0.5f) + 0.5f);
        r.GetPropertyBlock(materialPropertyBlock);
        materialPropertyBlock.SetColor("_Color", col);
        r.SetPropertyBlock(materialPropertyBlock);

        var path = new Path()
        {
            start = a,
            end = b,
            renderer = r,
            cells = list
        };
        paths.Add(path);

        var l = new List<Vector3>();
        for (int i = 0; i < list.Count; i++)
        {
            l.Add(list[i].transform.position);
        }
        r.positionCount = l.Count;
        r.SetPositions(l.ToArray());
    }
    public void RemovePath(Coordinate a, Coordinate b)
    {
        var path = paths.FirstOrDefault(c => c.start == a && c.end == b);
        if (path != null)
        {
            paths.Remove(path);
            path.Dispose();
        }
    }
    public class Path : IDisposable
    {
        public Coordinate start, end;
        public LineRenderer renderer;
        public List<CellInstance> cells = new List<CellInstance>();
        public void Dispose() => Destroy(renderer.gameObject);
    }
}

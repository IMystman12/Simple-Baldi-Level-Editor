using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class EditorPadOld : EditorPad
{
    [Header("General")]
    public EnvironmentController ec;
    Room room;
    [SerializeField] private bool delete;

    public Grid gridMan;
    
    void Start()
    {
        UpdateSize();

    }


    public void ChangeDelete(Toggle toggle)
    {
        delete = toggle.isOn;
    }


    [Header("Cell Override")]
    private bool overrideCellId;
    private int newId;
    [SerializeField] private Toggle[] cellOverrideToggle;
    public void CheckToggle()
    {
        overrideCellId = false;
        for (int i = 0; i < cellOverrideToggle.Length; i++)
        {
            if (cellOverrideToggle[i].isOn)
            {
                overrideCellId = true;
                newId = i;
            }
        }
    }

    [SerializeField] private RenderTexture mapTex;
    [SerializeField] private Camera mapRender;
    public void UpdateSize()
    {
        int a = 256, b = 256;
        ec.ReSize(new IntVector2(a, b));
        mapTex.Release();
        mapTex.width = a * 16;
        mapTex.height = b * 16;
        mapTex.Create();
        mapRender.transform.position = (Vector2)ec.size / 2;
        Vector3 vector = mapRender.transform.position;
        vector.z = -1;
        vector.y -= 0.5f;
        vector.x -= 0.5f;
        mapRender.transform.position = vector;
        mapRender.orthographicSize = b / 2;
    }
    public void SaveMap()
    {
        if (Path.IsPathRooted("mapPath.text"))
        {
            Debug.Log("Map Exported!");
            Texture2D tex = new Texture2D(mapTex.width, mapTex.height, TextureFormat.RGBA32, false, false);
            RenderTexture.active = mapTex;
            tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
            tex.Apply(false, false);
            tex.filterMode = FilterMode.Point;
            //        File.WriteAllBytes(mapPath.text, tex.EncodeToPNG());
        }
    }
    public void ExportAsset()
    {
        //    if (Path.IsPathRooted(assetPath.text))
        {
            //    File.WriteAllText(assetPath.text, JsonUtility.ToJson(new LevelAsset() { size = ec.size, rooms = ec.rooms.ToArray() }, true));
        }
    }
    public void ImportAsset()
    {
        //    if (Path.IsPathRooted(assetPath.text) && File.Exists(assetPath.text))
        {
            LevelAsset asset = JsonUtility.FromJson<LevelAsset>(File.ReadAllText("assetPath.text"));
            //      sizeX.text = asset.size.x.ToString();
            //    sizeZ.text = asset.size.z.ToString();
            UpdateSize();
            while (ec.rooms.Count != 0)
            {
                ec.DestroyRoom(ec.rooms[0]);
            }
            foreach (var item in asset.rooms)
            {
                room = ec.CreateRoom(item.color);
                foreach (var item0 in item.cells)
                {
                    ec.CreateCell(item0.position, room, item0.id);
                }
            }
        }
    }
}
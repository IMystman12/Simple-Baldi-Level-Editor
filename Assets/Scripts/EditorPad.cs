
using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EditorPad : MonoBehaviour
{
    [Header("General")]
    public EnvironmentController ec;
    [SerializeField] private Camera cameraBase;
    [SerializeField] private float moveSentivity = 10, scaleSentivity = 10;
    Vector2 pos;
    float val;
    Room room;
    [SerializeField] private bool delete;
    public bool inArea;
    [SerializeField] private GameObject[] categories = new GameObject[4];
    int index;
    public IntVector2 cursorGridPos => ec.GetGridPosition(cameraBase.ScreenToWorldPoint(Input.mousePosition));
    public Vector2 cursorPos => cameraBase.ScreenToWorldPoint(Input.mousePosition);
    [SerializeField] private EditorStateBase stateBase;
    void Start()
    {
        CreateRoom();
        ChangeMode(Mode.Single);
        for (int i = 0; i < cellOverrideToggle.Length; i++)
        {
            cellOverrideToggle[i].GetComponentInChildren<Image>().sprite = ec.cellSprites[i];
        }
        CheckToggle();
        CheckPath();
        UpdateSize();
    }

    void Update()
    {
        val = Time.fixedDeltaTime * moveSentivity;
        pos.x = Mathf.Clamp(pos.x + Input.GetAxis("Horizontal") * val, 0, ec.realSize.x);
        pos.y = Mathf.Clamp(pos.y + Input.GetAxis("Vertical") * val, 0, ec.realSize.x);
        cameraBase.transform.position = new Vector3(Mathf.Round(pos.x), Mathf.Round(pos.y), Mathf.Round(transform.position.z));
        cameraBase.orthographicSize = Mathf.Clamp(cameraBase.orthographicSize + Input.GetAxis("Mouse ScrollWheel") * scaleSentivity, 0.1f, 64);

        hit2D = Physics2D.Raycast(cameraBase.transform.position, cameraBase.transform.forward);
        currentMark = hit2D.transform == null ? null : hit2D.transform.GetComponent<Mark>();
        currentMark?.OnEnter();
        if (currentMark != previousMark)
        {
            previousMark?.OnExit();
            previousMark = currentMark;
        }

        stateBase?.Update();
        if (inArea)
        {
            if (Input.GetKey(KeyCode.Mouse0))
            {
                stateBase?.Down();
            }
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                stateBase?.Clicked();
            }
        }
    }
    public void InArea()
    {
        inArea = true;
    }

    public void OutArea()
    {
        inArea = false;
    }

    public void NextCategory()
    {
        categories[index].SetActive(false);
        index++;
        if (index == categories.Length)
        {
            index = 0;
        }
        categories[index].SetActive(true);
    }

    public void PreviousCategory()
    {
        categories[index].SetActive(false);
        index--;
        if (index == -1)
        {
            index = categories.Length - 1;
        }
        categories[index].SetActive(true);
    }

    public void ChangeDelete(Toggle toggle)
    {
        delete = toggle.isOn;
    }

    public void Change(EditorStateBase newStateBase)
    {
        stateBase?.Exit();
        stateBase = newStateBase;
        stateBase?.Enter(this);
    }

    public void ChangeMode(int i)
    {
        ChangeMode((Mode)i);
    }

    public void ChangeMode(Mode mode)
    {
        switch (mode)
        {
            case Mode.Single:
                Change(new EditorState_Single());
                break;
            case Mode.AreaSelect:
                Change(new EditorState_AreaSelect());
                break;
            case Mode.Icon:
                Change(new EditorState_MarkPlacing());
                break;
        }
    }

    public enum Mode
    {
        Single,
        AreaSelect,
        Icon
    }

    [Header("Room Mangae")]
    [SerializeField] private Image colorBG;
    [SerializeField] private Slider sliderR, sliderG, sliderB, sliderId;
    [SerializeField] private TMP_Text colorText, idText;
    Color color = Color.white;
    public void UpdateColor()
    {
        Debug.Log("Color Updated!");
        color.r = sliderR.value;
        color.g = sliderG.value;
        color.b = sliderB.value;
        colorBG.color = color;
        colorText.text = $"R:{Mathf.FloorToInt(color.r * 255)}\nG:{Mathf.FloorToInt(color.g * 255)}\nB:{Mathf.FloorToInt(color.b * 255)}";
        room.color = color;
        room.ChangeColor(ec);
    }
    public void ChangeRoom()
    {
        idText.text = ((int)sliderId.value).ToString();
        room = ec.rooms[(int)sliderId.value];
        color = room.color;
        sliderR.value = color.r;
        sliderG.value = color.g;
        sliderB.value = color.b;
        colorBG.color = color;
        colorText.text = $"R:{Mathf.FloorToInt(color.r * 255)}\nG:{Mathf.FloorToInt(color.g * 255)}\nB:{Mathf.FloorToInt(color.b * 255)}";
    }

    public void CreateRoom()
    {
        room = ec.CreateRoom(color);
        sliderId.value = ec.rooms.Count - 1;
        sliderId.maxValue = ec.rooms.Count - 1;
        idText.text = ((int)sliderId.value).ToString();
    }
    public void DestroyRoom()
    {
        if (ec.rooms.Count > 1)
        {
            ec.DestroyRoom(room);
            sliderId.value = ec.rooms.Count - 1;
            sliderId.maxValue = ec.rooms.Count - 1;
            idText.text = ((int)sliderId.value).ToString();
            ChangeRoom();
        }
    }

    [Header("Area Selecting")]
    public Transform viewPin, viewCellRange;
    public bool maze, hall;
    public void SetMaze(Toggle toggle)
    {
        maze = toggle.isOn;
    }
    public void SetHall(Toggle toggle)
    {
        hall = toggle.isOn;
    }
    public void SingleOperateForCell(IntVector2 intVector)
    {
        if (delete)
        {
            ec.DestroyCell(ec.CellFromPosition(intVector));
        }
        else
        {
            if (overrideCellId)
            {
                ec.CreateCell(intVector, room, newId).autoConnect = false;
            }
            else
            {
                ec.ConnectSurround(ec.CreateCell(intVector, room));
            }
        }
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

    [Header("Icon Placing")]
    [SerializeField] private SpriteRenderer iconRendererPref;
    [SerializeField] private Sprite[] marks = new Sprite[9];
    [SerializeField] private Toggle[] markToggle;
    [SerializeField] private Mark currentMark, previousMark;
    RaycastHit2D hit2D;
    public void ChangeSprite()
    {
        for (int i = 0; i < markToggle.Length; i++)
        {
            if (markToggle[i].isOn)
            {
                iconRendererPref.sprite = marks[i];
            }
        }
    }

    [Header("General Settings")]
    [SerializeField] private TMP_InputField assetPath, mapPath, sizeX, sizeZ;
    [SerializeField] private RenderTexture mapTex;
    [SerializeField] private Camera mapRender;
    void CheckPath()
    {
        if (!Path.IsPathRooted(assetPath.text))
        {
            assetPath.text = Path.Combine(Application.streamingAssetsPath, "Level.levelasset");
        }
        if (!Path.IsPathRooted(mapPath.text))
        {
            mapPath.text = Path.Combine(Application.streamingAssetsPath, "Map.png");
        }
    }
    public void UpdateSize()
    {
        int a = int.Parse(sizeX.text), b = int.Parse(sizeZ.text);
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
        if (Path.IsPathRooted(mapPath.text))
        {
            Debug.Log("Map Exported!");
            Texture2D tex = new Texture2D(mapTex.width, mapTex.height, TextureFormat.RGBA32, false, false);
            RenderTexture.active = mapTex;
            tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
            tex.Apply(false, false);
            tex.filterMode = FilterMode.Point;
            File.WriteAllBytes(mapPath.text, tex.EncodeToPNG());
        }
    }
    public void ExportAsset()
    {
        if (Path.IsPathRooted(assetPath.text))
        {
            File.WriteAllText(assetPath.text, JsonUtility.ToJson(new LevelAsset() { size = ec.size, rooms = ec.rooms.ToArray() }, true));
        }
    }
    public void ImportAsset()
    {
        if (Path.IsPathRooted(assetPath.text) && File.Exists(assetPath.text))
        {
            LevelAsset asset = JsonUtility.FromJson<LevelAsset>(File.ReadAllText(assetPath.text));
            sizeX.text = asset.size.x.ToString();
            sizeZ.text = asset.size.z.ToString();
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
public interface EditorStateBase

{
    void Enter(EditorPad pad);
    void Update();
    void Clicked();
    void Down();
    void Exit();
}
[Serializable]
public class LevelAsset
{
    public IntVector2 size;
    public Room[] rooms = new Room[0];
}
public class EditorState_Single : EditorStateBase
{
    EditorPad pad;
    public void Clicked()
    {
    }

    public void Down()
    {
        pad.SingleOperateForCell(pad.cursorGridPos);
    }

    public void Enter(EditorPad pad)
    {
        this.pad = pad;
    }

    public void Exit()
    {
    }

    public void Update()
    {
    }
}
public class EditorState_AreaSelect : EditorStateBase
{
    bool aSideActived;
    IntVector2 aSidePos;
    EditorPad pad;
    Vector2 vector, vector0;
    public void Clicked()
    {
        if (aSideActived)
        {
            Apply(new IntVector2(Mathf.Min(aSidePos.x, pad.cursorGridPos.x), Mathf.Min(aSidePos.z, pad.cursorGridPos.z)),
            new IntVector2(Mathf.Abs(aSidePos.x - pad.cursorGridPos.x), Mathf.Abs(aSidePos.z - pad.cursorGridPos.z)));
            aSideActived = false;
            pad.viewCellRange.gameObject.SetActive(false);
        }
        else
        {
            pad.viewCellRange.gameObject.SetActive(true);
            aSideActived = true;
            aSidePos = pad.cursorGridPos;
            aSidePos.x = Mathf.Clamp(aSidePos.x, 0, pad.ec.size.x);
            aSidePos.z = Mathf.Clamp(aSidePos.z, 0, pad.ec.size.z);
        }
    }

    public void Apply(IntVector2 pos, IntVector2 size)
    {
        if (pad.maze)
        {

        }
        else
        {
            IntVector2 a = pos + size;
            for (int i = pos.x; i != a.x; i++)
            {
                for (int j = pos.z; j != a.z; j++)
                {
                    pad.SingleOperateForCell(new IntVector2(i, j));
                }
            }
        }
    }

    public void Down()
    {
    }

    public void Enter(EditorPad pad)
    {
        this.pad = pad;
        pad.viewPin.gameObject.SetActive(true);
    }

    public void Exit()
    {
        pad.viewPin.gameObject.SetActive(false);
    }

    public void Update()
    {
        if (pad.inArea)
        {
            vector = pad.cursorPos;
            vector.x = Mathf.Clamp(vector.x, -0.5f, pad.ec.size.x - 0.5f);
            vector.y = Mathf.Clamp(vector.y, -0.5f, pad.ec.size.z - 0.5f);
            pad.viewPin.position = vector;

            if (aSideActived)
            {
                vector0 = vector - aSidePos;
                pad.viewCellRange.position = (vector + aSidePos) / 2;
                pad.viewCellRange.localScale = new Vector3(Mathf.Abs(vector0.x), Mathf.Abs(vector0.y), 1);
            }
        }
    }
}
public class EditorState_MarkPlacing : EditorStateBase
{
    EditorPad pad;
    public void Clicked()
    {
    }

    public void Down()
    {
    }

    public void Enter(EditorPad pad)
    {
        this.pad = pad;
    }

    public void Exit()
    {
    }

    public void Update()
    {
    }
}
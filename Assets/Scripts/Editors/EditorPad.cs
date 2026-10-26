using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;

public class EditorPad : Singleton<EditorPad>
{
    public EnvironmentController ecPref;
    public EnvironmentController ec;
    public void Initialize()
    {
        ec = Instantiate(ecPref);
        ec.Resize(new Coordinate(64, 64));
        ClearHistory();
    }
    public void Clear() => Destroy(ec.gameObject);

    public StateMachine editorState = new StateMachine();

    public Toolbar.ToolStateMachine tool = new Toolbar.ToolStateMachine();
    public TMP_Text positionTip;
    public Coordinate cursorGridPos;
    void GridPositionUpdate()
    {
        worldPoint = Camera.main.ScreenToWorldPoint(Input.touchSupported && Application.isMobilePlatform ? TouchPosition.point : Input.mousePosition);
        cursorGridPos = Coordinate.ConvertToGridCoordinate(worldPoint);
        cursorGridPos.x = Mathf.Clamp(cursorGridPos.x, 0, ec.realSize.x);
        cursorGridPos.z = Mathf.Clamp(cursorGridPos.z, 0, ec.realSize.z);
        positionTip.text = cursorGridPos.ToString();
    }
    public bool removal;

    [HideInInspector] public bool inArea;
    public void SetArea(bool val) => inArea = val;

    public bool pause { get; private set; }
    public int pauses;
    public Categories categories;
    public void Pause(bool val)
    {
        pauses += val ? 1 : -1;
        pause = pauses > 0;
        controls.SetActive(!val && Input.touchSupported && Application.isMobilePlatform);
        clkPrevious?.OffHighlight();
        categories?.Pause(val);
    }

    public static bool SubmitDelayed { get; set; }
    public static bool Submit { get; set; }
    public float movementX { get; set; }
    public float movementY { get; set; }
    public float zoom { get; set; }
    public GameObject controls;
    bool submitPrevious;

    [SerializeField] private float mobileZoom = 0.001f, mobileMovement = 2, scaleSensitivity = 10;
    float val;
    Vector2 pos;
    Vector3 worldPoint;
    void CameraUpdate()
    {
        val = Time.fixedDeltaTime * 10;
        pos.x = Mathf.Clamp(pos.x + Priority(Input.GetAxis("Horizontal"), movementX * mobileMovement) * val, 0, ec.realSize.x);
        pos.y = Mathf.Clamp(pos.y + Priority(Input.GetAxis("Vertical"), movementY * mobileMovement) * val, 0, ec.realSize.z);
        Camera.main.transform.position = new Vector3(pos.x, pos.y, Mathf.Round(transform.position.z));
        bg.position = new Vector3(Mathf.Floor(pos.x / 10) * 10 - 0.5f, Mathf.Floor(pos.y / 10) * 10 - 0.5f, 10);
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize + Priority(Input.GetAxis("Mouse ScrollWheel"), zoom * mobileZoom) * scaleSensitivity, 0.1f, 64);
    }
    float Priority(float a, float b) => Mathf.Abs(b) > Mathf.Abs(a) ? b : a;

    public LayerMask availableLayers;
    RaycastHit2D hit2D;
    public IClickable clkCurrent, clkPrevious, clkNew;
    void ClickUpdate()
    {
        if (!inArea)
        {
            clkNew = null;
        }
        else
        {
            hit2D = Physics2D.Raycast(worldPoint, Camera.main.transform.forward, 99, availableLayers);
            clkNew = hit2D.transform ? hit2D.transform.GetComponent<IClickable>() : null;
        }

        clkCurrent = (clkNew != null) ? (clkNew.IsAvailable() ? clkNew : null) : null;
        if (clkCurrent != clkPrevious)
        {
            clkPrevious?.OffHighlight();
            clkCurrent?.OnHighlight();
            clkPrevious = clkCurrent;
        }
        if (SubmitDelayed)
        {
            clkCurrent?.Clicked();
        }
    }
    #region "History Management"
    private List<HistoryAction> steps = new List<HistoryAction>();
    const int maxSteps = 256;
    //starts from -1 (non-step)
    int step = -1;
    public void Undo()
    {
        if (step > -1)
        {
            steps[step].Apply(true);
            step--;
        }
    }

    public void Redo()
    {
        if (step < (steps.Count - 1))
        {
            step++;
            steps[step].Apply(false);
        }
    }

    public void RecordStep(HistoryAction action)
    {
        action.Apply(false);
        int i = steps.Count - 1;
        if (step < i)
        {
            //s=-1 sc=1 s+1=0 c=0+1
            steps.RemoveRange(step + 1, i - step);
        }
        steps.Add(action);
        step++;
        if (steps.Count > 256)
        {
            steps.RemoveRange(0, steps.Count - maxSteps);
        }
    }
    public void ClearHistory()
    {
        steps.Clear();
        step = -1;
    }
    #endregion

    public Transform bg, cursorCross;
    void GuideUpdate() => cursorCross.position = (Vector3)Coordinate.ConvertToMapCoordinate(cursorGridPos) + Vector3.forward * 5;

    public void Quit()
    {
        Application.Quit();
        Debug.Log("Quit");
    }

    #region "SubEditor"
    public enum EditorType
    {
        Room,
        Icon,
        Lightmap,
        Setting,
        Save,
        Pos_Object,
        Pos_Entity,
    }
    public void Open(EditorType type, object arg = null)
    {
        if (subEditor.gameObject.activeSelf)
        {
            Close();
        }

        this.type = type;
        Pause(true);
        subEditor.gameObject.SetActive(true);
        for (int i = 0; i < list.Length; i++)
        {
            list[i].gameObject.SetActive(false);
        }
        list[(int)type].gameObject.SetActive(true);

        switch (type)
        {
            case EditorType.Room:
                var roomTag = (RoomInstance)arg;
                currentRoomTag = roomTag;
                roomTags.ForEach(a => a.OffHighlight());
                roomNameField.text = currentRoomTag.room.name;
                SetRoomName();
                roomColorField.color = currentRoomTag.room.color;
                roomColorField.ShowColor();

                destroyRoomButton.gameObject.SetActive(ec.rooms.Count > 1);

                SpriteSelector.Instance.Open(Categories.Category.Room);
                SpriteSelector.Instance.TryToSetValue(roomTag.room.mapBGName);
                SpriteSelector.Instance.onCheckValue.AddListener(() =>
                {
                    roomTag.room.mapBGName = SpriteSelector.Instance.Selection.name;
                    roomTag.room.ChangeColor(ec);
                });
                break;
            case EditorType.Setting:
                ShowLevelSize();
                break;
            case EditorType.Icon:
                currentIconTag = (IconInstance)arg;

                iconColorField.color = currentIconTag.icon.color;
                iconColorField.ShowColor();

                ShowIconPosition();
                ShowIconRotation();
                break;
            case EditorType.Lightmap:
                currentLight = (LightInstance)arg;
                light = ec.CellFromPosition(currentLight.position).light;

                colorFieldLight.color = light.color;
                colorFieldLight.ShowColor();

                colorFieldDark.color = ec.standardDark;
                colorFieldDark.ShowColor();

                strengthField.text = light.strength.ToString();
                calculateModeText.text = ec.lightCalculateMode.ToString();
                break;
            case EditorType.Save:
                if (string.IsNullOrWhiteSpace(levelName))
                {
                    levelName = $"Level_{DateTime.Now.ToBinary()}";
                }
                levelNameField.text = levelName;
                break;
        }
    }
    public void Close()
    {
        subEditor.gameObject.SetActive(false);
        Pause(false);
        switch (type)
        {
            case EditorType.Room:
                SpriteSelector.Instance.onCheckValue.RemoveAllListeners();
                SpriteSelector.Instance.Close();
                break;
            case EditorType.Lightmap:
                if (rebuildLight)
                {
                    DestroyLight(currentLight.position);
                    CreateLight(currentLight.position);
                }
                if (regenerateAfterClose)
                {
                    ec.RegenerateLight();
                }
                rebuildLight = false;
                regenerateAfterClose = false;
                break;
        }
    }
    [Header("SubEditor")]
    public EditorType type;
    public GameObject subEditor;
    public static bool SubEditorClosed => !Instance.subEditor.activeSelf;
    public GameObject[] list = new GameObject[0];
    public RenderTexture mapTex;
    public Camera mapRender;

    #region "Room"
    [Header("Room")]
    public RoomInstance roomTagPref;
    public Transform roomTagManager;
    [HideInInspector] public List<RoomInstance> roomTags = new List<RoomInstance>();
    [HideInInspector] public RoomInstance currentRoomTag;
    public GameObject destroyRoomButton;

    public void CreateRoom() => RecordStep(new HistoryAction_Room(new Room() { color = Color.white }, false));
    public void CreateBaseRoom() => new HistoryAction_Room(new Room() { color = Color.white }, false).Apply(false);
    public void DestroyRoom() => RecordStep(new HistoryAction_Room(currentRoomTag.room, true));

    public void UpdateRoomTags()
    {
        for (int i = 0; i < roomTags.Count;)
        {
            if (!ec.rooms.Contains(roomTags[i].room))
            {
                Destroy(roomTags[i].gameObject);
                roomTags.RemoveAt(i);
            }
            else
            {
                i++;
            }
        }

        RoomInstance tag;
        for (int i = 0; i < ec.rooms.Count; i++)
        {
            if (!roomTags.Any(a => a.room == ec.rooms[i]))
            {
                tag = Instantiate(roomTagPref, roomTagManager);
                tag.room = ec.rooms[i];
                tag.UpdateName();
                roomTags.Add(tag);
            }
        }

        RequeueTags();
    }
    void RequeueTags()
    {
        var k = Vector3.up * 1.5f;
        for (int i = 0; i < roomTags.Count; i++)
        {
            roomTags[i].transform.localPosition = k * i;
        }
    }

    public TMP_InputField roomNameField;
    public void SetRoomName()
    {
        if (!string.IsNullOrWhiteSpace(roomNameField.text))
        {
            currentRoomTag.room.name = roomNameField.text;
            currentRoomTag.UpdateName();
        }
        roomNameField.text = currentRoomTag.room.name;
    }


    public ColorField roomColorField;
    public void SetRoomColor()
    {
        currentRoomTag.room.color = roomColorField.color;
        currentRoomTag.room.ChangeColor(ec);
    }

    public class HistoryAction_Room : HistoryAction
    {
        Room backup;
        public HistoryAction_Room(Room room, bool invert)
        {
            this.invert = invert;
            backup = room;
        }
        protected override void Positive()
        {
            backup = Instance.ec.LoadRoom(backup);
            Instance.UpdateRoomTags();
            Instance.Open(EditorType.Room, Instance.roomTags[Instance.roomTags.Count - 1]);
        }
        protected override void Negative()
        {
            Instance.ec.DestroyRoom(backup);
            Instance.UpdateRoomTags();
            Instance.Close();
        }
    }
    #endregion
    #region "Icon"
    [Header("Icon")]
    public IconInstance iconPref;
    [HideInInspector] public List<IconInstance> icons = new List<IconInstance>();
    public IconInstance CreateIcon(Coordinate pos, string sprite)
    {
        var ico = Instantiate(iconPref, Coordinate.ConvertToMapCoordinate(pos), Quaternion.identity, ec.transform);
        ico.icon.spriteName = sprite ?? "Icon_Item";
        ico.UpdateFromData();
        ec.icons.Add(ico.icon);
        icons.Add(ico);
        return ico;
    }
    public Icon[] DestroyIcon(Coordinate pos)
    {
        IconInstance tag;
        List<Icon> result = new List<Icon>();
        for (int i = 0; i < icons.Count;)
        {
            tag = icons[i];
            if (Coordinate.ConvertToGridCoordinate(tag.transform.position) == pos)
            {
                DestroyIcon(tag);
                result.Add(tag.icon);
            }
            else
            {
                i++;
            }
        }
        return result.ToArray();
    }
    void DestroyIcon(IconInstance icon)
    {
        ec.icons.Remove(icon.icon);
        icons.Remove(icon);
        Destroy(icon.gameObject);
    }
    public void DestroyAllIcons()
    {
        for (; icons.Count > 0;)
        {
            DestroyIcon(icons[0]);
        }
    }

    [HideInInspector] public IconInstance currentIconTag;
    public TMP_InputField iconPositionField;
    public void SetIconPosition()
    {
        string[] array = iconPositionField.text.Split(',');
        if (array.Length < 2)
        {
            ShowIconPosition();
            return;
        }
        float val;
        Vector2 result = default;
        for (int i = 0; i < 2; i++)
        {
            if (float.TryParse(array[i], out val))
            {
                result[i] = val;
            }
            else
            {
                ShowIconPosition();
                return;
            }
        }
        currentIconTag.icon.position = result;
        currentIconTag.UpdateFromData();
    }
    public void ShowIconPosition() => iconPositionField.text = string.Join(",", currentIconTag.icon.position.x, currentIconTag.icon.position.y);

    public TMP_InputField iconRotationField;
    public void SetIconRotation()
    {
        float val;
        if (float.TryParse(iconRotationField.text, out val))
        {
            currentIconTag.icon.rotation = val;
            currentIconTag.UpdateFromData();
        }
    }
    public void ShowIconRotation() => iconRotationField.text = currentIconTag.icon.rotation.ToString();

    public ColorField iconColorField;
    public void SetIconColor()
    {
        currentIconTag.icon.color = iconColorField.color;
        currentIconTag.UpdateFromData();
    }
    #endregion
    #region "Lightmap"
    [Header("Lightmap")]
    public SpriteRenderer lightmapRenderer;
    [HideInInspector] public new Light light;

    public LightInstance lightPref;
    [HideInInspector] public List<LightInstance> lights = new List<LightInstance>();

    [HideInInspector] public LightInstance currentLight;

    public ColorField colorFieldLight, colorFieldDark;
    public TMP_InputField strengthField;
    public TMP_Text calculateModeText;

    bool regenerateAfterClose;
    bool rebuildLight;

    public void SetLightStrength()
    {
        int val;
        if (int.TryParse(strengthField.text, out val))
        {
            light.strength = val;
            rebuildLight = true;
        }
        else
        {
            strengthField.text = light.strength.ToString();
        }
    }

    public void SetLightColor()
    {
        light.color = colorFieldLight.color;
        rebuildLight = true;
    }

    public void SetDarkLightColor()
    {
        ec.standardDark = colorFieldDark.color;
        regenerateAfterClose = true;
    }

    public void SetLightCalculateMode()
    {
        ec.lightCalculateMode++;
        if (ec.lightCalculateMode == LightCalculateMode.EnumLength)
        {
            ec.lightCalculateMode = 0;
        }
        calculateModeText.text = ec.lightCalculateMode.ToString();
        regenerateAfterClose = true;
    }

    public void StartLightRender()
    {
        var lightmap = ec.lightmap;
        lightmapRenderer.sprite = Sprite.Create(lightmap, new Rect(0, 0, lightmap.width, lightmap.height), Vector2.zero, 1);
        lightmapRenderer.enabled = true;
        foreach (var a in lights)
        {
            a.gameObject.SetActive(true);
        }
    }
    public void CreateLight(Coordinate position)
    {
        var cell = ec.CellFromPosition(position);
        if (cell != null && !cell.hasLight)
        {
            var li = Instantiate(lightPref, Coordinate.ConvertToMapCoordinate(position), Quaternion.identity, ec.transform);
            li.position = cell.data.position;
            lights.Add(li);
            li.gameObject.SetActive(lightmapRenderer.enabled);
            ec.GenerateLight(position, light);
        }
    }
    public void DestroyLight(Coordinate position)
    {
        var cell = ec.CellFromPosition(position);
        if (cell != null)
        {
            var l = lights.FirstOrDefault(a => a.position == cell.data.position);
            lights.Remove(l);
            if (l)
            {
                Destroy(l.gameObject);
            }
            ec.RemoveLight(position);
        }
    }
    public void StopLightRender()
    {
        foreach (var a in lights)
        {
            a.gameObject.SetActive(false);
        }
        lightmapRenderer.enabled = false;
    }
    #endregion
    #region "Setting"
    [Header("Setting")]
    public TMP_InputField levelSizeField;
    public void SetLevelSize()
    {
        string[] array = levelSizeField.text.Split(',');
        if (array.Length < 2)
        {
            ShowLevelSize();
            return;
        }
        int val;
        Coordinate result = default;
        for (int i = 0; i < 2; i++)
        {
            if (int.TryParse(array[i], out val))
            {
                result[i] = Mathf.Clamp(val, 1, Mathf.Min(SystemInfo.maxTextureSize, 8192));
            }
            else
            {
                ShowLevelSize();
                return;
            }
        }
        ec.Resize(result);
        ShowLevelSize();
    }
    public void ShowLevelSize() => levelSizeField.text = string.Join(",", ec.size.x, ec.size.z);

    public void UpdateLevelSize(Coordinate newSize)
    {
        mapTex.Release();
        mapTex.width = newSize.x * 16;
        mapTex.height = newSize.z * 16;
        mapTex.Create();

        Vector3 vector = new Vector3(newSize.x / 2 - 0.5f, newSize.z / 2 - 0.5f, -1);
        mapRender.transform.position = vector;
        mapRender.aspect = (float)newSize.x / newSize.z;
        mapRender.orthographicSize = newSize.z / 2f;
    }

    public GameObject guide;
    public void ToggleBuildGuide() => guide.SetActive(!guide.activeSelf);

    public void ToggleToSetLight()
    {
        ec.lightAffected = !ec.lightAffected;
        ec.UpdateLights();
    }
    #endregion
    #region  "Save"
    [Header("Save")]
    public string levelName;
    public TMP_InputField levelNameField;
    public void SetLevelName()
    {
        if (!string.IsNullOrWhiteSpace(levelNameField.text))
        {
            levelName = levelNameField.text;
        }
    }

    string mapTexturePath => Path.Combine(Application.persistentDataPath, $"Map_{levelName}.png");
    string lightmapTexturePath => Path.Combine(Application.persistentDataPath, $"Lightmap_{levelName}.png");
    string mapAssetPath => Path.Combine(Application.persistentDataPath, levelName + ".mapAsset");
    public void SaveMapAndLightmap()
    {
        Shader.DisableKeyword("_BG_REQUIRED");

        mapRender.enabled = true;
        mapRender.Render();
        mapRender.enabled = false;

        Texture2D tex = new Texture2D(mapTex.width, mapTex.height, TextureFormat.RGBA32, false, false);
        RenderTexture.active = mapTex;
        tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
        tex.Apply(false, false);
        tex.filterMode = FilterMode.Point;
        Shader.EnableKeyword("_BG_REQUIRED");

        File.WriteAllBytes(mapTexturePath, tex.EncodeToPNG());

        File.WriteAllBytes(lightmapTexturePath, ec.lightmap.EncodeToPNG());

        Application.OpenURL(Application.persistentDataPath);
    }
    public void SaveAsset()
    {
        File.WriteAllText(mapAssetPath, JsonUtility.ToJson(ec.ConvertToAsset, true));
        Application.OpenURL(Application.persistentDataPath);
    }

    public void LoadAsset()
    {
        Clear();
        Initialize();
        if (File.Exists(mapAssetPath))
        {
            Debug.Log($"{levelName} was founded! Loading");
            var data = JsonUtility.FromJson<LevelAsset>(File.ReadAllText(mapAssetPath));

            if (SpriteSelector.Instance)
            {
                for (int i = 0; i < data.extraSprites.Length; i++)
                {
                    SpriteSelector.Instance.ImportExtraSprite(data.extraSprites[i]);
                }
            }

            ec.Build(data);
            UpdateRoomTags();
            Open(EditorType.Room, roomTags[0]);
        }
        else
        {
            CreateRoom();
        }
        Close();
    }
    #endregion
    #region "BalObject"
    [Header("BalObject")]
    [SerializeField] private int ph;
    #endregion
    #region "BalEntity"
    [Header("BalEntity")]
    [SerializeField] private int ph0;
    #endregion
    #endregion

    private void Start()
    {
        SpriteSelector.Instance.CheckSpriteFolder();
        Initialize();
        CreateBaseRoom();
        Shader.EnableKeyword("_BG_REQUIRED");
    }

    void Update()
    {
        SubmitDelayed = Submit && !submitPrevious;
        submitPrevious = Submit;

        tool?.state?.Update();

        if (!pause)
        {
            if (inArea)
            {
                GridPositionUpdate();
                GuideUpdate();
                ClickUpdate();
            }
            CameraUpdate();
        }
    }
}
public interface IClickable
{
    bool IsAvailable();
    void OnHighlight();
    void Clicked();
    void OffHighlight();
}
public enum EditorActions
{

}
public abstract class HistoryAction
{
    public bool invert;
    public void Apply(bool undo)
    {
        if (invert != undo) Negative();
        else Positive();
    }
    protected abstract void Positive();
    protected abstract void Negative();
}
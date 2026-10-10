using System.Collections.Generic;
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
    int pauses;
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
    public IClickable clkCurrent, clkPrevious;
    void ClickUpdate()
    {
        if (!inArea)
        {
            return;
        }
        hit2D = Physics2D.Raycast(worldPoint, Camera.main.transform.forward, 99, availableLayers);
        clkCurrent = hit2D.transform ? hit2D.transform.GetComponent<IClickable>() : null;
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

    public IconInstance iconPref;
    public List<IconInstance> icons = new List<IconInstance>();
    public IconInstance CreateIcon(Coordinate pos, Sprite sprite)
    {
        var ico = Instantiate(iconPref, Coordinate.ConvertToMapCoordinate(pos), Quaternion.identity, ec.transform);
        ico.spriteRenderer.sprite = sprite;
        ico.icon.spriteName = sprite ? sprite.name : "Icon_Item";
        ico.UpdateFromData();
        ec.icons.Add(ico.icon);
        icons.Add(ico);
        return ico;
    }
    public void DestroyIcon(Coordinate pos)
    {
        IconInstance tag;
        for (int i = 0; i < icons.Count;)
        {
            tag = icons[i];
            if (Coordinate.ConvertToGridCoordinate(tag.transform.position) == pos)
            {
                DestroyIcon(tag);
            }
            else
            {
                i++;
            }
        }
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

    public Transform bg, cursorCross;
    void GuideUpdate() => cursorCross.position = (Vector3)Coordinate.ConvertToMapCoordinate(cursorGridPos) + Vector3.forward * 5;
    private void Start()
    {
        SpriteSelector.Instance.CheckSpriteFolder();
        Initialize();
        RoomEditor.Instance.CreateRoom();
        Shader.EnableKeyword("_BG_REQUIRED");
        tool.subscribe += (a) => Debug.Log($"Points Received: {string.Join(",,", a)}");
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
    void OnHighlight();
    void Clicked();
    void OffHighlight();
}
public enum EditorActions
{

}
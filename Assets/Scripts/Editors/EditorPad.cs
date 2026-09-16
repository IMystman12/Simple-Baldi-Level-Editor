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
        ec.Resize(new IntVector2(64, 64));
    }
    public void Clear() => Destroy(ec.gameObject);

    public StateMachine editorState = new StateMachine();

    public Toolbar.ToolStateMachine tool = new Toolbar.ToolStateMachine();
    public TMP_Text positionTip;
    public IntVector2 cursorGridPos;
    void GridPositionUpdate()
    {
        cursorGridPos = IntVector2.GetGridPosition(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        cursorGridPos.x = Mathf.Clamp(cursorGridPos.x, 0, ec.realSize.x);
        cursorGridPos.z = Mathf.Clamp(cursorGridPos.z, 0, ec.realSize.z);
        positionTip.text = cursorGridPos.ToString();
    }
    public bool removal;

    [HideInInspector] public bool inArea;
    public void SetArea(bool val) => inArea = val;

    public bool pause { get; private set; }
    public Categories categories;
    public void Pause(bool val)
    {
        pause = val;
        clkPrevious?.OffHighlight();
        categories?.Pause(val);
    }

    [SerializeField] private float moveSensitivity = 10, scaleSensitivity = 10;
    float val;
    Vector2 pos;
    void CameraUpdate()
    {
        val = Time.fixedDeltaTime * (Input.GetKey(KeyCode.LeftShift) ? 5 * moveSensitivity : moveSensitivity);
        pos.x = Mathf.Clamp(pos.x + Input.GetAxis("Horizontal") * val, 0, ec.realSize.x);
        pos.y = Mathf.Clamp(pos.y + Input.GetAxis("Vertical") * val, 0, ec.realSize.x);
        Camera.main.transform.position = new Vector3(pos.x, pos.y, Mathf.Round(transform.position.z));
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize + Input.GetAxis("Mouse ScrollWheel") * scaleSensitivity, 0.1f, 64);
    }

    public LayerMask availableLayers;
    RaycastHit2D hit2D;
    public IClickable clkCurrent, clkPrevious;
    void ClickUpdate()
    {
        if (!inArea)
        {
            return;
        }
        hit2D = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Camera.main.transform.forward, 99, availableLayers);
        clkCurrent = hit2D.transform ? hit2D.transform.GetComponent<IClickable>() : null;
        if (clkCurrent != clkPrevious)
        {
            clkPrevious?.OffHighlight();
            clkCurrent?.OnHighlight();
            clkPrevious = clkCurrent;
        }
        if (Input.GetMouseButtonDown(0))
        {
            clkCurrent?.Clicked();
        }
    }

    public IconTag iconPref;
    public List<IconTag> icons = new List<IconTag>();
    public IconTag CreateIcon(IntVector2 pos, Sprite sprite)
    {
        var ico = Instantiate(iconPref, IntVector2.ToVector2(pos), Quaternion.identity, ec.transform);
        ico.spriteRenderer.sprite = sprite;
        ico.icon.spriteName = sprite ? sprite.name : "Icon_Item";
        ico.UpdateFromData();
        ec.icons.Add(ico.icon);
        icons.Add(ico);
        return ico;
    }
    public void DestroyIcon(IntVector2 pos)
    {
        IconTag tag;
        for (int i = 0; i < icons.Count;)
        {
            tag = icons[i];
            if (IntVector2.GetGridPosition(tag.transform.position) == pos)
            {
                DestroyIcon(tag);
            }
            else
            {
                i++;
            }
        }
    }
    void DestroyIcon(IconTag icon)
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

    public List<string> undo = new List<string>();
    public void TakeAction(string action, bool addToList)
    {

    }

    private void Start()
    {
        Initialize();
        Shader.EnableKeyword("_BG_REQUIRED");
        RoomEditor.Instance.CreateRoom();
        tool.subscribe += (a) => Debug.Log($"Points Received: {string.Join(",,", a)}");
    }

    void Update()
    {
        tool?.state?.Update();

        if (!pause)
        {
            GridPositionUpdate();
            CameraUpdate();
            ClickUpdate();
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
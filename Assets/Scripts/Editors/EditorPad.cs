using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EditorPad : MonoBehaviour
{
    public static EditorPad Instance { get; private set; }
    public Sprite[] sprites;

    public EnvironmentController ec;
    public StateMachine editorState = new StateMachine();

    public Toolbar.ToolStateMachine tool = new Toolbar.ToolStateMachine();
    public IntVector2 cursorGridPos => IntVector2.GetGridPosition(Camera.main.ScreenToWorldPoint(Input.mousePosition));
    public bool removal;

    [HideInInspector] public bool inArea;
    public void SetArea(bool val) => inArea = val;

    public bool pause;
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

    public RoomEditor roomEditor;

    public IconTag iconPref;
    public List<IconTag> icons = new List<IconTag>();
    public IconEditor iconEditor;
    public void CreateIcon(IntVector2 pos)
    {
        var ico = Instantiate(iconPref, IntVector2.ToVector2(pos), Quaternion.identity, ec.transform);
        ec.icons.Add(ico.icon);
        icons.Add(ico);
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


    public DoorTag doorPref;
    public List<DoorTag> doors = new List<DoorTag>();
    public DoorEditor doorEditor;
    public void CreateDoor(IntVector2 pos)
    {
        var ico = Instantiate(iconPref, IntVector2.ToVector2(pos), Quaternion.identity, ec.transform);
        ec.icons.Add(ico.icon);
        icons.Add(ico);
    }
    public void DestroyDoor(IconTag icon)
    {
        ec.icons.Remove(icon.icon);
        icons.Remove(icon);
        Destroy(icon.gameObject);
    }

    private void Awake() => Instance = this;

    private void Start()
    {
        roomEditor.CreateRoom();
        sprites = sprites.Distinct().ToArray();
        tool.subscribe += (a) => Debug.Log($"Points Received: {string.Join(",,", a)}");
    }

    void Update()
    {
        tool?.state?.Update();

        if (!pause)
        {
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
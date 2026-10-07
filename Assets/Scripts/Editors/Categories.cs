
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class Categories : Singleton<Categories>
{
    public enum Category
    {
        Room = -2,
        None = -1,
        Cell,
        Icon,
        Door,
        Lightmap,
        Options,
        Save,
        Navigation
    }
    public Category category;
    public Toggle[] toggles = new Toggle[6];
    void Start() => UpdateBool();
    public void Reset()
    {
        SpriteSelector.Instance.Close();
        Toolbar.Instance.Close();
        EditorPad.Instance.editorState.ChangeState(null);
    }
    public void Pause(bool val)
    {
        gameObject.SetActive(!val);
        Reset();
        if (!val)
        {
            UpdateBool();
        }
    }
    public void UpdateBool()
    {
        Reset();

        Category newCate = Category.None;
        for (int i = 0; i < toggles.Length; i++)
        {
            if (toggles[i].isOn)
            {
                newCate = (Category)i;
                break;
            }
        }

        category = newCate;
        switch (category)
        {
            case Category.None:
                Reset();
                break;
            case Category.Cell:
                EditorPad.Instance.editorState.ChangeState(new Editor_Cell());
                Toolbar.Instance.Open();
                SpriteSelector.Instance.Open(category);
                break;
            case Category.Icon:
                EditorPad.Instance.editorState.ChangeState(new Editor_Icon());
                Toolbar.Instance.Open();
                SpriteSelector.Instance.Open(category);
                break;
            case Category.Door:
                EditorPad.Instance.editorState.ChangeState(new Editor_Door());
                Toolbar.Instance.Open();
                Toolbar.Instance.Disable(1, true);
                SpriteSelector.Instance.Open(category);
                break;
            case Category.Lightmap:
                EditorPad.Instance.editorState.ChangeState(new Editor_Lightmap());
                Toolbar.Instance.Open();
                break;
            case Category.Options:
                OptionEditor.Instance.Open();
                toggles[(int)Category.Options].isOn = false;
                break;
            case Category.Save:
                SaveEditor.Instance.Open();
                toggles[(int)Category.Save].isOn = false;
                break;
            case Category.Navigation:
                EditorPad.Instance.editorState.ChangeState(new Editor_Navigation());
                Toolbar.Instance.Open();
                Toolbar.Instance.Disable(1, true);
                break;
        }
        Toolbar.Instance.UpdateValue();
    }
    public class Editor_Cell : StateBase
    {
        CellInstance c;
        public override void Enter() => EditorPad.Instance.tool.subscribe = (a) =>
            {
                foreach (var b in a)
                {
                    c = EditorPad.Instance.ec.CellFromPosition(b);
                    if (!EditorPad.Instance.removal)
                    {
                        if (c == null)
                        {
                            if (SpriteSelector.Instance.selectionIndex == 16)
                            {
                                EditorPad.Instance.ec.ConnectSurround(EditorPad.Instance.ec.CreateCell(b, RoomEditor.Instance.currentTag.room));
                            }
                            else
                            {
                                EditorPad.Instance.ec.CreateCell(b, RoomEditor.Instance.currentTag.room, SpriteSelector.Instance.selectionIndex);
                            }
                        }
                    }
                    else if (EditorPad.Instance.removal && c && c.room == RoomEditor.Instance.currentTag.room)
                    {
                        if (c != null)
                        {
                            if (SpriteSelector.Instance.selectionIndex > 15)
                            {
                                EditorPad.Instance.ec.DestroyCell(c);
                            }
                            else if (c.data.id == SpriteSelector.Instance.selectionIndex)
                            {
                                EditorPad.Instance.ec.DestroyCell(c);
                            }
                        }
                    }
                }
            };
        public override void Exit() => EditorPad.Instance.tool.subscribe = null;
    }
    public class Editor_Icon : StateBase
    {
        public override void Enter() => EditorPad.Instance.tool.subscribe = (a) =>
            {
                foreach (var b in a)
                {
                    if (EditorPad.Instance.removal)
                    {
                        EditorPad.Instance.DestroyIcon(b);
                    }
                    else
                    {
                        EditorPad.Instance.CreateIcon(b, SpriteSelector.Instance.Selection);
                    }
                }
            };
        public override void Exit() => EditorPad.Instance.tool.subscribe = null;
    }
    public class Editor_Door : StateBase
    {
        bool hasPosA;
        Coordinate posA;
        public override void Enter()
        {
            Toolbar.Instance.tip.text = "Door: Position?";
            Toolbar.Tool_Painter.clickMode = true;
            EditorPad.Instance.tool.subscribe = (a) =>
                {
                    if (!hasPosA)
                    {
                        hasPosA = true;
                        posA = a[0];
                        Toolbar.Instance.tip.text = "Door: Direction?";
                    }
                    else if (Mathf.Abs(posA.x - a[0].x) != Mathf.Abs(posA.z - a[0].z) && (Mathf.Abs(posA.x - a[0].x) == 1 || Mathf.Abs(posA.z - a[0].z) == 1))
                    {
                        Toolbar.Instance.tip.text = "Door: Position?";
                        Towards direction = TowardsExtension.FromPointAToB(posA, a[0]);
                        if (direction != Towards.NaD && EditorPad.Instance.ec.CellFromPosition(posA))
                        {
                            if (EditorPad.Instance.removal)
                            {
                                EditorPad.Instance.ec.DestroyDoor(posA, direction);
                            }
                            else
                            {
                                EditorPad.Instance.ec.CreateDoor(posA, direction, SpriteSelector.Instance.Selection);
                            }
                        }
                        hasPosA = false;
                        EditorPad.Instance.tool.state.ForceReset();
                    }
                };
        }
        public override void Exit()
        {
            Toolbar.Tool_Painter.clickMode = false;
            Toolbar.Instance.tip.text = string.Empty;
            EditorPad.Instance.tool.subscribe = null;
        }
    }
    public class Editor_Lightmap : StateBase
    {
        public override void Enter()
        {
            LightmapEditor.Instance.StartRender();
            EditorPad.Instance.tool.subscribe = (a) =>
            {
                if (EditorPad.Instance.removal)
                {
                    foreach (var b in a)
                    {
                        LightmapEditor.Instance.DestroyLight(b);
                    }
                }
                else
                {
                    foreach (var b in a)
                    {
                        LightmapEditor.Instance.CreateLight(b);
                    }
                }
            };
        }
        public override void Exit()
        {
            Toolbar.Tool_Painter.clickMode = false;
            EditorPad.Instance.tool.subscribe = null;
            LightmapEditor.Instance.StopRender();
        }
    }
    public class Editor_Navigation : StateBase
    {
        bool startDetected;
        Coordinate startCoordinate;
        public override void Enter()
        {
            Toolbar.Instance.tip.text = "Path: Start?";
            NavigationGuides.Instance.Show(true);
            Toolbar.Tool_Painter.clickMode = true;
            EditorPad.Instance.tool.subscribe = (a) =>
            {
                if (!startDetected)
                {
                    Toolbar.Instance.tip.text = "Path: End?";
                    startCoordinate = a[0];
                    startDetected = true;
                    return;
                }
                if (EditorPad.Instance.removal)
                {
                    NavigationGuides.Instance.RemovePath(startCoordinate, a[0]);
                }
                else
                {
                    NavigationGuides.Instance.MakePath(startCoordinate, a[0]);
                }
                Toolbar.Instance.tip.text = "Path: Start?";
                startDetected = false;
            };
        }
        public override void Exit()
        {
            Toolbar.Tool_Painter.clickMode = false;
            EditorPad.Instance.tool.subscribe = null;
            Toolbar.Instance.tip.text = "";
            NavigationGuides.Instance.Show(false);
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

public class Categories : Singleton<Categories>
{
    public enum Category
    {
        None = -1,
        Cell,
        Icon,
        Door,
        Generator,
        Options,
        Save,
        Multiplayer
    }
    public Category category;
    public Toggle[] toggles = new Toggle[6];
    void Start() => UpdateBool();
    public void Reset()
    {
        SpriteSelector.Instance.Close();
        Toolbar.Instance.gameObject.SetActive(false);
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
                Toolbar.Instance.gameObject.SetActive(true);
                Toolbar.Instance.ResetAll(true);
                SpriteSelector.Instance.Open(category);
                break;
            case Category.Icon:
                EditorPad.Instance.editorState.ChangeState(new Editor_Icon());
                Toolbar.Instance.gameObject.SetActive(true);
                Toolbar.Instance.ResetAll(true);
                SpriteSelector.Instance.Open(category);
                break;
            case Category.Door:
                EditorPad.Instance.editorState.ChangeState(new Editor_Door());
                Toolbar.Instance.gameObject.SetActive(true);
                Toolbar.Instance.ResetAll(true);
                Toolbar.Instance.Disable(1, true);
                SpriteSelector.Instance.Open(category);
                break;
            case Category.Generator:
                EditorPad.Instance.editorState.ChangeState(new Editor_Generator());
                Toolbar.Instance.gameObject.SetActive(true);
                Toolbar.Instance.ResetAll(true);
                Toolbar.Instance.Disable(0, true);
                SpriteSelector.Instance.Open(category);
                break;
            case Category.Save:
                break;
            case Category.Multiplayer:
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
                    if (!EditorPad.Instance.removal && c == null)
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
                    else if (EditorPad.Instance.removal && c && c.room == RoomEditor.Instance.currentTag.room)
                    {
                        EditorPad.Instance.ec.DestroyCell(EditorPad.Instance.ec.CellFromPosition(b));
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
        IntVector2 posA;
        public override void Enter() => EditorPad.Instance.tool.subscribe = (a) =>
            {
                Debug.Log(hasPosA);
                if (!hasPosA)
                {
                    hasPosA = true;
                    posA = a[0];
                    return;
                }
                Direction direction = Directions.FromPointAToB(posA, a[0]);
                if (EditorPad.Instance.ec.CellFromPosition(posA) && EditorPad.Instance.ec.CellFromPosition(a[0]))
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
            };
        public override void Exit() => EditorPad.Instance.tool.subscribe = null;
    }
    public class Editor_Generator : StateBase
    {
        public override void Enter() => EditorPad.Instance.tool.subscribe = (a) =>
            {
                foreach (var b in a)
                {
                }
            };
        public override void Exit() => EditorPad.Instance.tool.subscribe = null;
    }
}
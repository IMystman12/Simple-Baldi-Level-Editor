
using UnityEngine;
using UnityEngine.UI;

public class Categories : MonoBehaviour
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
    public Toolbar toolbar;
    public SpriteSelector spriteSelector;
    void Start() => UpdateBool();
    public void Reset()
    {
        spriteSelector.Close();
        toolbar.gameObject.SetActive(false);
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
                toolbar.gameObject.SetActive(true);
                toolbar.ResetAll(true);
                spriteSelector.Open(category);
                break;
            case Category.Icon:
                EditorPad.Instance.editorState.ChangeState(new Editor_Icon());
                toolbar.gameObject.SetActive(true);
                toolbar.ResetAll(true);
                spriteSelector.Open(category);
                break;
            case Category.Door:
                EditorPad.Instance.editorState.ChangeState(new Editor_Door());
                toolbar.gameObject.SetActive(true);
                toolbar.ResetAll(true);
                toolbar.Disable(1, true);
                spriteSelector.Open(category);
                break;
            case Category.Generator:
                EditorPad.Instance.editorState.ChangeState(new Editor_Generator());
                toolbar.gameObject.SetActive(true);
                toolbar.ResetAll(true);
                toolbar.Disable(0, true);
                spriteSelector.Open(category);
                break;
            case Category.Save:
                break;
            case Category.Multiplayer:
                break;
        }
        toolbar.UpdateValue();
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
                        EditorPad.Instance.ec.ConnectSurround(EditorPad.Instance.ec.CreateCell(b, EditorPad.Instance.roomEditor.currentTag.room));
                    }
                    else if (EditorPad.Instance.removal && c && c.room == EditorPad.Instance.roomEditor.currentTag.room)
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
                        EditorPad.Instance.CreateIcon(b);
                    }
                }
            };
        public override void Exit() => EditorPad.Instance.tool.subscribe = null;
    }
    public class Editor_Door : StateBase
    {
        public override void Enter() => EditorPad.Instance.tool.subscribe = (a) =>
            {
                if (a.Length != 2)
                {
                    return;
                }

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
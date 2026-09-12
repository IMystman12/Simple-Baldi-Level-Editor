
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
    void Start() => UpdateBool();
    public void Off()
    {
        for (int i = 0; i < toggles.Length; i++)
        {
            toggles[i].isOn = false;
        }
        UpdateBool();
        gameObject.SetActive(false);
    }
    public void UpdateBool()
    {
        toolbar.SetActive(false);
        EditorPad.Instance.editorState.ChangeState(null);

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
            case Category.Cell:
                EditorPad.Instance.editorState.ChangeState(new Editor_Cell());
                toolbar.SetActive(true);
                break;
            case Category.Icon:
                EditorPad.Instance.editorState.ChangeState(new Editor_Icon());
                toolbar.SetActive(true);
                break;
            case Category.Door:
                EditorPad.Instance.editorState.ChangeState(new Editor_Door());
                toolbar.SetActive(true);
                toolbar.Disable(1, true);
                break;
            case Category.Generator:
                EditorPad.Instance.editorState.ChangeState(new Editor_Generator());
                toolbar.SetActive(true);
                toolbar.Disable(0, true);
                break;
            case Category.Save:
                break;
            case Category.Multiplayer:
                break;
        }
    }
    public class Editor_Cell : StateBase
    {
        public override void Enter() => EditorPad.Instance.tool.subscribe = (a) =>
            {
                foreach (var b in a)
                {
                    if (EditorPad.Instance.removal)
                    {
                        if (EditorPad.Instance.ec.CellFromPosition(b))
                        {
                            EditorPad.Instance.ec.DestroyCell(EditorPad.Instance.ec.CellFromPosition(b));
                        }
                    }
                    else
                    {
                        EditorPad.Instance.ec.ConnectSurround(EditorPad.Instance.ec.CreateCell(b, EditorPad.Instance.roomEditor.currentTag.room));
                    }
                }
            };
        public override void Exit() => EditorPad.Instance.tool.subscribe = null;
    }
    public class Editor_Icon : StateBase
    {
        public static bool stop;
        public override void Enter() => EditorPad.Instance.tool.subscribe = (a) =>
            {
                if (stop)
                {
                    return;
                }
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
                foreach (var b in a)
                {
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
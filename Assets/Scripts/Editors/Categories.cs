
using System.Collections.Generic;
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
        Setting,
        Save,
        BaldiMan
    }
    public Category category;
    public Toggle[] toggles = new Toggle[6];
    public Button playButton;
    void Start() => UpdateBool();
    public void Reset()
    {
        playButton.gameObject.SetActive(false);
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
                Toolbar.Instance.Open();
                SpriteSelector.Instance.Open(category);
                SpriteSelector.Instance.SetValue(16);
                EditorPad.Instance.editorState.ChangeState(new Editor_Cell());
                break;
            case Category.Icon:
                Toolbar.Instance.Open();
                SpriteSelector.Instance.Open(category);
                EditorPad.Instance.editorState.ChangeState(new Editor_Icon());
                break;
            case Category.Door:
                Toolbar.Instance.Open();
                Toolbar.Instance.Disable(1, true);
                SpriteSelector.Instance.Open(category);
                EditorPad.Instance.editorState.ChangeState(new Editor_Door());
                break;
            case Category.Lightmap:
                Toolbar.Instance.Open();
                EditorPad.Instance.editorState.ChangeState(new Editor_Lightmap());
                break;
            case Category.Setting:
                EditorPad.Instance.Open(EditorPad.EditorType.Setting);
                toggles[(int)Category.Setting].isOn = false;
                break;
            case Category.Save:
                EditorPad.Instance.Open(EditorPad.EditorType.Save);
                toggles[(int)Category.Save].isOn = false;
                break;
            case Category.BaldiMan:
                playButton.gameObject.SetActive(true);
                Toolbar.Instance.Open();
                SpriteSelector.Instance.Open(category);
                SpriteSelector.Instance.SetValue(0);
                EditorPad.Instance.editorState.ChangeState(new Editor_BaldiMan());
                break;
        }
        Toolbar.Instance.UpdateValue();
    }
    public class Editor_Cell : StateBase
    {
        public override void Enter() => EditorPad.Instance.tool.subscribe = (a) => EditorPad.Instance.RecordStep(new HistoryAction_Cell(a));
        public override void Exit() => EditorPad.Instance.tool.subscribe = null;
        public class HistoryAction_Cell : HistoryAction
        {
            Room r;
            int i;
            CellInstance c;
            Coordinate[] array;
            public HistoryAction_Cell(Coordinate[] coordinates)
            {
                r = EditorPad.Instance.currentRoomTag.room;
                i = SpriteSelector.Instance.selectionIndex;
                invert = EditorPad.Instance.removal;
                array = coordinates;
            }
            protected override void Positive()
            {
                foreach (var b in array)
                {
                    c = EditorPad.Instance.ec.CellFromPosition(b);
                    if (!c)
                    {
                        if (i == 16)
                        {
                            EditorPad.Instance.ec.ConnectSurround(EditorPad.Instance.ec.CreateCell(b, r));
                        }
                        else
                        {
                            EditorPad.Instance.ec.CreateCell(b, r, i);
                        }
                    }
                }
            }
            protected override void Negative()
            {
                foreach (var b in array)
                {
                    c = EditorPad.Instance.ec.CellFromPosition(b);
                    if (c && c.room == r && (i > 15 || c.data.id == i))
                    {
                        EditorPad.Instance.ec.DestroyCell(c);
                    }
                }
            }
        }
    }
    public class Editor_Icon : StateBase
    {
        public override void Enter() => EditorPad.Instance.tool.subscribe = (a) => EditorPad.Instance.RecordStep(new HistoryAction_Icon(a));
        public override void Exit() => EditorPad.Instance.tool.subscribe = null;
        public class HistoryAction_Icon : HistoryAction
        {
            string spriteName;
            Icon[] icons = new Icon[0];
            Coordinate[] array;
            public HistoryAction_Icon(Coordinate[] coordinates)
            {
                spriteName = SpriteSelector.Instance.Selection.name;
                invert = EditorPad.Instance.removal;
                array = coordinates;
            }

            protected override void Positive()
            {
                IconInstance iconI;
                for (int i = 0; i < array.Length; i++)
                {
                    if (icons.Length == 0)
                    {
                        EditorPad.Instance.CreateIcon(array[i], spriteName);
                    }
                    else
                    {
                        iconI = EditorPad.Instance.CreateIcon(default, null);
                        icons[i].CopyTo(iconI.icon);
                        iconI.UpdateFromData();
                    }
                }
            }

            protected override void Negative()
            {
                var list = new List<Icon>();
                for (int i = 0; i < array.Length; i++)
                {
                    list.AddRange(EditorPad.Instance.DestroyIcon(array[i]));
                }
                icons = list.ToArray();
            }
        }
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
                            EditorPad.Instance.RecordStep(new HistoryAction_Door(posA, direction));
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
        public class HistoryAction_Door : HistoryAction
        {
            Door backup;
            Coordinate coordinate;
            Towards towards;
            string selection;
            public HistoryAction_Door(Coordinate coordinate, Towards towards)
            {
                this.coordinate = coordinate;
                this.towards = towards;
                selection = SpriteSelector.Instance.Selection.name;
                invert = EditorPad.Instance.removal;
            }
            protected override void Positive()
            {
                if (backup == null)
                {
                    backup = EditorPad.Instance.ec.CreateDoor(coordinate, towards, selection).door;
                }
                else
                {
                    EditorPad.Instance.ec.LoadDoor(backup);
                }
            }
            protected override void Negative() => backup = EditorPad.Instance.ec.DestroyDoor(coordinate, towards);
        }
    }
    public class Editor_Lightmap : StateBase
    {
        public override void Enter()
        {
            EditorPad.Instance.StartLightRender();
            EditorPad.Instance.tool.subscribe = (a) => EditorPad.Instance.RecordStep(new HistoryAction_Lightmap(a));
        }
        public override void Exit()
        {
            EditorPad.Instance.tool.subscribe = null;
            EditorPad.Instance.StopLightRender();
        }
        public class HistoryAction_Lightmap : HistoryAction
        {
            Coordinate[] array;
            public HistoryAction_Lightmap(Coordinate[] coordinates)
            {
                invert = EditorPad.Instance.removal;
                array = coordinates;
            }
            protected override void Positive()
            {
                foreach (var b in array)
                {
                    EditorPad.Instance.CreateLight(b);
                }
            }
            protected override void Negative()
            {
                foreach (var b in array)
                {
                    EditorPad.Instance.DestroyLight(b);
                }
            }
        }
    }
    public class Editor_BaldiMan : StateBase
    {
        public override void Enter()
        {
            Toolbar.Tool_Painter.clickMode = true;
            EditorPad.Instance.tool.subscribe = (a) =>
               {
                   if (SpriteSelector.Instance.selectionIndex > 1)
                   {
                       int i = SpriteSelector.Instance.selectionIndex - 2;
                       if (EditorPad.Instance.removal)
                       {
                           foreach (var b in a)
                           {
                               BaldiManager.Instance.RemoveEntityFromSpawn(i, b);
                           }
                       }
                       else
                       {
                           foreach (var b in a)
                           {
                               Debug.Log("abc");
                               BaldiManager.Instance.AddEntityToSpawn(i, b);
                           }
                       }
                   }
               };
        }
        public override void Exit()
        {
            Toolbar.Tool_Painter.clickMode = false;
            EditorPad.Instance.tool.subscribe = null;
        }
    }
}
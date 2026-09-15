using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Toolbar : Singleton<Toolbar>
{
    public enum Tool
    {
        None = -1,
        Painter,
        Area,
    }
    public class ToolStateMachine : StateMachine<Tool_StateBase>
    {
        public Action<IntVector2[]> subscribe;
        public override void ChangeState(Tool_StateBase newState)
        {
            base.ChangeState(newState);
            if (state != null)
            {
                state.subscribe = subscribe;
            }
        }
    }
    public class Tool_StateBase : StateBase
    {
        protected bool fitForBuild => EditorPad.Instance.inArea && EditorPad.Instance.ec.ContainsCoordinates(EditorPad.Instance.cursorGridPos);
        protected bool submit => Input.GetMouseButton(0);
        protected bool submitDelayed => Input.GetMouseButtonDown(0);
        public Action<IntVector2[]> subscribe;
        protected void ReceivePosition(params IntVector2[] positions) => subscribe?.Invoke(positions);
        public virtual void ForceReset()
        { }
    }
    public class Tool_Painter : Tool_StateBase
    {
        IntVector2 pos = IntVector2.one * -1;
        public override void Update()
        {
            if (fitForBuild && submit && pos != EditorPad.Instance.cursorGridPos)
            {
                pos = EditorPad.Instance.cursorGridPos;
                ReceivePosition(pos);
            }
        }
        public override void ForceReset() => pos = IntVector2.one * -1;
    }
    public class Tool_Area : Tool_StateBase
    {
        public bool first = true;
        public IntVector2 posA;
        public override void Update()
        {
            if (fitForBuild && submitDelayed)
            {
                if (first)
                {
                    first = false;
                    posA = EditorPad.Instance.cursorGridPos;
                    Debug.Log("PosA ready");
                }
                else
                {
                    var posB = EditorPad.Instance.cursorGridPos;
                    List<IntVector2> positions = new List<IntVector2>();
                    for (int j = Mathf.Min(posA.z, posB.z), j0 = Mathf.Max(posA.z, posB.z); j <= j0; j++)
                    {
                        for (int i = Mathf.Min(posA.x, posB.x), i0 = Mathf.Max(posA.x, posB.x); i <= i0; i++)
                        {
                            positions.Add(new IntVector2(i, j));
                        }
                    }
                    ReceivePosition(positions.ToArray());
                    first = true;
                    Debug.Log("PosB ready");
                }
            }
        }
    }

    public Toggle[] tools = new Toggle[2];
    public Toggle remove;
    public void Disable(int i, bool val)
    {
        if (i < tools.Length)
        {
            tools[i].interactable = !val;
            return;
        }
        remove.interactable = !val;
    }
    public void ResetAll(bool val)
    {
        for (int i = 0; i < tools.Length + 1; i++)
        {
            Disable(i, !val);
        }
    }
    public void UpdateValue()
    {
        EditorPad.Instance.removal = remove.isOn;
        var tool = Tool.None;
        for (int i = 0; i < tools.Length; i++)
        {
            if (tools[i].isOn && tools[i].interactable)
            {
                tool = (Tool)i;
                break;
            }
        }

        switch (tool)
        {
            case Tool.None:
                EditorPad.Instance.tool.ChangeState(null);
                break;
            case Tool.Painter:
                EditorPad.Instance.tool.ChangeState(new Tool_Painter());
                break;
            case Tool.Area:
                EditorPad.Instance.tool.ChangeState(new Tool_Area());
                break;
        }
    }
}

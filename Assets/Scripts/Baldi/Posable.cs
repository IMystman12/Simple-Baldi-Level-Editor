using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Posable : MonoBehaviour, IClickable
{
    public SpriteRenderer spriteRenderer;
    public void Clicked()
    {
    }
    public void OffHighlight() => spriteRenderer.transform.localScale = Vector3.one;
    public void OnHighlight() => spriteRenderer.transform.localScale = Vector3.one * 3;
    public bool IsAvailable() => EditorPad.Instance.tool.state == null && EditorPad.Instance.editorState.state != null && EditorPad.Instance.editorState.state.GetType() == typeof(Categories.Editor_BaldiMan);
}

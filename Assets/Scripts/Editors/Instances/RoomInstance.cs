using TMPro;
using UnityEngine;

public class RoomInstance : MonoBehaviour, IClickable
{
    public TMP_Text text;
    public Room room;
    public void UpdateName() => text.text = room.name;
    public void Clicked() => EditorPad.Instance.Open(EditorPad.EditorType.Room, this);
    public void OffHighlight() => text.color = EditorPad.Instance.currentRoomTag == this ? Color.yellow : Color.white;
    public void OnHighlight() => text.color = Color.green;
    public bool IsAvailable() => this != null && EditorPad.Instance.tool.state == null && EditorPad.SubEditorClosed;
}

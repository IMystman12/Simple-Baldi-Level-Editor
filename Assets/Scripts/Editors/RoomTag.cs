using TMPro;
using UnityEngine;

public class RoomTag : MonoBehaviour, IClickable
{
    public TMP_Text text;
    public Room room;
    public void UpdateName() => text.text = room.name;
    public void Clicked() => EditorPad.Instance.OpenRoomEditor(this);
    public void OffHighlight() => text.color = EditorPad.Instance.roomEditor.currentTag == this ? Color.yellow : Color.white;
    public void OnHighlight() => text.color = Color.green;
}

using TMPro;
using UnityEngine;

public class RoomTag : MonoBehaviour, IClickable
{
    public TMP_Text text;
    public Room room;
    public void UpdateName() => text.text = room.name;
    public void Clicked() => RoomEditor.Instance.Open(this);
    public void OffHighlight() => text.color = RoomEditor.Instance.currentTag == this ? Color.yellow : Color.white;
    public void OnHighlight() => text.color = Color.green;
}

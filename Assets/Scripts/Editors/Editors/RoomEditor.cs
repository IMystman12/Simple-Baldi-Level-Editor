using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class RoomEditor : Singleton<RoomEditor>
{
    public RoomTag tagPref;
    public Transform tagManager;
    public List<RoomTag> tags = new List<RoomTag>();
    public RoomTag currentTag;
    public GameObject destroyButton;
    public void Open(RoomTag roomTag)
    {
        currentTag = roomTag;
        tags.ForEach(a => a.OffHighlight());
        nameField.text = currentTag.room.name;
        ApplyName();
        colorField.color = currentTag.room.color;
        colorField.ShowColor();

        destroyButton.gameObject.SetActive(EditorPad.Instance.ec.rooms.Count > 1);
        EditorPad.Instance.Pause(true);
        gameObject.SetActive(true);

        SpriteSelector.Instance.Open(Categories.Category.Room);
        SpriteSelector.Instance.TryToSetValue(roomTag.room.mapBGName);
        SpriteSelector.Instance.onCheckValue?.AddListener(() =>
        {
            roomTag.room.mapBGName = SpriteSelector.Instance.Selection.name;
            roomTag.room.ChangeColor(EditorPad.Instance.ec);
        });
    }
    public void Close()
    {
        SpriteSelector.Instance.Close();
        gameObject.SetActive(false);
        EditorPad.Instance.Pause(false);
    }

    public void CreateRoom() => EditorPad.Instance.RecordStep(new HistoryAction_Room(new Room() { color = Color.white }, false));
    public void DestroyThis() => EditorPad.Instance.RecordStep(new HistoryAction_Room(currentTag.room, true));

    public void UpdateTags()
    {
        var ec = EditorPad.Instance.ec;
        for (int i = 0; i < tags.Count;)
        {
            if (!ec.rooms.Contains(tags[i].room))
            {
                Destroy(tags[i].gameObject);
                tags.RemoveAt(i);
            }
            else
            {
                i++;
            }
        }

        RoomTag tag;
        for (int i = 0; i < ec.rooms.Count; i++)
        {
            if (!tags.Any(a => a.room == ec.rooms[i]))
            {
                tag = Instantiate(tagPref, tagManager);
                tag.room = ec.rooms[i];
                tag.UpdateName();
                tags.Add(tag);
            }
        }

        RequeueTags();
    }
    void RequeueTags()
    {
        var k = Vector3.up * 1.5f;
        for (int i = 0; i < tags.Count; i++)
        {
            tags[i].transform.localPosition = k * i;
        }
    }

    public TMP_InputField nameField;
    public void ApplyName()
    {
        if (!string.IsNullOrWhiteSpace(nameField.text))
        {
            currentTag.room.name = nameField.text;
            currentTag.UpdateName();
        }
        nameField.text = currentTag.room.name;
    }


    public ColorField colorField;
    public void ColorUpdate()
    {
        currentTag.room.color = colorField.color;
        currentTag.room.ChangeColor(EditorPad.Instance.ec);
    }

    public class HistoryAction_Room : HistoryAction
    {
        Room backup;
        public HistoryAction_Room(Room room, bool invert)
        {
            this.invert = invert;
            backup = room;
        }
        protected override void Positive()
        {
            if (Instance.gameObject.activeSelf)
            {
                Instance.Close();
            }
            backup = EditorPad.Instance.ec.LoadRoom(backup);
            Instance.UpdateTags();
            Instance.Open(Instance.tags[Instance.tags.Count - 1]);
        }
        protected override void Negative()
        {
            EditorPad.Instance.ec.DestroyRoom(backup);
            Instance.UpdateTags();
            Instance.Close();
        }
    }
}

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
    public void Open(RoomTag roomTag)
    {
        currentTag = roomTag;
        tags.ForEach(a => a.OffHighlight());
        nameField.text = currentTag.room.name;
        ApplyName();
        ShowColor();
        
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
    public void CreateRoom()
    {
        gameObject.SetActive(false);
        EditorPad.Instance.ec.CreateRoom(Color.white);
        UpdateTags();
        Open(tags[tags.Count - 1]);
    }
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
    public void DestroyThis()
    {
        EditorPad.Instance.ec.DestroyRoom(currentTag.room);
        tags.Remove(currentTag);
        Destroy(currentTag.gameObject);
        RequeueTags();
        Close();
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

    public TMP_InputField colorField;
    public void TryApplyColor()
    {
        string[] array = colorField.text.Split(',');
        if (array.Length < 3)
        {
            ShowColor();
            return;
        }
        int val;
        Color result = Color.white;
        for (int i = 0; i < 3; i++)
        {
            if (int.TryParse(array[i], out val) && val < 256)
            {
                result[i] = val / 255;
            }
            else
            {
                ShowColor();
                return;
            }
        }
        currentTag.room.color = result;
        currentTag.room.ChangeColor(EditorPad.Instance.ec);
    }
    void ShowColor() => colorField.text = string.Join(",", currentTag.room.color.r * 255, currentTag.room.color.g * 255, currentTag.room.color.b * 255);
}

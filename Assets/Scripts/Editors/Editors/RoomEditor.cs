using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RoomEditor : MonoBehaviour
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
    }
    public void Close()
    {
        gameObject.SetActive(false);
        EditorPad.Instance.Pause(false);
    }
    public void CreateRoom()
    {
        gameObject.SetActive(false);
        var tag = Instantiate(tagPref, tagManager);
        tag.room = EditorPad.Instance.ec.CreateRoom(Color.white);
        tags.Add(tag);
        Open(tags[tags.Count - 1]);
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
        if (!string.IsNullOrEmpty(nameField.text) && !string.IsNullOrWhiteSpace(nameField.text))
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

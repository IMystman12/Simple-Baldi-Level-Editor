using TMPro;
using UnityEngine;

public class IconEditor : Singleton<IconEditor>
{
    public IconInstance currentTag;
    public void Open(IconInstance icon)
    {
        currentTag = icon;

        colorField.color = currentTag.icon.color;
        colorField.ShowColor();

        ShowPosition();
        ShowRotation();
        EditorPad.Instance.Pause(true);
        gameObject.SetActive(true);
    }
    public void Close()
    {
        gameObject.SetActive(false);
        EditorPad.Instance.Pause(false);
    }

    public TMP_InputField positionField;
    public void TryApplyPosition()
    {
        string[] array = positionField.text.Split(',');
        if (array.Length < 2)
        {
            ShowPosition();
            return;
        }
        float val;
        Vector2 result = default;
        for (int i = 0; i < 2; i++)
        {
            if (float.TryParse(array[i], out val))
            {
                result[i] = val;
            }
            else
            {
                ShowPosition();
                return;
            }
        }
        currentTag.icon.position = result;
        currentTag.UpdateFromData();
    }
    public void ShowPosition() => positionField.text = string.Join(",", currentTag.icon.position.x, currentTag.icon.position.y);

    public TMP_InputField rotationField;
    public void TryApplyRotation()
    {
        float val;
        if (float.TryParse(rotationField.text, out val))
        {
            currentTag.icon.rotation = val;
            currentTag.UpdateFromData();
        }
    }
    public void ShowRotation() => rotationField.text = currentTag.icon.rotation.ToString();

    public ColorField colorField;
    public void ColorUpdate()
    {
        currentTag.icon.color = colorField.color;
        currentTag.UpdateFromData();
    }
}

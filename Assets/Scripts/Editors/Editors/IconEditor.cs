using TMPro;
using UnityEngine;

public class IconEditor : MonoBehaviour
{
    public IconTag currentTag;
    public void Open(IconTag icon)
    {
        currentTag = icon;
        ShowColor();
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

    public SpriteSelector spriteSelector;
    public bool hasNewSprite;
    void OnEnable()
    {
        if (hasNewSprite)
        {
            hasNewSprite = false;
            currentTag.icon.spriteName = spriteSelector.result.name;
            currentTag.UpdateFromData();
        }
    }
    public void SetSprite()
    {
        spriteSelector.Open(gameObject);
        hasNewSprite = true;
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
        for (int i = 0; i < 4; i++)
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
        currentTag.icon.color = result;
        currentTag.UpdateFromData();
    }
    void ShowColor() => colorField.text = string.Join(",", currentTag.icon.color.r * 255, currentTag.icon.color.g * 255, currentTag.icon.color.b * 255);
}

using TMPro;
using UnityEngine;

public class ColorField : MonoBehaviour
{
    public Color color = Color.white;
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
                result[i] = val / 255f;
            }
            else
            {
                ShowColor();
                return;
            }
        }
        color = result;
        ShowColor();
    }
    public void ShowColor()
    {
        colorField.text = string.Join(",", color.r * 255, color.g * 255, color.b * 255);
        colorField.textComponent.color = (0.2126f * color.r + 0.7152f * color.g + 0.0722f * color.b) < 0.1f ? Color.grey : color;
    }
}

using TMPro;
using UnityEngine;

public class OptionEditor : Singleton<OptionEditor>
{
    public void Open()
    {
        EditorPad.Instance.Pause(true);
        gameObject.SetActive(true);
        ShowSize();
    }
    public void Close()
    {
        gameObject.SetActive(false);
        EditorPad.Instance.Pause(false);
    }

    public TMP_InputField sizeField;
    public void TryToApplySize()
    {
        string[] array = sizeField.text.Split(',');
        if (array.Length < 2)
        {
            ShowSize();
            return;
        }
        int val;
        IntVector2 result = default;
        for (int i = 0; i < 2; i++)
        {
            if (int.TryParse(array[i], out val))
            {
                result[i] = val;
            }
            else
            {
                ShowSize();
                return;
            }
        }
        EditorPad.Instance.ec.Resize(result);
    }
    public void ShowSize() => sizeField.text = string.Join(",", EditorPad.Instance.ec.size.x, EditorPad.Instance.ec.size.z);

    public RenderTexture mapTex;
    public Camera mapRender;
    public void UpdateSize(IntVector2 newSize)
    {
        mapTex.Release();
        mapTex.width = newSize.x * 16;
        mapTex.height = newSize.z * 16;
        mapTex.Create();

        Vector3 vector = new Vector3(newSize.x / 2 - 0.5f, newSize.z / 2 - 0.5f, -1);
        mapRender.transform.position = vector;
        mapRender.aspect = (float)newSize.x / newSize.z;
        mapRender.orthographicSize = newSize.z / 2f;
    }

    public GameObject guide;
    public void ToggleGuide() => guide.SetActive(!guide.activeSelf);
}

using TMPro;
using UnityEngine;

public class OptionEditor : Singleton<OptionEditor>
{
    public void Open()
    {
        EditorPad.Instance.Pause(true);
        gameObject.SetActive(true);
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
        UpdateSize(result);
    }
    public void ShowSize() => sizeField.text = string.Join(",", EditorPad.Instance.ec.size.x, EditorPad.Instance.ec.size.z);

    public RenderTexture mapTex;
    public Camera mapRender;
    public void UpdateSize(IntVector2 newSize)
    {
        EditorPad.Instance.ec.ReSize(newSize);

        mapTex.Release();
        mapTex.width = newSize.x * 16;
        mapTex.height = newSize.z * 16;
        mapTex.Create();

        mapRender.transform.position = (Vector2)EditorPad.Instance.ec.size / 2;
        Vector3 vector = mapRender.transform.position;
        vector.z = -1;
        vector.y -= 0.5f;
        vector.x -= 0.5f;
        mapRender.transform.position = vector;
        mapRender.orthographicSize = Mathf.Max(newSize.x, newSize.z) / 2;
    }

    public GameObject guide;
    public void ToggleGuide() => guide.SetActive(!guide.activeSelf);
}

using System.IO;
using TMPro;
using UnityEngine;

public class SaveEditor : Singleton<SaveEditor>
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

    public string levelName;
    public TMP_InputField nameField;
    public void TryToUpdateName()
    {
        if (!string.IsNullOrWhiteSpace(nameField.text))
        {
            levelName = nameField.text.Trim();
        }
    }

    public RenderTexture mapTex;
    string mapTexturePath => Path.Combine(Application.persistentDataPath, levelName + ".png");
    string mapAssetPath => Path.Combine(Application.persistentDataPath, levelName + ".mapAsset");
    public void SaveMap()
    {
        Debug.Log("Map Exported!");

        Texture2D tex = new Texture2D(mapTex.width, mapTex.height, TextureFormat.RGBA32, false, false);
        RenderTexture.active = mapTex;
        tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
        tex.Apply(false, false);
        tex.filterMode = FilterMode.Point;

        File.WriteAllBytes(mapTexturePath, tex.EncodeToPNG());
    }
    public void SaveAsset() => File.WriteAllText(mapAssetPath, JsonUtility.ToJson(EditorPad.Instance.ec.ConvertToAsset, true));
    public void LoadAsset()
    {
        if (File.Exists(mapAssetPath))
        {
            EditorPad.Instance.ec.Build(JsonUtility.FromJson<LevelAsset>(File.ReadAllText(mapAssetPath)));
        }
    }
}

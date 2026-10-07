using System.IO;
using TMPro;
using UnityEngine;

public class SaveEditor : Singleton<SaveEditor>
{
    public void Open()
    {
        EditorPad.Instance.Pause(true);
        gameObject.SetActive(true);
        if (string.IsNullOrWhiteSpace(levelName))
        {
            levelName = $"Level_{System.DateTime.Now.ToBinary()}";
        }
        nameField.text = levelName;
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
            levelName = nameField.text;
        }
    }

    public Camera mapRender => OptionEditor.Instance.mapRender;
    public RenderTexture mapTex => OptionEditor.Instance.mapTex;
    string mapTexturePath => Path.Combine(Application.persistentDataPath, $"Map_{levelName}.png");
    string lightmapTexturePath => Path.Combine(Application.persistentDataPath, $"Lightmap_{levelName}.png");
    string mapAssetPath => Path.Combine(Application.persistentDataPath, levelName + ".mapAsset");
    public void SaveMapAndLightmap()
    {
        Shader.DisableKeyword("_BG_REQUIRED");

        mapRender.enabled = true;
        mapRender.Render();
        mapRender.enabled = false;

        Texture2D tex = new Texture2D(mapTex.width, mapTex.height, TextureFormat.RGBA32, false, false);
        RenderTexture.active = mapTex;
        tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
        tex.Apply(false, false);
        tex.filterMode = FilterMode.Point;
        Shader.EnableKeyword("_BG_REQUIRED");

        File.WriteAllBytes(mapTexturePath, tex.EncodeToPNG());

        File.WriteAllBytes(lightmapTexturePath, EditorPad.Instance.ec.lightmap.EncodeToPNG());

        Application.OpenURL(Application.persistentDataPath);
    }
    public void SaveAsset()
    {
        File.WriteAllText(mapAssetPath, JsonUtility.ToJson(EditorPad.Instance.ec.ConvertToAsset, true));
        Application.OpenURL(Application.persistentDataPath);
    }
    public void LoadAsset()
    {
        EditorPad.Instance.Clear();
        EditorPad.Instance.Initialize();
        if (File.Exists(mapAssetPath))
        {
            Debug.Log($"{levelName} was founded! Loading");
            EditorPad.Instance.ec.Build(JsonUtility.FromJson<LevelAsset>(File.ReadAllText(mapAssetPath)));
            RoomEditor.Instance.UpdateTags();
            RoomEditor.Instance.Open(RoomEditor.Instance.tags[0]);
        }
        else
        {
            RoomEditor.Instance.CreateRoom();
        }
        Close();
    }
}

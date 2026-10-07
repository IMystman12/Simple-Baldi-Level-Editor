using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class LightmapEditor : Singleton<LightmapEditor>
{
    public Light light;
    public SpriteRenderer lightmapRenderer;

    public LightInstance lightPref;
    public List<LightInstance> lights = new List<LightInstance>();

    public LightInstance currentLight;

    public ColorField colorFieldLight, colorFieldDark;
    public TMP_InputField strengthField;
    public TMP_Text calculateModeText;

    bool regenerateAfterClose;
    bool rebuildLight;

    public void Open(LightInstance lightInstance)
    {
        EditorPad.Instance.Pause(true);
        gameObject.SetActive(true);
        currentLight = lightInstance;
        light = EditorPad.Instance.ec.CellFromPosition(lightInstance.position).light;

        colorFieldLight.color = light.color;
        colorFieldLight.ShowColor();

        colorFieldDark.color = EditorPad.Instance.ec.standardDark;
        colorFieldDark.ShowColor();

        strengthField.text = light.strength.ToString();
        calculateModeText.text = EditorPad.Instance.ec.lightCalculateMode.ToString();
    }

    public void ChangeLightStrength()
    {
        int val;
        if (int.TryParse(strengthField.text, out val))
        {
            light.strength = val;
            rebuildLight = true;
        }
        else
        {
            strengthField.text = light.strength.ToString();
        }
    }

    public void ChangeLightColor()
    {
        light.color = colorFieldLight.color;
        rebuildLight = true;
    }

    public void ChangeDarkColor()
    {
        EditorPad.Instance.ec.standardDark = colorFieldDark.color;
        regenerateAfterClose = true;
    }

    public void SwitchCalculateMode()
    {
        EditorPad.Instance.ec.lightCalculateMode++;
        if (EditorPad.Instance.ec.lightCalculateMode == LightCalculateMode.EnumLength)
        {
            EditorPad.Instance.ec.lightCalculateMode = 0;
        }
        calculateModeText.text = EditorPad.Instance.ec.lightCalculateMode.ToString();
        regenerateAfterClose = true;
    }

    public void Close()
    {
        gameObject.SetActive(false);
        EditorPad.Instance.Pause(false);
        if (rebuildLight)
        {
            DestroyLight(currentLight.position);
            CreateLight(currentLight.position);
        }
        if (regenerateAfterClose)
        {
            EditorPad.Instance.ec.RegenerateLight();
        }
        rebuildLight = false;
        regenerateAfterClose = false;
    }

    public void StartRender()
    {
        var lightmap = EditorPad.Instance.ec.lightmap;
        lightmapRenderer.sprite = Sprite.Create(lightmap, new Rect(0, 0, lightmap.width, lightmap.height), Vector2.zero, 1);
        lightmapRenderer.enabled = true;
    }
    public void CreateLight(Coordinate position)
    {
        var cell = EditorPad.Instance.ec.CellFromPosition(position);
        if (cell != null && !cell.hasLight)
        {
            var li = Instantiate(lightPref, Coordinate.ConvertToMapCoordinate(position), Quaternion.identity, EditorPad.Instance.ec.transform);
            li.position = cell.data.position;
            lights.Add(li);
            EditorPad.Instance.ec.GenerateLight(position, light);
        }
    }
    public void DestroyLight(Coordinate position)
    {
        var cell = EditorPad.Instance.ec.CellFromPosition(position);
        if (cell != null)
        {
            var l = lights.FirstOrDefault(a => a.position == cell.data.position);
            lights.Remove(l);
            if (l)
            {
                Destroy(l.gameObject);
            }
            EditorPad.Instance.ec.RemoveLight(position);
        }
    }
    public void StopRender()
    {
        lightmapRenderer.enabled = false;
    }
}

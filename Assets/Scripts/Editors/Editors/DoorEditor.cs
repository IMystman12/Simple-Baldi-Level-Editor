using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorEditor : MonoBehaviour
{
    public DoorTag currentTag;
    public void Open(DoorTag icon)
    {
        currentTag = icon;
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
            currentTag.door.spriteName = spriteSelector.result.name;
            currentTag.UpdateFromData();
        }
    }
    public void SetSprite()
    {
        spriteSelector.Open(gameObject);
        hasNewSprite = true;
    }

}

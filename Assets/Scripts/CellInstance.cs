using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellInstance : MonoBehaviour
{
    public bool autoConnect;
    public Room room;
    public Cell data;
    public SpriteRenderer rendererBase, rendererBG;
    public void ChangeColor()
    {
        rendererBase.color = room.color;
        rendererBG.color = room.color;
        Color color = rendererBG.color;
        color.a = 0.25f;
        rendererBG.color = color;
    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SpriteSelector : Singleton<SpriteSelector>
{
    [SerializeField] private Sprite[] sprites = new Sprite[0];
    public Sprite Selection => selectionIndex < 0 || selectionIndex >= sprites.Length ? null : sprites[selectionIndex];
    public Toggle togglePref;
    public List<Toggle> toggles = new List<Toggle>();
    public int page, togglePerPage = 15, selectionIndex, maxPage;
    const float padding = 32 + 5;
    public void MovePage(int dir)
    {
        page += dir;
        if (page < 0)
        {
            page = maxPage - 1;
        }
        if (page >= maxPage)
        {
            page = 0;
        }
        toggles.ForEach(a => a.transform.localPosition = togglePref.transform.localPosition + new Vector3(padding * 3, 0, 0));
        for (int i = page * togglePerPage, j = 0, i0 = Mathf.Min((page + 1) * togglePerPage, toggles.Count); i < i0; i++, j++)
        {
            toggles[i].transform.localPosition = togglePref.transform.localPosition + new Vector3(padding * (j % 3), -padding * Mathf.FloorToInt(j / 3), 0);
        }
        CheckValue();
    }

    public void TryToSetValue(string spriteName)
    {
        var toggle = toggles.FirstOrDefault(a => a.image.sprite.name == spriteName);
        if (toggle)
        {
            toggle.isOn = true;
        }
        CheckValue();
    }
    public UnityEvent onCheckValue;
    public void CheckValue()
    {
        selectionIndex = toggles.IndexOf(toggles.FirstOrDefault(a => a.isOn));
        onCheckValue?.Invoke();
    }

    public void Open(Categories.Category category)
    {
        gameObject.SetActive(true);
        switch (category)
        {
            case Categories.Category.Room:
                sprites = rooms;
                break;
            case Categories.Category.Cell:
                sprites = cells;
                break;
            case Categories.Category.Icon:
                sprites = icons;
                break;
            case Categories.Category.Door:
                sprites = doors;
                break;
            case Categories.Category.Generator:
                sprites = generators;
                break;
        }
        for (int i = 0; i < toggles.Count; i++)
        {
            Destroy(toggles[i].gameObject);
        }
        toggles.Clear();
        Toggle t;
        for (int i = 0, i0 = sprites.Length; i < i0; i++)
        {
            t = Instantiate(togglePref, togglePref.transform.parent);
            t.image.sprite = sprites[i];
            t.gameObject.SetActive(true);
            toggles.Add(t);
        }
        page = 0;
        maxPage = Mathf.CeilToInt((float)toggles.Count / togglePerPage);
        MovePage(0);
    }
    public void Close()
    {
        onCheckValue?.RemoveAllListeners();
        gameObject.SetActive(false);
    }

    public Sprite[] rooms = new Sprite[8];
    public Sprite[] cells = new Sprite[17];
    public Sprite[] icons = new Sprite[17];
    public Sprite[] doors = new Sprite[17];
    public Sprite[] generators = new Sprite[2];
}

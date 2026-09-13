using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SpriteSelector : MonoBehaviour
{
    public Sprite[] sprites = new Sprite[0];
    public Toggle togglePref;
    public List<Toggle> toggles = new List<Toggle>();
    public UnityEvent<int> checkValue;

    public int page, togglePerPage = 15;
    public void MovePage(int dir)
    {
        page += dir;
        int maxPage = Mathf.CeilToInt((float)toggles.Count / togglePerPage);
        if (page < 0)
        {
            page = maxPage - 1;
        }
        if (page >= maxPage)
        {
            page = 0;
        }
        toggles.ForEach(a => a.gameObject.SetActive(false));
        for (int i = page * togglePerPage, i0 = Mathf.Min((page + 1) * togglePerPage, toggles.Count); i < i0; i++)
        {
            toggles[i].gameObject.SetActive(true);
        }
        CheckValue();
    }

    public void CheckValue()
    {
        int result = toggles.IndexOf(toggles.First(a => a.isOn && a.gameObject.activeSelf));
        if (result != -1)
        {
            checkValue.Invoke(result);
        }
    }

    public void Open(Categories.Category category)
    {
        gameObject.SetActive(true);
        switch (category)
        {
            case Categories.Category.None:
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
            case Categories.Category.Options:
                break;
            case Categories.Category.Save:
                break;
            case Categories.Category.Multiplayer:
                break;
        }
        while (toggles.Count > 0)
        {
            Destroy(toggles[0].gameObject);
            toggles.RemoveAt(0);
        }
        Toggle t;
        for (int i = 0, i0 = sprites.Length; i < i0; i++)
        {
            t = Instantiate(togglePref, togglePref.transform.parent);
            t.image.sprite = sprites[i];
            toggles.Add(t);
        }
        page = 0;
        MovePage(0);
    }
    public void Close()
    {
        gameObject.SetActive(false);
    }

    public Sprite[] cells = new Sprite[17];
    public Sprite[] icons = new Sprite[17];
    public Sprite[] doors = new Sprite[17];
    public Sprite[] generators = new Sprite[2];
}

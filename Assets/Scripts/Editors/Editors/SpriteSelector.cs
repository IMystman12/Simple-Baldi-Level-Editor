using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SpriteSelector : MonoBehaviour
{
    public Sprite result;
    public Sprite[] sprites => EditorPad.Instance.sprites;
    public Toggle togglePref;
    public List<Toggle> toggles = new List<Toggle>();

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

    public void OnEnable()
    {
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

    public void CheckValue() => result = toggles.First(a => a.isOn && a.gameObject.activeSelf).image.sprite;

    public GameObject source;
    public void Open(GameObject source)
    {
        source.gameObject.SetActive(false);
        gameObject.SetActive(true);
        this.source = source;
    }
    public void Close()
    {
        gameObject.SetActive(false);
        source.gameObject.SetActive(true);
    }
}

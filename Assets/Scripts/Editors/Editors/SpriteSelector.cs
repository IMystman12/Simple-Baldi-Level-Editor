using System;
using System.Collections.Generic;
using System.IO;
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
    const string folderName = "Sprite";
    public void OpenFolder() => Application.OpenURL(Path.Combine(Application.persistentDataPath, folderName));
    public void CheckSpriteFolder()
    {
        string s = Path.Combine(Application.persistentDataPath, folderName), s0;
        if (!Directory.Exists(s))
        {
            Directory.CreateDirectory(s);
        }
        Categories.Category[] folders = new Categories.Category[]
     {
            Categories.Category.Room,
            Categories.Category.Icon,
            Categories.Category.Door
     };
        for (int i = 0; i < folders.Length; i++)
        {
            s0 = Path.Combine(s, folders[i].ToString());
            if (!Directory.Exists(s0))
            {
                Directory.CreateDirectory(s0);
                continue;
            }
            foreach (var a in Directory.GetFiles(s0, "*", SearchOption.AllDirectories))
            {
                ImportSpriteFromPath(a, folders[i]);
            }
        }
    }
    //skill issue + hard to maintain!
    public void ImportExtraSprite(ExtraSprite extraSprite)
    {
        if (extraSprites.Any(a => a.name == extraSprite.name))
        {
            return;
        }
        string s = Path.Combine(Application.persistentDataPath, folderName, extraSprite.category.ToString(), $"{extraSprite.name}.png");
        File.WriteAllBytes(s, extraSprite.bytes);
        ImportSpriteFromPath(s, extraSprite.category);
    }
    void ImportSpriteFromPath(string a, Categories.Category category)
    {
        var nam = Path.GetFileNameWithoutExtension(a);
        if (string.IsNullOrWhiteSpace(nam))
        {
            return;
        }
        var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Repeat
        };
        if (tex.LoadImage(File.ReadAllBytes(a)))
        {
            var spr = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f, category != Categories.Category.Room ? Mathf.Min(tex.width, tex.height) : 1);
            spr.name = nam;
            extraSprites.Add(spr);
            switch (category)
            {
                case Categories.Category.Room:
                    rooms.Add(spr);
                    break;
                case Categories.Category.Icon:
                    icons.Add(spr);
                    break;
                case Categories.Category.Door:
                    doors.Add(spr);
                    break;
            }
            return;
        }
        Destroy(tex);
    }
    public bool GetExtraSprite(string nam, out Sprite sprite) => (sprite = extraSprites.FirstOrDefault(a => a.name == nam)) != null;
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
                sprites = rooms.ToArray();
                break;
            case Categories.Category.Cell:
                sprites = cells;
                break;
            case Categories.Category.Icon:
                sprites = icons.ToArray();
                break;
            case Categories.Category.Door:
                sprites = doors.ToArray();
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

    public List<Sprite> rooms = new List<Sprite>();
    public Sprite[] cells = new Sprite[17];
    public List<Sprite> icons = new List<Sprite>();
    public List<Sprite> doors = new List<Sprite>();
    public List<Sprite> extraSprites = new List<Sprite>();
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class EnvironmentController : MonoBehaviour
{
    public LevelAsset ConvertToAsset => new LevelAsset()
    {
        size = size,
        rooms = rooms.ToArray(),
        icons = icons.ToArray(),
        lights = lights.ToArray(),
        extraSprites = CollectExtraSprites()
    };

    ExtraSprite[] CollectExtraSprites()
    {
        var lookup = new HashSet<(Categories.Category, string)>();
        List<ExtraSprite> extraSprites = new List<ExtraSprite>();
        string n;
        Sprite sprite;
        for (int i = 0; i < rooms.Count; i++)
        {
            n = rooms[i].mapBGName;
            if (lookup.Add((Categories.Category.Room, n)) && SpriteSelector.Instance.GetExtraSprite(n, out sprite))
            {
                extraSprites.Add(new ExtraSprite()
                {
                    category = Categories.Category.Room,
                    name = n,
                    bytes = sprite.texture.EncodeToPNG()
                });
            }

            for (int j = 0; j < rooms[i].doors.Count; j++)
            {
                n = rooms[i].doors[j].spriteName;
                if (lookup.Add((Categories.Category.Door, n)) && SpriteSelector.Instance.GetExtraSprite(n, out sprite))
                {
                    extraSprites.Add(new ExtraSprite()
                    {
                        category = Categories.Category.Door,
                        name = n,
                        bytes = sprite.texture.EncodeToPNG()
                    });
                }
            }
        }

        for (int i = 0; i < icons.Count; i++)
        {
            n = icons[i].spriteName;
            if (lookup.Add((Categories.Category.Icon, n)) && SpriteSelector.Instance.GetExtraSprite(n, out sprite))
            {
                extraSprites.Add(new ExtraSprite()
                {
                    category = Categories.Category.Icon,
                    name = n,
                    bytes = sprite.texture.EncodeToPNG()
                });
            }
        }

        return extraSprites.ToArray();
    }

    public void Build(LevelAsset asset)
    {
        Resize(asset.size);

        {
            Room roomCopied, roomCreated;
            Cell cellCopied;
            Door doorCopied;
            DoorInstance doorCreated;
            for (int i = 0; i < asset.rooms.Length; i++)
            {
                roomCopied = asset.rooms[i];
                roomCreated = CreateRoom(roomCopied.color);
                roomCreated.name = roomCopied.name;
                roomCreated.mapBGName = roomCopied.mapBGName;
                for (int j = 0; j < roomCopied.cells.Count; j++)
                {
                    cellCopied = roomCopied.cells[j];
                    CreateCell(cellCopied.position, roomCreated, cellCopied.id);
                }
                for (int j = 0; j < roomCopied.doors.Count; j++)
                {
                    doorCopied = roomCopied.doors[j];
                    doorCreated = CreateDoor(doorCopied.position, doorCopied.direction, null);
                    doorCopied.CopyTo(doorCreated.door);
                    doorCreated.UpdateFromData();
                }
            }
        }

        RoomEditor.Instance.UpdateTags();

        if (EditorPad.Instance)
        {
            Icon iconCopied;
            IconInstance icon;
            for (int i = 0; i < asset.icons.Length; i++)
            {
                iconCopied = asset.icons[i];
                icon = EditorPad.Instance.CreateIcon(default, null);
                iconCopied.CopyTo(icon.icon);
                icon.UpdateFromData();
            }
            if (LightmapEditor.Instance)
            {
                Light lightCopied;
                for (int i = 0; i < asset.lights.Length; i++)
                {
                    lightCopied = asset.lights[i];
                    LightmapEditor.Instance.light = lightCopied;
                    LightmapEditor.Instance.CreateLight(lightCopied.position);
                }
            }
        }
    }

    public Coordinate realSize;
    public Coordinate size;
    public void Resize(Coordinate sizeNew)
    {
        size = sizeNew;
        realSize = sizeNew - Coordinate.one;
        RegenerateLight();

        OptionEditor.Instance?.UpdateSize(sizeNew);
    }

    public Sprite[] cellSprites = new Sprite[16];
    public CellInstance cellPref;
    public Dictionary<Coordinate, CellInstance> cells = new Dictionary<Coordinate, CellInstance>();

    public Texture2D lightmap;
    public LightCalculateMode lightCalculateMode;
    public Color standardDark = Color.black;
    public List<Light> lights = new List<Light>();
    public void GenerateLight(Coordinate position, Light lightToCopy)
    {
        var cell = CellFromPosition(position);
        if (!cell || cell.hasLight)
        {
            return;
        }

        var light = new Light();
        if (lightToCopy != null)
        {
            lightToCopy.CopyTo(light);
        }
        light.position = position;
        lights.Add(light);

        cell.light = light;
        cell.hasLight = true;
        cell.cost = 0;

        CellInstance pivot;
        List<CellInstance> visited = new List<CellInstance>();
        List<CellInstance> pends = new List<CellInstance>() { cell };
        while (pends.Count > 0)
        {
            pivot = pends[0];

            pends.RemoveAt(0);
            visited.Add(pivot);

            cell.cellsWithLight.Add(pivot);
            pivot.lights.Add(new LightWithDistance() { light = light, distance = pivot.cost });
            pivot.CalculateLight();

            foreach (var a in GetNeighbors(pivot, false, true))
            {
                if (!visited.Contains(a) && !pends.Contains(a) && Coordinate.Distance(a.data.position, position) < (light.strength + 1))
                {
                    pends.Add(a);
                    a.cost = pivot.cost + 1;
                }
            }
        }
        if (pends.Count > 0)
        {
            Debug.LogWarning("Light setup was out of attempts! Pends: " + pends.Count);
        }


        lightmap.Apply(false, false);
    }
    public void RemoveLight(Coordinate position)
    {
        var cell = CellFromPosition(position);
        if (!cell || !cell.hasLight)
        {
            return;
        }

        cell.hasLight = false;
        while (cell.cellsWithLight.Count > 0)
        {
            cell.cellsWithLight[0].lights.Remove(cell.cellsWithLight[0].lights.FirstOrDefault(a => a.light == cell.light));
            cell.cellsWithLight[0].CalculateLight();
            cell.cellsWithLight.RemoveAt(0);
        }
        lights.Remove(cell.light);

        lightmap.Apply(false, false);
    }
    public void RegenerateLight()
    {
        lightmap = new Texture2D(size.x, size.z, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
        var c = lightmap.GetPixels();
        for (int i = 0; i < c.Length; i++)
        {
            c[i] = standardDark;
        }
        lightmap.SetPixels(c);

        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.z; j++)
            {
                CellFromPosition(new Coordinate(i, j))?.CalculateLight();
            }
        }

        lightmap.Apply(false, false);
    }

    public DoorInstance doorPref;
    public List<DoorInstance> doors = new List<DoorInstance>();
    public DoorInstance CreateDoor(Coordinate position, Towards direction, Sprite sprite)
    {
        var cell = CellFromPosition(position);
        if (cell)
        {
            if (cell.data.GetRoom(this).doors.FirstOrDefault((a) => a.position == position && a.direction == direction) != null)
            {
                return null;
            }

            var cellB = CellFromPosition(position + direction.GetRelativeCoordinate());
            if (cellB)
            {
                ConnectCell(cell, cellB);
            }

            var door = Instantiate(doorPref, (Vector2)position, direction.GetUIRotation(), transform);

            var doorData = door.door;
            doorData.spriteName = sprite ? sprite.name : "Icon_Door_Open";
            doorData.position = position;
            doorData.direction = direction;

            door.UpdateFromData();

            cell.data.GetRoom(this).doors.Add(doorData);
            doors.Add(door);
            return door;
        }
        return null;
    }
    public void DestroyDoor(Coordinate position, Towards direction)
    {
        var cell = CellFromPosition(position);
        if (cell)
        {
            var cellB = CellFromPosition(position + direction.GetRelativeCoordinate());
            if (!cellB)
            {
                return;
            }

            if (cell.room != cellB.room)
            {
                ConnectCell(cell, cellB, false);
            }

            var room = cell.data.GetRoom(this);
            var door = doors.FirstOrDefault((a) => a.door.position == position && a.door.direction == direction);
            if (door != null)
            {
                doors.Remove(door);
                room?.doors.Remove(door.door);
                Destroy(door.gameObject);
            }
        }
    }

    public List<Room> rooms = new List<Room>();
    public List<Icon> icons = new List<Icon>();
    public CellInstance CreateCell(Coordinate position, Room room, int id = 15)
    {
        if (ContainsCoordinates(position) && !CellFromPosition(position))
        {
            CellInstance cellInstance;
            cellInstance = Instantiate(cellPref, (Vector2)position, Quaternion.identity, transform);
            cellInstance.data.position = position;
            cellInstance.room = room;
            cellInstance.room.cells.Add(cellInstance.data);
            cellInstance.ChangeColor();
            cellInstance.data.id = id;
            cellInstance.rendererBase.sprite = cellSprites[id];
            cellInstance.ec = this;
            if (cells.ContainsKey(position))
            {
                cells[position] = cellInstance;
            }
            else
            {
                cells.Add(position, cellInstance);
            }
            return cellInstance;
        }
        return null;
    }
    public void ConnectCell(CellInstance cellA, CellInstance cellB, bool connect = true)
    {
        if (cellA & cellB)
        {
            Towards dir = TowardsExtension.FromPointAToB(cellA.data.position, cellB.data.position);
            if (TowardsExtension.OpenTowardsFromBin(cellA.data.id).Contains(dir) != connect)
            {
                cellA.data.id -= dir.GetBinary() * (connect ? 1 : -1);
                cellA.rendererBase.sprite = cellSprites[cellA.data.id];
            }

            dir = dir.GetOpposite();
            if (TowardsExtension.OpenTowardsFromBin(cellB.data.id).Contains(dir) != connect)
            {
                cellB.data.id -= dir.GetBinary() * (connect ? 1 : -1);
                cellB.rendererBase.sprite = cellSprites[cellB.data.id];
            }
        }
    }
    public void ConnectSurround(CellInstance cellA, bool connect = true)
    {
        if (!cellA)
        {
            return;
        }
        foreach (var a in GetNeighbors(cellA, true, false))
        {
            ConnectCell(cellA, a, connect);
        }
    }
    public CellInstance GetNeighbor(CellInstance cellA, Towards direction)
    {
        if (cellA)
        {
            Coordinate intVector = cellA.data.position + direction.GetRelativeCoordinate();
            return CellFromPosition(intVector);
        }
        return null;
    }
    public List<CellInstance> GetNeighbors(CellInstance cellA, bool matchRoom = false, bool passible = true)
    {
        if (cellA)
        {
            List<CellInstance> list = new List<CellInstance>();
            CellInstance cellInstance;
            foreach (var a in passible ? TowardsExtension.OpenTowardsFromBin(cellA.data.id) : TowardsExtension.All)
            {
                cellInstance = GetNeighbor(cellA, a);
                if (cellInstance && (cellA.room == cellInstance.room || !matchRoom))
                {
                    list.Add(cellInstance);
                }
            }
            return list;
        }
        return null;
    }
    public Room CreateRoom(Color color)
    {
        rooms.Add(new Room() { color = color });
        return rooms[rooms.Count - 1];
    }
    public void DestroyRoom(Room room)
    {
        rooms.Remove(room);
        foreach (var a in room.cells)
        {
            DestroyCell(CellFromPosition(a.position));
        }
        DoorInstance doorInstance;
        foreach (var a in room.doors)
        {
            doorInstance = doors.FirstOrDefault(b => b.door == a);
            if (doorInstance)
            {
                Destroy(doorInstance.gameObject);
            }
        }
    }

    public void DestroyCell(CellInstance cellA)
    {
        if (!cellA)
        {
            return;
        }
        ConnectSurround(cellA, false);
        cellA.room.cells.Remove(cellA.data);
        Destroy(cellA.gameObject);
    }
    public bool ContainsCoordinates(Coordinate vector) => vector.x >= 0 && vector.z >= 0 && vector.x < size.x && vector.z < size.z;
    public CellInstance CellFromPosition(Coordinate vector)
    {
        Coordinate vectorA = Coordinate.ConvertToGridCoordinate(vector);
        if (ContainsCoordinates(vector) && cells.ContainsKey(vectorA))
        {
            return cells[vectorA];
        }
        return null;
    }


    public List<CellInstance> FindPath(Coordinate a, Coordinate b)
    {
        var start = CellFromPosition(a);
        if (!start)
        {
            return null;
        }

        var sw = Stopwatch.StartNew();

        CellInstance pivot = start;

        List<CellInstance> visited = new List<CellInstance>();
        List<CellInstance> pends = new List<CellInstance>() { start };

        while (pends.Count > 0 && pivot.data.position != b)
        {
            pivot = pends[0];
            pends.RemoveAt(0);
            visited.Add(pivot);

            foreach (var c in GetNeighbors(pivot, false, true))
            {
                if (!visited.Contains(c) && !pends.Contains(c))
                {
                    pends.Add(c);
                    c.parent = pivot;
                }
            }
        }

        int attempts = 100;
        var end = pivot;
        List<CellInstance> result = new List<CellInstance>() { end };
        while (pivot != start && attempts > 0)
        {
            attempts--;
            pivot = pivot.parent;
            result.Add(pivot);
        }
        if (pivot != start)
        {
            Debug.LogWarning("Path packing was out of attempts!");
        }
        result.Reverse();
        Debug.Log("Found path success in " + sw.ElapsedMilliseconds + " " + result.Count);
        return result;
    }
}

[Serializable]
public class Room
{
    public Color color = Color.white;
    public string name = "Room", mapBGName = "Transparent";
    public List<Cell> cells = new List<Cell>();
    public List<Door> doors = new List<Door>();
    public void ChangeColor(EnvironmentController ec)
    {
        for (int i = 0; i < cells.Count; i++)
        {
            ec.CellFromPosition(cells[i].position)?.ChangeColor();
        }
        for (int i = 0; i < doors.Count; i++)
        {
            ec.doors.FirstOrDefault(a => a.door == doors[i])?.UpdateFromData();
        }
    }
}
[Serializable]
public class Cell
{
    public int id = 16;
    public Coordinate position;
    public Light light;
    public Room GetRoom(EnvironmentController ec)
    {
        foreach (var a in ec.rooms)
        {
            foreach (var b in a.cells)
            {
                if (b == this)
                {
                    return a;
                }
            }
        }
        return null;
    }
}
[Serializable]
public class LevelAsset
{
    public Coordinate size;
    public Room[] rooms = new Room[0];
    public Icon[] icons = new Icon[0];
    public Light[] lights = new Light[0];
    public ExtraSprite[] extraSprites = new ExtraSprite[0];
}
[Serializable]
public class ExtraSprite
{
    public Categories.Category category;
    public string name;
    public byte[] bytes;
}
[Serializable]
public class Icon
{
    public Vector2 position;
    public float rotation;

    public string spriteName;
    public Color color = Color.white;

    public void CopyTo(Icon dest)
    {
        dest.position = position;
        dest.rotation = rotation;
        dest.spriteName = spriteName;
        dest.color = color;
    }
}
[Serializable]
public class Door
{
    public Coordinate position;
    public Towards direction;

    public string spriteName;

    public void CopyTo(Door dest)
    {
        dest.position = position;
        dest.direction = direction;
        dest.spriteName = spriteName;
    }
}
[Serializable]
public class Light
{
    public Coordinate position;
    public Color color = Color.white;
    public int strength = 5;
    public void CopyTo(Light dest)
    {
        dest.color = color;
        dest.strength = strength;
    }
}
public enum LightCalculateMode
{
    Cumulative,
    Additive,
    Greatest,
    EnumLength
}
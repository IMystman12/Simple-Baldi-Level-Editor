using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnvironmentController : MonoBehaviour
{
    public LevelAsset ConvertToAsset => new LevelAsset()
    {
        size = size,
        rooms = rooms.ToArray(),
        icons = icons.ToArray()
    };

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
            IconTag icon;
            for (int i = 0; i < asset.icons.Length; i++)
            {
                iconCopied = asset.icons[i];
                icon = EditorPad.Instance.CreateIcon(default, null);
                iconCopied.CopyTo(icon.icon);
                icon.UpdateFromData();
            }
        }
    }

    public IntVector2 realSize;
    public IntVector2 size;
    public void Resize(IntVector2 sizeNew)
    {
        size = sizeNew;
        realSize = sizeNew - IntVector2.one;
        OptionEditor.Instance?.UpdateSize(sizeNew);
    }
    public Sprite[] cellSprites = new Sprite[16];
    public CellInstance cellPref;

    public Dictionary<IntVector2, CellInstance> cells = new Dictionary<IntVector2, CellInstance>();

    public DoorInstance doorPref;
    public List<DoorInstance> doors = new List<DoorInstance>();
    public DoorInstance CreateDoor(IntVector2 position, Direction direction, Sprite sprite)
    {
        var cell = CellFromPosition(position);
        if (cell)
        {
            if (cell.data.GetRoom(this).doors.FirstOrDefault((a) => a.position == position && a.direction == direction) != null)
            {
                return null;
            }

            var cellB = CellFromPosition(position + direction.ToIntVector2());
            if (cellB)
            {
                ConnectCell(cell, cellB);
            }

            var door = Instantiate(doorPref, (Vector2)position, direction.ToUiRotation(), transform);

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
    public void DestroyDoor(IntVector2 position, Direction direction)
    {
        var cell = CellFromPosition(position);
        if (cell)
        {
            var cellB = CellFromPosition(position + direction.ToIntVector2());
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
    public CellInstance CreateCell(IntVector2 position, Room room, int id = 15)
    {
        if (ContainsCoordinates(position))
        {
            CellInstance cellInstance;
            if (!CellFromPosition(position))
            {
                cellInstance = Instantiate(cellPref, (Vector2)position, Quaternion.identity, transform);
                cellInstance.data.position = position;
                cellInstance.room = room;
                cellInstance.room.cells.Add(cellInstance.data);
                cellInstance.ChangeColor();
                cellInstance.data.id = id;
                cellInstance.rendererBase.sprite = cellSprites[id];
            }
            else
            {
                cellInstance = CellFromPosition(position);
                cellInstance.room.cells.Remove(cellInstance.data);
                cellInstance.room = room;
                cellInstance.room.cells.Add(cellInstance.data);
                cellInstance.ChangeColor();
                cellInstance.data.id = id;
                cellInstance.rendererBase.sprite = cellSprites[id];
            }

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
            Direction dir = Directions.FromPointAToB(cellA.data.position, cellB.data.position);
            if (Directions.OpenDirectionsFromBin(cellA.data.id).Contains(dir) != connect)
            {
                cellA.data.id -= dir.ToBinary() * (connect ? 1 : -1);
                cellA.rendererBase.sprite = cellSprites[cellA.data.id];
            }
            dir = dir.GetOpposite();
            if (Directions.OpenDirectionsFromBin(cellB.data.id).Contains(dir) != connect)
            {
                cellB.data.id -= dir.ToBinary() * (connect ? 1 : -1);
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
        CellInstance cellInstance;
        foreach (var item in Directions.All)
        {
            cellInstance = GetNeighbor(cellA, item);
            if (cellInstance && cellA.room == cellInstance.room)
            {
                ConnectCell(cellA, cellInstance, connect);
            }
        }
    }
    public CellInstance GetNeighbor(CellInstance cellA, Direction direction)
    {
        if (cellA)
        {
            IntVector2 intVector = cellA.data.position + direction.ToIntVector2();
            return CellFromPosition(intVector);
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
        Destroy(cellA.gameObject);
    }
    public bool ContainsCoordinates(IntVector2 vector) => vector.x >= 0 && vector.z >= 0 && vector.x < size.x && vector.z < size.z;
    public CellInstance CellFromPosition(IntVector2 vector)
    {
        IntVector2 vectorA = IntVector2.GetGridPosition(vector);
        if (ContainsCoordinates(vector) && cells.ContainsKey(vectorA))
        {
            return cells[vectorA];
        }
        return null;
    }
}

[Serializable]
public class Room
{
    public string name = "Room", mapBGName = "Transparent";
    public Color color = Color.white;
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
    public IntVector2 position;
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
    public IntVector2 size;
    public Room[] rooms = new Room[0];
    public Icon[] icons = new Icon[0];
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
    public IntVector2 position;
    public Direction direction;

    public string spriteName;

    public void CopyTo(Door dest)
    {
        dest.position = position;
        dest.direction = direction;
        dest.spriteName = spriteName;
    }
}
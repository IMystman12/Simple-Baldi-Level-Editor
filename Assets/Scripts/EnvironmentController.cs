using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnvironmentController : MonoBehaviour
{
    public LevelAsset ConvertToAsset
    {
        get
        {
            return null;
        }
    }

    public void Build(LevelAsset asset)
    {

    }

    public IntVector2 realSize;
    public IntVector2 size;
    public Sprite[] cellSprites = new Sprite[16];
    public CellInstance cellPref;

    public CellInstance[,] cells;

    public DoorInstance doorPref;
    public List<DoorInstance> doors = new List<DoorInstance>();
    public void CreateDoor(IntVector2 position, Direction direction, Sprite sprite)
    {
        var cell = CellFromPosition(position);
        if (cell)
        {
            if (cell.data.GetRoom(this).doors.FirstOrDefault((a) => a.position == position && a.direction == direction) != null)
            {
                return;
            }

            var cellB = CellFromPosition(position + direction.ToIntVector2());
            if (!cellB)
            {
                return;
            }

            ConnectCell(cell, cellB);

            var door = Instantiate(doorPref, (Vector2)position, direction.ToUiRotation(), transform);

            var doorData = door.door;
            doorData.spriteName = sprite.name;
            doorData.position = position;
            doorData.direction = direction;

            door.UpdateFromData();

            cell.data.GetRoom(this).doors.Add(doorData);
            doors.Add(door);
        }
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
            var door = room.doors.FirstOrDefault((a) => a.position == position && a.direction == direction);
            if (door != null)
            {
                var doorInst = doors.FirstOrDefault(a => a.door == door);
                if (doorInst)
                {
                    doors.Remove(doorInst);
                    Destroy(doorInst.gameObject);
                }

                room.doors.Remove(door);
            }
        }
    }

    public List<Room> rooms = new List<Room>();
    public List<Icon> icons = new List<Icon>();
    private void Start()
    {
        Initialize(new IntVector2(32, 32));
    }
    public void Initialize(IntVector2 size)
    {
        ReSize(size);
    }
    public void ReSize(IntVector2 size)
    {
        this.size = size;
        realSize = size - IntVector2.one;
        CellInstance[,] overrode = new CellInstance[size.x, size.z];
        List<CellInstance> cellsSaved = new List<CellInstance>();
        if (cells != null)
        {
            int ae = Mathf.Min(size.x, cells.GetLength(0)), be = Mathf.Min(size.z, cells.GetLength(1));
            for (int a = 0; a < ae; a++)
            {
                for (int b = 0; b < be; b++)
                {
                    overrode[a, b] = cells[a, b];
                    cellsSaved.Add(cells[a, b]);
                }
            }
            for (int a = 0; a < cells.GetLength(0); a++)
            {
                for (int b = 0; b < cells.GetLength(1); b++)
                {
                    if (!cellsSaved.Contains(cells[a, b]))
                    {
                        DestroyCell(cells[a, b]);
                    }
                }
            }
        }
        cells = overrode;
    }
    public CellInstance CreateCell(IntVector2 position, Room room, int id = 15)
    {
        if (ContainsCoordinates(position))
        {
            if (!CellFromPosition(position))
            {
                var cellInstance = Instantiate(cellPref, (Vector2)position, Quaternion.identity, transform);
                cells[position.x, position.z] = cellInstance;
                cellInstance.data.position = position;
                cellInstance.room = room;
                cellInstance.room.cells.Add(cellInstance.data);
                cellInstance.ChangeColor();
                cellInstance.data.id = id;
                cellInstance.rendererBase.sprite = cellSprites[id];
                return cellInstance;
            }
            else
            {
                var cellInstance = CellFromPosition(position);
                cellInstance.room.cells.Remove(cellInstance.data);
                cellInstance.room = room;
                cellInstance.room.cells.Add(cellInstance.data);
                cellInstance.ChangeColor();
                cellInstance.data.id = id;
                cellInstance.rendererBase.sprite = cellSprites[id];
                return cellInstance;
            }
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
        foreach (var item in room.cells)
        {
            DestroyCell(CellFromPosition(item.position));
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
        if (ContainsCoordinates(vector))
        {
            return cells[vectorA.x, vectorA.z];
        }
        return null;
    }
}

[Serializable]
public class Room
{
    public string name = "Room";
    public Color color = Color.white;
    public List<Cell> cells = new List<Cell>();
    public List<Door> doors = new List<Door>();
    public void ChangeColor(EnvironmentController ec)
    {
        for (int i = 0; i < cells.Count; i++)
        {
            ec.CellFromPosition(cells[i].position)?.ChangeColor();
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
}
[Serializable]
public class Door
{
    public IntVector2 position;
    public Direction direction;

    public string spriteName;
}
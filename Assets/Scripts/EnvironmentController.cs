using System;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentController : MonoBehaviour
{
    public IntVector2 realSize;
    public IntVector2 size;
    public Sprite[] cellSprites = new Sprite[16];
    public CellInstance cellPref, cellInstance;
    public CellInstance[,] cells;
    public List<Room> rooms = new List<Room>();
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
        realSize = size + IntVector2.one * 5;
        CellInstance[,] overrided = new CellInstance[size.x, size.z];
        if (cells != null)
        {
            int ae = Mathf.Min(size.x, cells.GetLength(0)), be = Mathf.Min(size.z, cells.GetLength(1));
            for (int a = 0; a < ae; a++)
            {
                for (int b = 0; b < be; b++)
                {
                    overrided[a, b] = cells[a, b];
                }
            }
        }
        cells = overrided;
    }
    public CellInstance CreateCell(IntVector2 position, Room room, int id = 15)
    {
        if (ContainsRange(position))
        {
            if (!CellFromPosition(position))
            {
                cellInstance = Instantiate(cellPref, (Vector2)position, Quaternion.identity, transform);
                cells[position.x, position.z] = cellInstance;
                cellInstance.data.position = position;
                cellInstance.room = room;
                room.cells.Add(cellInstance.data);
                cellInstance.ChangeColor();
                cellInstance.data.id = id;
                cellInstance.rendererBase.sprite = cellSprites[id];
                return cellInstance;
            }
            else
            {
                cellInstance = CellFromPosition(position);
                room.cells.Remove(cellInstance.data);
                cellInstance.room = room;
                room.cells.Add(cellInstance.data);
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
        foreach (var item in Directions.All)
        {
            cellInstance = GetNeighbor(cellA, item);
            if (cellA.autoConnect && cellInstance && cellInstance.autoConnect && cellA.room == cellInstance.room)
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
        if (cellA.autoConnect)
        {
            ConnectSurround(cellA, false);
        }
        Destroy(cellA.gameObject);
    }

    public IntVector2 GetGridPosition(Vector2 position)
    {
        return new IntVector2(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));
    }
    public bool ContainsRange(IntVector2 vector)
    {
        return vector.x >= 0 && vector.z >= 0 && vector.x < size.x && vector.z < size.z;
    }
    public CellInstance CellFromPosition(IntVector2 vector)
    {
        IntVector2 vectorA = GetGridPosition(vector);
        if (ContainsRange(vector))
        {
            return cells[vectorA.x, vectorA.z];
        }
        return null;
    }
}

[Serializable]
public class Room
{
    public Color color;
    public List<Cell> cells = new List<Cell>();
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
}
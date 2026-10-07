using System;
using UnityEngine;
[Serializable]
public struct Coordinate
{
    public int x;
    public int z;
    public Coordinate(int Nx, int Ny)
    {
        x = Nx;
        z = Ny;
    }
    public int this[int index]
    {
        get
        {
            switch (index)
            {
                case 0:
                    return x;
                case 1:
                    return z;
                default:
                    throw new IndexOutOfRangeException();
            }
        }
        set
        {
            switch (index)
            {
                case 0:
                    x = value;
                    break;
                case 1:
                    z = value;
                    break;
                default:
                    throw new IndexOutOfRangeException($"{index}");
            }
        }
    }
    public static int Distance(Coordinate a, Coordinate b) => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.z - b.z);
    public static Coordinate ConvertToGridCoordinate(Vector2 coordinate) => new Coordinate(Mathf.RoundToInt(coordinate.x), Mathf.RoundToInt(coordinate.y));
    public static Vector2 ConvertToMapCoordinate(Coordinate coordinate) => coordinate;
    public override string ToString() => $"{x},{z}";
    public static Coordinate one => new Coordinate(1, 1);
    public static implicit operator Vector2(Coordinate coordinate) => new Vector2(coordinate.x, coordinate.z);
    public static Coordinate operator +(Coordinate a, Coordinate b) => new Coordinate(a.x + b.x, a.z + b.z);
    public static Coordinate operator -(Coordinate a, Coordinate b) => new Coordinate(a.x - b.x, a.z - b.z);
    public static Coordinate operator *(Coordinate a, Coordinate b) => new Coordinate(a.x * b.x, a.z * b.z);
    public static Coordinate operator /(Coordinate a, Coordinate b) => new Coordinate(a.x / b.x, a.z / b.z);
    public static Coordinate operator *(Coordinate a, int b) => new Coordinate(a.x * b, a.z * b);
    public static bool operator ==(Coordinate a, Coordinate b) => a.x == b.x && a.z == b.z;
    public static bool operator !=(Coordinate a, Coordinate b) => a.x != b.x || a.z != b.z;
}
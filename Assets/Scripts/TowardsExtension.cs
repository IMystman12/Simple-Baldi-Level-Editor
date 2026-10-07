using System.Collections.Generic;
using UnityEngine;

public static class TowardsExtension
{
  static Towards[] opposite = new Towards[]
    {
        Towards.South,
        Towards.West,
        Towards.North,
        Towards.East,
    };
  public static Towards GetOpposite(this Towards towards) => opposite[(int)towards];

  public static List<Towards> All => new List<Towards>()
      {
          Towards.North,
          Towards.East,
          Towards.South,
          Towards.West
      };

  static List<Coordinate> relativeCoordinates = new List<Coordinate>()
      {
          new Coordinate(0,1),
          new Coordinate(1,0),
          new Coordinate(0,-1),
          new Coordinate(-1,0),
      };
  public static Coordinate GetRelativeCoordinate(this Towards towards) => relativeCoordinates[(int)towards];

  static List<Quaternion> uIRotations = new List<Quaternion>()
      {
        Quaternion.Euler(0,0,0),
        Quaternion.Euler(0,0,270),
        Quaternion.Euler(0,0,180),
        Quaternion.Euler(0,0,90),
      };
  public static Quaternion GetUIRotation(this Towards towards) => uIRotations[(int)towards];

  public static int GetBinary(this Towards towards) => 1 << (int)towards;

  public static Towards FromPointAToB(Coordinate coordinateA, Coordinate coordinateB)
  {
    Coordinate delta = coordinateB - coordinateA;
    if (delta.z > 0) return Towards.North;
    if (delta.x > 0) return Towards.East;
    if (delta.z < 0) return Towards.South;
    if (delta.x < 0) return Towards.West;
    return Towards.NaD;
  }

  public static List<Towards> OpenTowardsFromBin(int bin)
  {
    var list = new List<Towards>();
    for (int i = 0; i < 4; i++)
    {
      if ((bin & (1 << i)) == 0)
      {
        list.Add((Towards)i);
      }
    }
    return list;
  }
}

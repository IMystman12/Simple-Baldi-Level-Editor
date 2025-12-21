using System;
using System.Collections.Generic;
using UnityEngine;

public static class Directions
{
	public const int Count = 4;

	private const float angleBuffer = 80f;

	private static IntVector2[] vectors = new IntVector2[5]
	{
		new IntVector2(0, 1),
		new IntVector2(1, 0),
		new IntVector2(0, -1),
		new IntVector2(-1, 0),
		new IntVector2(0, 0)
	};

	private static IntVector2[] cellDataRotationVectors = new IntVector2[4]
	{
		new IntVector2(1, 1),
		new IntVector2(1, -1),
		new IntVector2(-1, -1),
		new IntVector2(-1, 1)
	};

	private static Vector3[] vector3s = new Vector3[4]
	{
		new Vector3(0f, 0f, 1f),
		new Vector3(1f, 0f, 0f),
		new Vector3(0f, 0f, -1f),
		new Vector3(-1f, 0f, 0f)
	};

	private static Direction[] opposites = new Direction[5]
	{
		Direction.South,
		Direction.West,
		Direction.North,
		Direction.East,
		Direction.Null
	};

	private static Quaternion[] rotations = new Quaternion[4]
	{
		Quaternion.identity,
		Quaternion.Euler(0f, 90f, 0f),
		Quaternion.Euler(0f, 180f, 0f),
		Quaternion.Euler(0f, 270f, 0f)
	};

	private static Quaternion[] uiRotations = new Quaternion[4]
	{
		Quaternion.identity,
		Quaternion.Euler(0f, 0f, 270f),
		Quaternion.Euler(0f, 0f, 180f),
		Quaternion.Euler(0f, 0f, 90f)
	};

	private static float[] degrees = new float[4] { 0f, 90f, 180f, 270f };

	private static int[] binary = new int[4] { 1, 2, 4, 8 };

	public static Direction RandomDirection
	{
		get
		{
			return (Direction)UnityEngine.Random.Range(0, 4);
		}
	}

	public static IntVector2[] Vectors
	{
		get
		{
			return vectors;
		}
	}

	public static Direction ControlledRandomDirection(System.Random rng)
	{
		return (Direction)rng.Next(0, 4);
	}

	public static Direction FromInt(int i)
	{
		return (Direction)i;
	}

	public static IntVector2 CellDataRotationVector(Direction direction)
	{
		return cellDataRotationVectors[(int)direction];
	}

	public static IntVector2 ToIntVector2(this Direction direction)
	{
		return vectors[(int)direction];
	}

	public static Vector3 ToVector3(this Direction direction)
	{
		return vector3s[(int)direction];
	}

	public static Direction GetOpposite(this Direction direction)
	{
		return opposites[(int)direction];
	}

	public static Direction RotatedRelativeToNorth(this Direction direction, Direction rotation)
	{
		int num = (int)direction;
		num = (int)(num + rotation);
		if (num >= 4)
		{
			num -= 4;
		}
		return (Direction)num;
	}

	public static Quaternion ToRotation(this Direction direction)
	{
		return rotations[(int)direction];
	}

	public static Quaternion ToUiRotation(this Direction direction)
	{
		return uiRotations[(int)direction];
	}

	public static float ToDegrees(this Direction direction)
	{
		return degrees[(int)direction];
	}

	public static int ToBinary(this Direction direction)
	{
		return binary[(int)direction];
	}

	public static List<Direction> PerpendicularList(this Direction dir)
	{
		List<Direction> list = new List<Direction>();
		switch (dir)
		{
			case Direction.North:
				list.Add(Direction.East);
				list.Add(Direction.West);
				break;
			case Direction.East:
				list.Add(Direction.North);
				list.Add(Direction.South);
				break;
			case Direction.South:
				list.Add(Direction.East);
				list.Add(Direction.West);
				break;
			case Direction.West:
				list.Add(Direction.North);
				list.Add(Direction.South);
				break;
		}
		return list;
	}

	public static List<Direction> OpenDirectionsFromBin(int bin)
	{
		List<Direction> list = new List<Direction>();
		for (int i = 0; i < 4; i++)
		{
			if ((bin & (1 << i)) == 0)
			{
				list.Add((Direction)i);
			}
		}
		return list;
	}

	public static void FillOpenDirectionsFromBin(List<Direction> list, int bin)
	{
		list.Clear();
		for (int i = 0; i < 4; i++)
		{
			if ((bin & (1 << i)) == 0)
			{
				list.Add((Direction)i);
			}
		}
	}

	public static List<Direction> ClosedDirectionsFromBin(int bin)
	{
		List<Direction> list = new List<Direction>();
		for (int i = 0; i < 4; i++)
		{
			if ((bin & (1 << i)) > 0)
			{
				list.Add((Direction)i);
			}
		}
		return list;
	}

	public static void FillClosedDirectionsFromBin(List<Direction> list, int bin)
	{
		list.Clear();
		for (int i = 0; i < 4; i++)
		{
			if ((bin & (1 << i)) > 0)
			{
				list.Add((Direction)i);
			}
		}
	}

	public static bool ContainsDirection(this int val, Direction direction)
	{
		return (val & (1 << direction.BitPosition())) > 0;
	}

	public static List<Direction> All => new List<Direction>
		{
			Direction.North,
			Direction.East,
			Direction.South,
			Direction.West
		};

	public static void FillWithAll(List<Direction> list)
	{
		list.Clear();
		list.Add(Direction.North);
		list.Add(Direction.East);
		list.Add(Direction.South);
		list.Add(Direction.West);
	}

	public static void DirsFromVector3(Vector3 vector, List<Direction> list)
	{
		list.Clear();
		float num = Vector3.SignedAngle(vector, Vector3.right, Vector3.up);
		if (num >= 10f && num <= 170f)
		{
			list.Add(Direction.North);
		}
		if (num >= -80f && num <= 80f)
		{
			list.Add(Direction.East);
		}
		if (num >= -170f && num <= -10f)
		{
			list.Add(Direction.South);
		}
		if (num <= -100f || num >= 100f)
		{
			list.Add(Direction.West);
		}
	}

	public static Direction DirFromVector3(Vector3 vector, float buffer)
	{
		float num = Vector3.SignedAngle(vector, Vector3.right, Vector3.up);
		if (num >= 90f - buffer && num <= 90f + buffer)
		{
			return Direction.North;
		}
		if (num >= 0f - buffer && num <= 0f + buffer)
		{
			return Direction.East;
		}
		if (num >= -90f - buffer && num <= -90f + buffer)
		{
			return Direction.South;
		}
		if (num <= -180f + buffer || num >= 180f - buffer)
		{
			return Direction.West;
		}
		Debug.LogWarning("DirFromVector3 with angle " + num + " and buffer " + buffer + " returned null");
		return Direction.Null;
	}

	public static void ReverseList(List<Direction> list)
	{
		for (int i = 0; i < list.Count; i++)
		{
			list[i] = list[i].GetOpposite();
		}
	}

	public static int BitPosition(this Direction dir)
	{
		switch (dir)
		{
			case Direction.North:
				return 0;
			case Direction.East:
				return 1;
			case Direction.South:
				return 2;
			case Direction.West:
				return 3;
			default:
				return 0;
		}
	}

	public static int RotateBin(int type, Direction direction)
	{
		int num = type;
		for (int i = 0; i < (int)direction; i++)
		{
			num <<= 1;
			num = (num | (num >> 4)) & 0xF;
		}
		return num;
	}

	public static Direction FromPointAToB(IntVector2 pointA, IntVector2 pointB)
	{
		if (pointA.z < pointB.z)
		{
			return Direction.North;
		}
		if (pointA.x < pointB.x)
		{
			return Direction.East;
		}
		if (pointA.z > pointB.z)
		{
			return Direction.South;
		}
		if (pointA.x > pointB.x)
		{
			return Direction.West;
		}
		return Direction.Null;
	}
}

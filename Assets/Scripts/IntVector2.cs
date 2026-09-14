using System;
using UnityEngine;

[Serializable]
public struct IntVector2
{
	public int x;

	public int z;

	public IntVector2(int x, int z)
	{
		this.x = x;
		this.z = z;
	}

	public static IntVector2 operator +(IntVector2 a, IntVector2 b)
	{
		a.x += b.x;
		a.z += b.z;
		return a;
	}

	public static IntVector2 operator -(IntVector2 a, IntVector2 b)
	{
		a.x -= b.x;
		a.z -= b.z;
		return a;
	}

	public static IntVector2 operator *(IntVector2 a, int b)
	{
		a.x *= b;
		a.z *= b;
		return a;
	}

	public static Vector2 operator *(IntVector2 a, float b)
	{
		Vector2 result = default(Vector2);
		result.x = (float)a.x * b;
		result.y = (float)a.z * b;
		return result;
	}

	public static bool operator ==(IntVector2 a, IntVector2 b)
	{
		if (a.x == b.x)
		{
			return a.z == b.z;
		}
		return false;
	}

	public static bool operator !=(IntVector2 a, IntVector2 b)
	{
		if (a.x == b.x)
		{
			return a.z != b.z;
		}
		return true;
	}

	public static implicit operator Vector2(IntVector2 intVector)
	{
		return new Vector2(intVector.x, intVector.z);
	}

	public static IntVector2 one => new IntVector2(1, 1);

	public IntVector2 Scale(IntVector2 toScale)
	{
		toScale.x *= x;
		toScale.z *= z;
		return toScale;
	}

	public static IntVector2 ControlledRandomPosition(int minX, int maxX, int minZ, int maxZ, System.Random rng)
	{
		return new IntVector2(rng.Next(minX, maxX), rng.Next(minZ, maxZ));
	}

	public static IntVector2 RandomPosition(int rangeX, int rangeZ)
	{
		return new IntVector2(UnityEngine.Random.Range(0, rangeX), UnityEngine.Random.Range(0, rangeZ));
	}

	public static Vector2 ToVector2(IntVector2 iv2)
	{
		return new Vector2(iv2.x, iv2.z);
	}

	public static IntVector2 GetGridPosition(Vector3 position)
	{
		IntVector2 result = default(IntVector2);
		result.x = Mathf.RoundToInt(position.x);
		result.z = Mathf.RoundToInt(position.y);
		return result;
	}

	public static IntVector2 GetGridPosition(Vector2 position)
	{
		IntVector2 result = default(IntVector2);
		result.x = Mathf.RoundToInt(position.x);
		result.z = Mathf.RoundToInt(position.y);
		return result;
	}

	public static IntVector2 CombineLowest(IntVector2 vectorA, IntVector2 vectorB)
	{
		IntVector2 result = default(IntVector2);
		if (vectorA.x > vectorB.x)
		{
			result.x = vectorB.x;
		}
		else
		{
			result.x = vectorA.x;
		}
		if (vectorA.z > vectorB.z)
		{
			result.z = vectorB.z;
		}
		else
		{
			result.z = vectorA.z;
		}
		return result;
	}

	public static IntVector2 CombineGreatest(IntVector2 vectorA, IntVector2 vectorB)
	{
		IntVector2 result = default(IntVector2);
		if (vectorA.x > vectorB.x)
		{
			result.x = vectorA.x;
		}
		else
		{
			result.x = vectorB.x;
		}
		if (vectorA.z > vectorB.z)
		{
			result.z = vectorA.z;
		}
		else
		{
			result.z = vectorB.z;
		}
		return result;
	}

	public IntVector2 Adjusted(IntVector2 pivot, Direction direction)
	{
		IntVector2 intVector = default(IntVector2);
		if (direction == Direction.North || direction == Direction.South)
		{
			intVector.x = x - pivot.x;
			intVector.z = z - pivot.z;
		}
		else
		{
			intVector.x = z - pivot.z;
			intVector.z = x - pivot.x;
		}
		return intVector.Scale(Directions.CellDataRotationVector(direction));
	}

	public override string ToString() => string.Format("{0},{1}", x, z);

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
			}
			throw new IndexOutOfRangeException();
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
			}
		}
	}
}

using System.Collections.Generic;
using UnityEngine;

public static class IntVector2Extensions
{
	public static void Adjust(this List<IntVector2> list, IntVector2 position, IntVector2 pivot, Direction direction)
	{
		for (int i = 0; i < list.Count; i++)
		{
			list[i] = list[i].Adjusted(pivot, direction) + position;
		}
	}

	public static void RemoveMatchingIntVector2s(this List<IntVector2> list, IntVector2 toRemove)
	{
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] == toRemove)
			{
				list.RemoveAt(i);
				i--;
				Debug.Log("Found and removed matching IV2");
			}
		}
	}

	public static void AddRangeExceptDuplicates(this List<IntVector2> list, List<IntVector2> toAdd)
	{
		for (int i = 0; i < toAdd.Count; i++)
		{
			if (!list.Contains(toAdd[i]))
			{
				list.Add(toAdd[i]);
			}
		}
	}
}

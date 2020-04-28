using System;
using Assets.Scripts.Managers;
using UnityEngine;

namespace Assets.Scripts.Utils
{
	internal static class BoundariesUtil
	{
		/// <summary>
		/// This function will calculate the boundaries of an object.
		/// </summary>
		/// <param name="parent"></param>
		/// <returns></returns>
		public static Bounds GetMaxBounds(GameObject parent)
		{
			Bounds total = new Bounds(parent.transform.position, Vector3.zero);
			foreach (Collider child in parent.GetComponentsInChildren<Collider>())
			{
				total.Encapsulate(child.bounds);
			}

			return total;
		}

		/// <summary>
		/// This function will calculate the maximum boundaries of the grid.
		/// Tuple item 1 is maximum X and item 2 is maximum Z.
		/// </summary>
		public static Tuple<int, int> CalculateMaxGridBoundaries(GridManager gridManager)
		{
			int maxX = gridManager.Cols * gridManager.TileSize;
			int maxZ = gridManager.Rows * gridManager.TileSize;

			return new Tuple<int, int>(maxX, maxZ);
		}

	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Utils
{
	/// <summary>
	/// Extensions for an Enumerable.
	/// </summary>
	public static class EnumerableExtension
	{
		/// <summary>
		/// This function will pick 1 random element from an enumerable.
		/// </summary>
		/// <returns></returns>
		public static T PickRandom<T>(this IEnumerable<T> source)
		{
			return source.PickRandom(1).Single();
		}

		/// <summary>
		/// This function will pick random elements from an enumerable depending on the count.
		/// </summary>
		/// <param name="count">Amount of objects to pick</param>
		/// <returns></returns>
		public static IEnumerable<T> PickRandom<T>(this IEnumerable<T> source, int count)
		{
			return source.Shuffle().Take(count);
		}

		/// <summary>
		/// This function will shuffle all elements in an enumerable.
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
		{
			return source.OrderBy(x => Guid.NewGuid());
		}
	}

	/// <summary>
	/// Generate a sprite based on a 2D texture.
	/// </summary>
	public static class Texture2DExtension
	{
		/// <summary>
		/// This function will generate a sprite based on a Texture2D object.
		/// </summary>
		/// <returns>Returns a sprite based on a texture</returns>
		public static Sprite GenerateSprite(this Texture2D source)
		{
			return Sprite.Create(source, new Rect(0.0f, 0.0f, source.width, source.height), new Vector2(0.5f, 0.5f),
				100.0f);
		}
	}
}

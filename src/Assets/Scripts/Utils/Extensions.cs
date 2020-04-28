using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Utils
{
	/// <summary>
	/// Pick a random element from a list
	/// </summary>
	public static class EnumerableExtension
	{
		public static T PickRandom<T>(this IEnumerable<T> source)
		{
			return source.PickRandom(1).Single();
		}

		public static IEnumerable<T> PickRandom<T>(this IEnumerable<T> source, int count)
		{
			return source.Shuffle().Take(count);
		}

		public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
		{
			return source.OrderBy(x => Guid.NewGuid());
		}
	}

	/// <summary>
	/// Generate a sprite based on a 2D texture
	/// </summary>
	public static class Texture2DExtension
	{
		public static Sprite GenerateSprite(this Texture2D source)
		{
			return Sprite.Create(source, new Rect(0.0f, 0.0f, source.width, source.height), new Vector2(0.5f, 0.5f),
				100.0f);
		}
	}
}

using UnityEngine;

namespace Assets.Scripts.Interfaces
{
	internal interface IVisualizedObject
	{
		/// <summary>
		/// Type of an object. For now we only know building, staging-building and vehicle.
		/// </summary>
		string Type { get; }

		/// <summary>
		/// Size of an object. This is important because some elements in the game rely on the size of an object, such as building size, vehicle size.
		/// </summary>
		int Size { get; }

		/// <summary>
		/// Identifier of an object. Can be anything, but must be unique for every object except vehicles and buildings.
		/// </summary>
		string Identifier { get; }

		/// <summary>
		/// GameObject of an object. This can be null if the object is not spawned yet.
		/// </summary>
		GameObject GameObject { get; set; }
	}
}
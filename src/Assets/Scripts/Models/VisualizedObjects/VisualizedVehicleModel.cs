using Assets.Scripts.Interfaces;
using UnityEngine;

namespace Assets.Scripts.Models.VisualizedObjects
{
	internal class VisualizedVehicleModel : IVisualizedObject
	{
		public string Type { get; } = "vehicle";
		public int Size { get; set; } = 15;
		public string Identifier { get; set; }
		public GameObject GameObject { get; set; }

		protected bool Equals(VisualizedVehicleModel other)
		{
			return Identifier == other.Identifier;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((VisualizedVehicleModel) obj);
		}

		public override int GetHashCode()
		{
			return (Identifier != null ? Identifier.GetHashCode() : 0);
		}
	}
}
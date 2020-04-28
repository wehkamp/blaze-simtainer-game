using System.Collections.Generic;
using Assets.Scripts.Interfaces;
using UnityEngine;

namespace Assets.Scripts.Models.VisualizedObjects
{
	internal class VisualizedBuildingModel : IVisualizedBuilding
	{
		// Default 15.0
		public string Type { get; } = "building";
		public int Size { get; set; } = 15;
		public Dictionary<string, double> LayerValues { get; set; }
		public string Identifier { get; set; }
		public GameObject GameObject { get; set; }

		protected bool Equals(VisualizedBuildingModel other)
		{
			return Identifier == other.Identifier;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((VisualizedBuildingModel) obj);
		}

		public override int GetHashCode()
		{
			return (Identifier != null ? Identifier.GetHashCode() : 0);
		}
	}
}
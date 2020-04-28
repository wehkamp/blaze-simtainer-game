using System.Collections.Generic;
using Assets.Scripts.Interfaces;
using UnityEngine;

namespace Assets.Scripts.Models.VisualizedObjects
{
	internal class VisualizedStagingBuildingModel : IVisualizedBuilding
	{
		public string Type { get; } = "staging-building";
		public int Size { get; set; } = 15;
		public Dictionary<string, double> LayerValues { get; set; }
		public string Identifier { get; set; }
		public GameObject GameObject { get; set; }

		protected bool Equals(VisualizedStagingBuildingModel other)
		{
			return Identifier == other.Identifier;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((VisualizedStagingBuildingModel) obj);
		}

		public override int GetHashCode()
		{
			return (Identifier != null ? Identifier.GetHashCode() : 0);
		}
	}
}
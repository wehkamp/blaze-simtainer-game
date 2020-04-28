using System.Collections.Generic;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Models.Layers;

namespace Assets.Scripts.Models
{
	internal class NeighbourhoodModel
	{
		/// <summary>
		/// Name of the neighbourhood. Must be unique.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// List of objects for the neighbourhood.
		/// </summary>
		public List<IVisualizedObject> VisualizedObjects { get; set; } = new List<IVisualizedObject>();

		/// <summary>
		/// List of layers values for the neighbourhood. This is necessary to calculate thresholds.
		/// </summary>
		public List<LayerValueModel> LayerValues { get; set; }

		/// <summary>
		/// Age of a neighbourhood. This is important to determine the state of a building.
		/// </summary>
		public int Age { get; set; } = 150;

		/// <summary>
		/// Team that belongs to a neighbourhood. Only used when teams are enabled.
		/// </summary>
		public string Team { get; set; }

		protected bool Equals(NeighbourhoodModel other)
		{
			return Name == other.Name;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((NeighbourhoodModel) obj);
		}

		public override int GetHashCode()
		{
			return (Name != null ? Name.GetHashCode() : 0);
		}
	}
}
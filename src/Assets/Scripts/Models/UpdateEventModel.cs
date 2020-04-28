using System.Collections.Generic;
using Assets.Scripts.Interfaces;

namespace Assets.Scripts.Models
{
	internal class UpdateEventModel
	{
		/// <summary>
		/// The neighbourhood name must always be set.
		/// </summary>
		public string NeighbourhoodName { get; set; }

		/// <summary>
		/// If a new neighbourhood has been added, this property must be set.
		/// </summary>
		public NeighbourhoodModel AddedNeighbourhood { get; set; }

		/// <summary>
		/// If a neighbourhood has been updated, this property must be set.
		/// </summary>
		public NeighbourhoodModel UpdatedNeighbourhood { get; set; }

		/// <summary>
		/// If a neighbourhood has been removed, this property must be set. The string is the identifier of the removed neighbourhood.
		/// So basically the name.
		/// </summary>
		public string RemovedNeighbourhood { get; set; }

		/// <summary>
		/// If a new object has been added, building for example, this property must be set.
		/// </summary>
		public IVisualizedObject AddedVisualizedObject { get; set; }

		/// <summary>
		/// If an object has been removed, this property must be set. The string is the identifier of the removed object.
		/// So basically the name.
		/// </summary>
		public string RemovedVisualizedObject { get; set; }

		/// <summary>
		/// If layer values have been updated, this property must be set. The structure is the following for example
		/// {"NAME-OF-A-VISUALIZED-OBJECT": {"cpuLayer": 0.1, "memoryLayer": 394 } }
		/// </summary>
		public Dictionary<string, Dictionary<string, double>> UpdatedLayerValues { get; set; }

		/// <summary>
		/// If objects are updated, this property must be set. It's only used for now in the <see cref="Managers.TrafficManager"/>.
		/// </summary>
		public List<IVisualizedObject> UpdatedVisualizedObjects { get; set; }
	}
}
using System.Collections.Generic;
using Assets.Scripts.Models.Layers;

namespace Assets.Scripts.Models
{
	/// <summary>
	/// Most important data class in the game.
	/// This model contains information about every neighbourhood, every layer and every team.
	/// </summary>
	internal class GameModel
	{
		/// <summary>
		/// List of neighbourhood retrieved from the API and kept up-to-date in the <see cref="Managers.CityManager"/>.
		/// </summary>
		public HashSet<NeighbourhoodModel> Neighbourhoods { get; set; }

		/// <summary>
		/// List of layers that we retrieve from the API. This is handled in the <see cref="Managers.LayerManager"/>
		/// </summary>
		public List<VisualLayerModel> Layers { get; set; }

		/// <summary>
		/// List of teams that we retrieve from the API. This is handled in the <see cref="Managers.TeamManager"/>
		/// </summary>
		public HashSet<string> Teams { get; set; }
	}
}
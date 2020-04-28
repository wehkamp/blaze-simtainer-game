using System.Collections.Generic;

namespace Assets.Scripts.Interfaces
{
	internal interface IVisualizedBuilding : IVisualizedObject
	{
		/// <summary>
		/// Dictionary with all layer values. Must be the same as the layers that are given in the GameMode <see cref="Models.GameModel"/>.
		/// </summary>
		Dictionary<string, double> LayerValues { get; set; }
	}
}

using System.Collections.Generic;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Models.VisualizedObjects;

namespace Assets.Scripts.Utils
{
	/// <summary>
	/// Factory to generate a visualized object based on the type received from the API.
	/// </summary>
	internal static class VisualizedObjectFactory
	{
		public static IVisualizedObject Build(string typeOfObject, int size,
			Dictionary<string, double> layerValues, string identifier)
		{
			switch (typeOfObject)
			{
				case "building":
					return new VisualizedBuildingModel
						{Size = size, LayerValues = layerValues, Identifier = identifier};
				case "staging-building":
					return new VisualizedStagingBuildingModel
						{Size = size, LayerValues = layerValues, Identifier = identifier};
				case "vehicle":
					return new VisualizedVehicleModel
						{Size = size, Identifier = identifier};
				default:
					return null;
			}
		}
	}
}
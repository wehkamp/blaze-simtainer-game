using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Models;
using Assets.Scripts.Models.Layers;
using SimpleJSON;

namespace Assets.Scripts.Utils
{
	internal static class JsonParser
	{
		/// <summary>
		/// Function to parse all the JSON data and convert it into a GameModel object.
		/// If API structure changes happen it should be adjusted in here.
		/// </summary>
		/// <param name="jsonString"></param>
		/// <returns></returns>
		public static GameModel ParseGameModel(string jsonString)
		{
			HashSet<NeighbourhoodModel> neighbourhoodModels = new HashSet<NeighbourhoodModel>();
			HashSet<string> teams = new HashSet<string>();
			JSONNode jsonOBj = JSON.Parse(jsonString);

			// Loop through all layers
			List<VisualLayerModel> visualLayerModels = jsonOBj["layers"].Children
				.Select(layer => new VisualLayerModel {Icon = layer["icon"], Name = layer["layerType"]}).ToList();

			// Loop through neighboorhoods
			foreach (JSONNode neighbourhoodJsonObj in jsonOBj["neighbourhoods"].Children)
			{
				NeighbourhoodModel neighbourhood = ParseNeighbourhoodModel(neighbourhoodJsonObj);
				neighbourhoodModels.Add(neighbourhood);
			}

			// Loop through teams
			foreach (JSONNode team in jsonOBj["teams"])
			{
				teams.Add(team);
			}

			GameModel gameModel = new GameModel
				{Neighbourhoods = neighbourhoodModels, Layers = visualLayerModels, Teams = teams};
			return gameModel;
		}

		private static NeighbourhoodModel ParseNeighbourhoodModel(JSONNode jsonNode)
		{
			NeighbourhoodModel neighbourhood = new NeighbourhoodModel();
			neighbourhood.Name = jsonNode["name"];
			neighbourhood.Age = jsonNode["daysOld"];
			neighbourhood.Team = jsonNode["team"];

			List<LayerValueModel> visualLayers = jsonNode["layerValues"].Children.Select(visualLayer =>
				new LayerValueModel
				{
					MinValue = visualLayer["minValue"],
					LayerType = visualLayer["layerType"],
					MaxValue = visualLayer["maxValue"]
				}).ToList();

			neighbourhood.LayerValues = visualLayers;

			// Loop through all visualized objects (buildings, traffic for later, etc)
			foreach (JSONNode visualizedObjectJson in jsonNode["visualizedObjects"].Children)
			{
				Dictionary<string, double> layerValues = new Dictionary<string, double>();
				foreach (KeyValuePair<string, JSONNode> layerValue in visualizedObjectJson["layerValues"])
				{
					layerValues.Add(layerValue.Key, layerValue.Value);
				}

				IVisualizedObject visualizedObject =
					VisualizedObjectFactory.Build(
						visualizedObjectJson["type"],
						visualizedObjectJson["size"],
						layerValues, visualizedObjectJson["identifier"]);


				neighbourhood.VisualizedObjects.Add(visualizedObject);
			}

			return neighbourhood;
		}

		/// <summary>
		/// Function to parse an update event into the correct model.
		/// If API structure changes happen it should be adjusted in here.
		/// </summary>
		/// <param name="jsonString"></param>
		/// <returns></returns>
		public static UpdateEventModel ParseUpdateEvent(string jsonString)
		{
			JSONNode jsonOBj = JSON.Parse(jsonString);


			UpdateEventModel updateEventModel = new UpdateEventModel();

			if (jsonOBj["neighbourhoodName"] != null)
			{
				updateEventModel.NeighbourhoodName = jsonOBj["neighbourhoodName"];
			}

			if (jsonOBj["removedNeighbourhood"] != null)
			{
				updateEventModel.RemovedNeighbourhood = jsonOBj["removedNeighbourhood"];
			}

			if (jsonOBj["addedNeighbourhood"] != null)
			{
				updateEventModel.AddedNeighbourhood = ParseNeighbourhoodModel(jsonOBj["addedNeighbourhood"]);
			}

			if (jsonOBj["updatedNeighbourhood"] != null)
			{
				updateEventModel.UpdatedNeighbourhood = ParseNeighbourhoodModel(jsonOBj["updatedNeighbourhood"]);
			}

			if (jsonOBj["removedVisualizedObject"] != null)
			{
				updateEventModel.RemovedVisualizedObject = jsonOBj["removedVisualizedObject"];
			}

			if (jsonOBj["addedVisualizedObject"] != null)
			{
				updateEventModel.AddedVisualizedObject =
					VisualizedObjectFactory.Build(
						jsonOBj["addedVisualizedObject"]["type"],
						jsonOBj["addedVisualizedObject"]["size"],
						null, jsonOBj["addedVisualizedObject"]["identifier"]);
			}

			if (jsonOBj["updatedVisualizedObjects"] != null)
			{
				List<IVisualizedObject> visualizedObjects = new List<IVisualizedObject>();
				foreach (JSONNode updatedVisualizedObjectNode in jsonOBj["updatedVisualizedObjects"].Children)
				{
					visualizedObjects.Add(VisualizedObjectFactory.Build(
						updatedVisualizedObjectNode["type"],
						updatedVisualizedObjectNode["size"],
						null, updatedVisualizedObjectNode["identifier"]));
				}

				if (visualizedObjects.Count > 0)
					updateEventModel.UpdatedVisualizedObjects = visualizedObjects;
			}

			if (jsonOBj["updatedLayerValues"] != null)
			{
				Dictionary<string, Dictionary<string, double>> updatedLayerValues =
					new Dictionary<string, Dictionary<string, double>>();


				foreach (KeyValuePair<string, JSONNode> layerValue in jsonOBj["updatedLayerValues"])
				{
					Dictionary<string, double> layerValues = new Dictionary<string, double>();
					foreach (KeyValuePair<string, JSONNode> values in layerValue.Value)
					{
						layerValues.Add(values.Key, values.Value);
					}

					updatedLayerValues.Add(layerValue.Key, layerValues);
				}

				updateEventModel.UpdatedLayerValues = updatedLayerValues;
			}

			return updateEventModel;
		}

		/// <summary>
		/// Function to parse the URL of a neighbourhood.
		/// </summary>
		/// <param name="jsonString"></param>
		/// <returns></returns>
		public static string ParseNeighbourhoodUrl(string jsonString)
		{
			JSONNode jsonOBj = JSON.Parse(jsonString);
			return jsonOBj["url"];
		}
	}
}
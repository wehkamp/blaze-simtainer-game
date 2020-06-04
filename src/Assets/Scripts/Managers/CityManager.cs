using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Models;
using Assets.Scripts.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Managers
{
	internal class CityManager : Singleton<CityManager>
	{
		// This object is used through the whole application. It contains all the information about the city
		public GameModel GameModel;

		// Event for when the grid is updated
		public UnityEvent CityUpdatedEvent = new UnityEvent();


		// Start is called before the first frame update
		void Start()
		{
			// Add event listener for API initialization
			ApiManager.Instance.ApiInitializedEvent.AddListener(ApiInitialization);

			// Add event listener for API updates
			ApiManager.Instance.ApiUpdateEvent.AddListener(HandleApiUpdate);
		}

		/// <summary>
		/// If the escape button is pressed, we want to go back to the main menu
		/// </summary>
		void Update()
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				if (CameraManager.Instance.ActiveCamera != CameraManager.CameraType.MainCamera)
				{
					CameraManager.Instance.SwitchCamera(CameraManager.CameraType.MainCamera);
				}
				else
				{
					SceneManager.LoadScene("MainMenu");
				}
			}
		}

		/// <summary>
		/// Generate the city as soon as the API is initialized.
		/// </summary>
		/// <param name="gameModel"></param>
		private void ApiInitialization(GameModel gameModel)
		{
			GameModel = gameModel;
			GridManager.Instance.GenerateCity(GameModel);
			CityUpdatedEvent?.Invoke();
		}

		/// <summary>
		/// This method handles all the updates from the API.
		/// </summary>
		/// <param name="updateEventModel"></param>
		private void HandleApiUpdate(UpdateEventModel updateEventModel)
		{
			try
			{
				// Check if the layer values are updated and if so update them in the GameModel
				if (updateEventModel.UpdatedLayerValues != null)
				{
					UpdateLayerValues(updateEventModel.UpdatedLayerValues);
				}

				// Check if a neighbourhood is removed
				if (updateEventModel.RemovedNeighbourhood != null)
				{
					RemoveNeighbourhood(updateEventModel.RemovedNeighbourhood);
				}

				// Check if a visualized object is removed. For now it is probably a building
				if (updateEventModel.RemovedVisualizedObject != null)
				{
					// Removed visualized object is just an identifier
					RemoveVisualizedBuildings(updateEventModel.NeighbourhoodName,
						updateEventModel.RemovedVisualizedObject);
				}

				// Check if a neighbourhood is updated
				if (updateEventModel.UpdatedNeighbourhood != null)
				{
					// Update age & LayerValues
					UpdateNeighbourhood(updateEventModel.NeighbourhoodName, updateEventModel.UpdatedNeighbourhood);
				}

				if (updateEventModel.AddedNeighbourhood != null)
				{
					AddNeighbourhood(updateEventModel.AddedNeighbourhood);
				}

				if (updateEventModel.AddedVisualizedObject != null)
				{
					AddBuilding(updateEventModel.NeighbourhoodName, updateEventModel.AddedVisualizedObject);
				}

				CityUpdatedEvent?.Invoke();
			}
			catch (Exception e)
			{
#if UNITY_EDITOR
				Debug.LogError(e.StackTrace);
#else
				Debug.LogError(e.Message);
#endif
			}
		}

		/// <summary>
		/// Function to remove a visualized object from the game. Vehicles are excluded.
		/// </summary>
		/// <param name="neighbourhoodName"></param>
		/// <param name="identifier"></param>
		private void RemoveVisualizedBuildings(string neighbourhoodName, string identifier)
		{
			NeighbourhoodModel neighbourhood =
				GameModel.Neighbourhoods.SingleOrDefault(neighbourhoodModel =>
					neighbourhoodModel.Name == neighbourhoodName);
			// Make a list of buildings we need to remove. We don't want to remove vehicles in this manager.
			List<IVisualizedBuilding> removedBuildings =
				neighbourhood?.VisualizedObjects.Where(
					x => x.Identifier == identifier && x is IVisualizedBuilding).Cast<IVisualizedBuilding>().ToList();
			if (removedBuildings != null)
			{
				foreach (IVisualizedBuilding removedBuilding in removedBuildings)
				{
					GridManager.Instance.DestroyBuilding(removedBuilding);
					neighbourhood.VisualizedObjects.Remove(removedBuilding);
				}
			}
			else
			{
				Debug.LogWarning($"Removing object went wrong! Neighbourhood {neighbourhoodName} does not exists.");
			}
		}

		/// <summary>
		/// Function to de-spawn and remove a complete neighbourhood.
		/// </summary>
		/// <param name="neighbourhoodName"></param>
		private void RemoveNeighbourhood(string neighbourhoodName)
		{
			NeighbourhoodModel removedNeighbourhood =
				GameModel.Neighbourhoods.SingleOrDefault(neighbourhoodModel =>
					neighbourhoodModel.Name == neighbourhoodName);

			if (removedNeighbourhood != null)
			{
				// We could find one! Now let's destroy it
				GridManager.Instance.DestroyBlock(removedNeighbourhood, true);
				GameModel.Neighbourhoods.Remove(removedNeighbourhood);
			}
			else
			{
				Debug.LogWarning(
					$"Removing a neighbourhood went wrong! Neighbourhood {neighbourhoodName} does not exists");
			}
		}

		/// <summary>
		/// Function to update an existing neighbourhood.
		/// </summary>
		/// <param name="neighbourhoodName"></param>
		/// <param name="updatedNeighbourhood"></param>
		private void UpdateNeighbourhood(string neighbourhoodName, NeighbourhoodModel updatedNeighbourhood)
		{
			NeighbourhoodModel neighbourhood =
				GameModel.Neighbourhoods.SingleOrDefault(neighbourhoodModel =>
					neighbourhoodModel.Name == neighbourhoodName);
			if (neighbourhood != null)
			{
				// Update the age and layer values of the neighbourhood
				neighbourhood.Age = updatedNeighbourhood.Age;
				neighbourhood.LayerValues = updatedNeighbourhood.LayerValues;
			}
			else
			{
				Debug.LogWarning(
					$"Updating neighbourhood went wrong! Neighbourhood {neighbourhoodName} does not exists!");
			}
		}

		/// <summary>
		/// Function to update all layer values of every neighbourhood.
		/// </summary>
		/// <param name="updatedLayerValues"></param>
		private void UpdateLayerValues(IReadOnlyDictionary<string, Dictionary<string, double>> updatedLayerValues)
		{
			foreach (NeighbourhoodModel neighbourhood in GameModel.Neighbourhoods)
			{
				foreach (IVisualizedBuilding visualizedObject in neighbourhood.VisualizedObjects
					.OfType<IVisualizedBuilding>().Where(visualizedObject =>
						updatedLayerValues.ContainsKey(visualizedObject.Identifier)))
				{
					visualizedObject.LayerValues =
						updatedLayerValues[visualizedObject.Identifier];
				}
			}
		}

		/// <summary>
		/// Function to add a new neighbourhood to the game and spawn them.
		/// </summary>
		/// <param name="newNeighbourhood"></param>
		private void AddNeighbourhood(NeighbourhoodModel newNeighbourhood)
		{
			if (GameModel.Neighbourhoods.Any(neighbourhoodModel =>
				neighbourhoodModel.Name != newNeighbourhood.Name))
			{
				GridManager.Instance.SpawnNeighbourhood(newNeighbourhood);
				GameModel.Neighbourhoods.Add(newNeighbourhood);
			}
		}

		/// <summary>
		/// Function to add a new visualized object to the game.
		/// </summary>
		/// <param name="neighbourhoodName"></param>
		/// <param name="newVisualizedObject"></param>
		private void AddBuilding(string neighbourhoodName, IVisualizedObject newVisualizedObject)
		{
			// We only care about buildings in this manager
			if (!(newVisualizedObject is IVisualizedBuilding)) return;

			NeighbourhoodModel neighbourhood =
				GameModel.Neighbourhoods.SingleOrDefault(x => x.Name == neighbourhoodName);
			if (neighbourhood == null)
			{
				Debug.LogWarning($"Adding object went wrong! Neighbourhood {neighbourhoodName} does not exists!");
				return;
			}

			// Check if there are buildings with the same identifier.
			List<IVisualizedBuilding> existingVisualizedObjects = neighbourhood.VisualizedObjects.Where(x =>
					x.Identifier == newVisualizedObject.Identifier && x is IVisualizedBuilding)
				.Cast<IVisualizedBuilding>()
				.ToList();

			// Destroy every building that has the same identifier.
			foreach (IVisualizedBuilding visualizedObject in existingVisualizedObjects)
			{
				GridManager.Instance.DestroyBuilding(visualizedObject);
				neighbourhood.VisualizedObjects.Remove(visualizedObject);
			}

			// TODO: We actually need to check if there are destroyed grass tiles, but haven't found a solution for it yet.
			neighbourhood.VisualizedObjects.Add(newVisualizedObject);
			GridManager.Instance.DestroyBlock(neighbourhood, false);
			GridManager.Instance.SpawnNeighbourhood(neighbourhood);
		}
	}
}
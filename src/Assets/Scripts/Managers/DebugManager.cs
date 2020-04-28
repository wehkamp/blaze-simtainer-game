using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Components;
using Assets.Scripts.Components.Navigators;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Models;
using Assets.Scripts.Utils;
using IngameDebugConsole;
using UnityEngine;

namespace Assets.Scripts.Managers
{
	internal class DebugManager : Singleton<DebugManager>
	{

		[SerializeField] private GameObject _fpsCounter;
		void Start()
		{
			//_apiChangeUnityEventManager.AddListener(InitializeBuildingsAndRoads);
			DebugLogConsole.AddCommand<string>("neighbourhood.remove", "Destroys a neighbourhood", RemoveNeighbourhood);
			DebugLogConsole.AddCommand<string>("building.remove", "Destroys a building", RemoveBuilding);
			DebugLogConsole.AddCommand<string>("neighbourhood.search", "Search a neighbourhood", SearchNeighbourhood);
			DebugLogConsole.AddCommand<bool>("fps", "Search a neighbourhood", ShowFpsCounter);
			DebugLogConsole.AddCommand("barrelroll", "Let the plane do barrel rolls", ToggleBarrelRoll);
		}


		/// <summary>
		/// Function to remove a neighbourhood. This only happens in-game, no API calls involved!
		/// </summary>
		/// <param name="serviceName"></param>
		public void RemoveNeighbourhood(string serviceName)
		{
			List<NeighbourhoodModel> neighbourhoods = CityManager.Instance.GameModel.Neighbourhoods
				.Where(x => x.Name.Contains(serviceName)).ToList();

			foreach (NeighbourhoodModel neighbourhood in neighbourhoods)
			{
				ApiManager.Instance.ApiUpdateEvent?.Invoke(new UpdateEventModel
					{RemovedNeighbourhood = neighbourhood.Name});
			}
		}


		/// <summary>
		/// Function to remove the first building of a neighbourhood. This only happens in-game, no API calls involved!
		/// </summary>
		/// <param name="serviceName"></param>
		public void RemoveBuilding(string serviceName)
		{
			NeighbourhoodModel neighbourhood = CityManager.Instance.GameModel.Neighbourhoods.FirstOrDefault(x => x.Name == serviceName);
			IVisualizedBuilding building =
				(IVisualizedBuilding) neighbourhood?.VisualizedObjects.FirstOrDefault(x => x is IVisualizedBuilding);
			if (building != null)
			{
				ApiManager.Instance.ApiUpdateEvent?.Invoke(new UpdateEventModel
					{ RemovedVisualizedObject = building.Identifier });
			}
		}

		/// <summary>
		/// Function to search for a neighbourhood.
		/// </summary>
		/// <param name="serviceName"></param>
		public void SearchNeighbourhood(string serviceName)
		{
			NeighbourhoodModel search = CityManager.Instance.GameModel.Neighbourhoods
				.FirstOrDefault(x => x.Name.Contains(serviceName));
			IVisualizedObject randomContainer = search?.VisualizedObjects.FirstOrDefault(x => x.GameObject != null);

			if (randomContainer == null || randomContainer.GameObject == null) return;
			CameraController.Instance.FocusOnTarget(randomContainer.GameObject.transform.position);
			OnObjectClickManager.Instance.ResetHighlighting();
			OnObjectClickManager.Instance.HighlightNeighbourhood(randomContainer.GameObject);
		}

		/// <summary>
		/// Function to enable/disable fps counter.
		/// </summary>
		/// <param name="enabled"></param>
		public void ShowFpsCounter(bool enabled)
		{
			_fpsCounter.SetActive(enabled);
		}

		/// <summary>
		/// Do a barrel roll :)
		/// </summary>
		public void ToggleBarrelRoll()
		{
			foreach (PlaneNavigator airPlane in FindObjectsOfType<PlaneNavigator>())
			{
				airPlane.ToggleBarrelRoll();
			}
		}
	}
}
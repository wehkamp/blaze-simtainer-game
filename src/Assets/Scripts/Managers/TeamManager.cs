using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Models;
using Assets.Scripts.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.Managers
{
	/// <summary>
	/// This class is the team manager for the dropdown in the top-left.
	/// Because we want to make other buildings transparent, we need to change the materials of the building.
	/// Therefor there must be a Transparent- material in the resources/materials folder.
	/// This may not be the prettiest solution, but we can not change the material renderer mode on the fly due to WebGL limitations.
	/// WebGL requires materials to be baked and therefor this solution is used.
	/// </summary>
	internal class TeamManager : Singleton<TeamManager>
	{
		private List<string> _teams = new List<string>();

		public TMP_Dropdown TeamDropdown;

		// Event when the team selection has been changed
		public UnityEvent TeamSelectionChangedEvent = new UnityEvent();

		// Field can be null
		public string SelectedTeam { get; set; }

		// Start is called before the first frame update
		void Start()
		{
			ApiManager.Instance.ApiInitializedEvent.AddListener(SetupTeams);
		}

		/// <summary>
		/// This function retrieves the information from the API initialization which contains all the information about the teams.
		/// </summary>
		/// <param name="gameModel"></param>
		private void SetupTeams(GameModel gameModel)
		{
			// Check if teams are enabled. Settings are already loaded with API initialization.
			if (!SettingsManager.Instance.Settings.Teams.Enabled)
			{
				Destroy(TeamDropdown.gameObject);
				TeamSelectionChangedEvent.RemoveAllListeners();
			}
			else
			{
				_teams = gameModel.Teams.OrderBy(x => x).ToList();
				TeamDropdown.AddOptions(_teams);
				TeamDropdown.onValueChanged.AddListener(OnValueChanged);
				CityManager.Instance.CityUpdatedEvent.AddListener(UpdateOpacity);
				TrafficManager.Instance.VehicleSpawned.AddListener(OnVehicleSpawn);
			}

			ApiManager.Instance.ApiInitializedEvent.RemoveListener(SetupTeams);
		}

		/// <summary>
		/// Event handler when a new vehicle spawns
		/// </summary>
		/// <param name="vehicle"></param>
		private void OnVehicleSpawn(TrafficManager.Vehicle vehicle)
		{
			if (SelectedTeam != null)
			{
				if (vehicle.NeighbourhoodModel.Team != SelectedTeam)
				{
					SetMaterials(vehicle.VehicleGameObject, true);
				}
			}
		}

		/// <summary>
		/// This function is being called by the dropdown menu.
		/// </summary>
		/// <param name="index"></param>
		private void OnValueChanged(int index)
		{
			// Set selected team to null if index = 0, means nothing is selected
			SelectedTeam = index == 0 ? null : _teams[index - 1];
			UpdateOpacity();
			TeamSelectionChangedEvent?.Invoke();
		}

		/// <summary>
		/// Update opacity for all neighbourhoods. Set transparency if the team does not belong to the neighbourhood that is being checked.
		/// </summary>
		private void UpdateOpacity()
		{
			foreach (NeighbourhoodModel neighbourhood in CityManager.Instance.GameModel.Neighbourhoods)
			{
				if (SelectedTeam == null)
					SetMaterials(neighbourhood, false);
				else
					SetMaterials(neighbourhood, SelectedTeam != neighbourhood.Team);
			}
		}

		private void SetMaterials(NeighbourhoodModel neighbourhood, bool fade)
		{
			foreach (IVisualizedObject visualizedObject in neighbourhood.VisualizedObjects.Where(visualizedObject =>
				visualizedObject.GameObject != null))
			{
				SetMaterials(visualizedObject.GameObject, fade);
			}
		}

		/// <summary>
		///  Function to set change materials for a neighbourhood.
		/// </summary>
		/// <param name="gameObj">Set the materials of the given object</param>
		/// <param name="fade">If fade is enabled, the selected neighbourhood will be made transparent</param>
		private void SetMaterials(GameObject gameObj, bool fade)
		{
			Renderer[] childRenderers = gameObj.GetComponentsInChildren<Renderer>();
			if (childRenderers == null) return;

			foreach (Renderer childRenderer in childRenderers)
			{
				if (childRenderer == null || childRenderer.sharedMaterial == null)
					continue;
				string materialName = childRenderer.sharedMaterial.name.Split(' ')[0];

				if (fade)
				{
					// Ugly way of replacing materials, maybe someone knows a better solution for this, but WebGL requires included materials
					// So we can not on the fly change the material to transparency
					if (materialName.StartsWith("Transparent-")) continue;
					childRenderer.sharedMaterial = AssetsManager.Instance.GetMaterial($"Transparent-{materialName}");
					if (childRenderer.sharedMaterial == null)
					{
						Debug.LogError($"Material can not be found! {materialName}");
					}
				}
				else
				{
					if (!materialName.StartsWith("Transparent-")) continue;
					childRenderer.sharedMaterial = AssetsManager.Instance.GetMaterial($"{materialName.Replace("Transparent-", "")}");
				}
			}
		}
	}
}
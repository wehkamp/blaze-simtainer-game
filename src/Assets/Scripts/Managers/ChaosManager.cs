using System;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Models;
using Assets.Scripts.Models.VisualizedObjects;
using Assets.Scripts.Utils;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Components;
using Assets.Scripts.Components.Navigators;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets.Scripts.Managers
{
	/// <summary>
	/// This class is used for chaos engineering in the game. There is a tank driving around and an air plane flying over.
	/// If attack mode is enabled the air plane drops bombs on building and the tank is driving point-to-point and destroying random buildings.
	/// </summary>
	internal class ChaosManager : Singleton<ChaosManager>
	{
		[System.Serializable]
		internal class TargetChangedEvent : UnityEvent<string>
		{
		}

		// UI elements
		public GameObject ToggleAttackModeCheckbox;
		public Image TankUiImage;
		public Image PlaneUiImage;

		public TargetChangedEvent TankTargetChanged;

		public TargetChangedEvent PlaneTargetChangedEvent;

		// Objects
		private GameObject _tankGameObject;
		private GameObject _planeGameObject;
		private TankNavigator _tankNavigator;
		private PlaneNavigator _planeNavigator;

		// Other
		private bool _foundTargets;
		private int _minimumBuildings = 2;

		// Start is called before the first frame update
		void Start()
		{
			GridManager.Instance.GridInitializedEvent.AddListener(GridInitialized);
			TeamManager.Instance.TeamSelectionChangedEvent.AddListener(TeamSelectionChanged);
			AssetsManager.Instance.AssetsLoaded.AddListener(AssetsLoaded);
			CameraManager.Instance.CameraChanged.AddListener(OnCameraChanged);
		}

		private void OnCameraChanged()
		{
			Color planeUiColor = Color.white;
			Color tankUiColor = Color.white;
			switch (CameraManager.Instance.ActiveCameraType)
			{
				case CameraManager.CameraType.PlaneCamera:
					planeUiColor = Color.green;
					break;
				case CameraManager.CameraType.TankCamera:
					tankUiColor = Color.green;
					break;
			}

			if (PlaneUiImage != null)
				PlaneUiImage.color = planeUiColor;
			if (TankUiImage != null)
				TankUiImage.color = tankUiColor;
		}

		private void AssetsLoaded()
		{
			TankUiImage.sprite =
				AssetsManager.Instance.GetVehicleSpritesByType(SettingsManager.Instance.Settings.AssetBundle.Chaos
					.TankPrefab);
			PlaneUiImage.sprite =
				AssetsManager.Instance.GetVehicleSpritesByType(SettingsManager.Instance.Settings.AssetBundle.Chaos
					.PlanePrefab);
		}

		/// <summary>
		/// City is initialized so now we start spawning our tank and air plane.
		/// </summary>
		private void GridInitialized()
		{
			// Check if chaos is enabled. Settings are already loaded on grid initialization.
			if (SettingsManager.Instance.Settings.Chaos.Enabled)
			{
				if (SettingsManager.Instance.Settings.Chaos.TankEnabled)
				{
					KeyValuePair<Vector3, Quaternion> spawnPoint =
						GridManager.Instance.VehicleSpawnPoints.PickRandom();

					// Initialize Tank
					_tankGameObject = Instantiate(
						AssetsManager.Instance.GetPredefinedPrefab(AssetsManager.PrefabType.Tank),
						spawnPoint.Key,
						spawnPoint.Value);
					_tankNavigator = _tankGameObject.AddComponent<TankNavigator>();
					CameraManager.Instance.TankCamera = _tankGameObject.GetComponentInChildren<Camera>();
					CameraManager.Instance.TankCamera.gameObject.SetActive(false);
				}
				else
				{
					Destroy(TankUiImage);
				}

				if (SettingsManager.Instance.Settings.Chaos.PlaneEnabled)
				{
					// Initialize Plane
					// Plane is determining it's own spawn point
					_planeGameObject = Instantiate(
						AssetsManager.Instance.GetPredefinedPrefab(AssetsManager.PrefabType.Plane),
						new Vector3(0f, 80f, 0f), Quaternion.Euler(0f, 90f, 0f));
					_planeNavigator = _planeGameObject.AddComponent<PlaneNavigator>();
					CameraManager.Instance.PlaneCamera = _planeGameObject.GetComponentInChildren<Camera>();
					CameraManager.Instance.PlaneCamera.gameObject.SetActive(false);
				}
				else
				{
					Destroy(PlaneUiImage);
				}

				_minimumBuildings = SettingsManager.Instance.Settings.Chaos.MinimumBuildings;
			}
			else
			{
				Destroy(ToggleAttackModeCheckbox);
			}

			CityManager.Instance.CityUpdatedEvent.RemoveListener(GridInitialized);
		}

		/// <summary>
		/// Function to enable or disable firing on targets.
		/// </summary>
		/// <param name="firingEnabled"></param>
		public void SetFiringEnabled(bool firingEnabled)
		{
			if (_tankGameObject != null)
				_tankNavigator.IsFiringEnabled = firingEnabled;
			if (_planeGameObject != null)
				_planeNavigator.IsFiringEnabled = firingEnabled;
		}

		/// <summary>
		/// Check if the tank still has a target. Otherwise pick a new target.
		/// It won't check for a new target is the tank is in stand-by.
		/// </summary>
		void LateUpdate()
		{
			// We need to pick a new target for the tank here
			if (_tankNavigator != null && _tankNavigator.Target == null && !_tankNavigator.IsStandby)
			{
				IVisualizedObject target = SelectTarget();
				if (target == null || !_foundTargets)
				{
					_tankNavigator.SetStandby(true);
				}
				else
				{
					_tankNavigator.SetTarget(target);
				}
			}
		}

		/// <summary>
		/// Team selection has changed, so we want to clear the target of the tank.
		/// Target is determined on a team, if a team is selected of course.
		/// </summary>
		private void TeamSelectionChanged()
		{
			_tankNavigator.RemoveTarget();
			_tankNavigator.SetStandby(false);
		}

		/// <summary>
		/// Function to select a target for the tank.
		/// It requires at least 2 buildings from a neighbourhood to be eligible.
		/// </summary>
		/// <returns></returns>
		private IVisualizedObject SelectTarget()
		{
			List<NeighbourhoodModel> randomNeighbourhoodQuery = CityManager.Instance.GameModel.Neighbourhoods
				.Where(neighbourhoodModel =>
					neighbourhoodModel.VisualizedObjects.Count(visualizedObject =>
						visualizedObject is VisualizedBuildingModel) >= _minimumBuildings).ToList();
			if (TeamManager.Instance.SelectedTeam != null)
			{
				randomNeighbourhoodQuery =
					randomNeighbourhoodQuery.Where(x => x.Team == TeamManager.Instance.SelectedTeam).ToList();
			}

			// No target found. Tank is probably going in standby mode now.
			if (randomNeighbourhoodQuery.Count == 0)
			{
				_foundTargets = false;
				return null;
			}

			// Pick a random neighbourhood from the list
			NeighbourhoodModel randomNeighbourhoodModel =
				randomNeighbourhoodQuery.PickRandom();

			// We found a target and it's a building. Return the target.
			IVisualizedObject obj = randomNeighbourhoodModel.VisualizedObjects
				.First(visualizedObject =>
					visualizedObject.GameObject != null && visualizedObject is VisualizedBuildingModel);
			_foundTargets = true;
			return obj;
		}

		public void GoToTankCamera()
		{
			if (CameraManager.Instance.ActiveCameraType == CameraManager.CameraType.TankCamera)
			{
				CameraManager.Instance.SwitchCamera(CameraManager.CameraType.MainCamera);
			}
			else
			{
				CameraManager.Instance.SwitchCamera(CameraManager.CameraType.TankCamera);
			}
		}

		public void GoToPlaneCamera()
		{
			if (CameraManager.Instance.ActiveCameraType == CameraManager.CameraType.PlaneCamera)
			{
				CameraManager.Instance.SwitchCamera(CameraManager.CameraType.MainCamera);
			}
			else
			{
				CameraManager.Instance.SwitchCamera(CameraManager.CameraType.PlaneCamera);
			}
		}
	}
}
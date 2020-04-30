using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Components.Navigators;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Models;
using Assets.Scripts.Models.Layers;
using Assets.Scripts.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.Managers
{
	/// <summary>
	/// This class will take care of the click events that happen.
	/// </summary>
	internal class OnObjectClickManager : Singleton<OnObjectClickManager>
	{
		public GameObject InfoPanel;

		private readonly Dictionary<Renderer, Color> _clickedObjects = new Dictionary<Renderer, Color>();

		private IVisualizedObject _selectedObject;
		private int _defaultLayer;

		private float _maxDistance;

		public GameObject DestroyButton;

		// Start is called before the first frame update
		void Start()
		{
			_defaultLayer = LayerMask.GetMask("Default");
			_maxDistance = Mathf.Infinity;

			// Check with both events if renderers are removed
			CityManager.Instance.CityUpdatedEvent.AddListener(RemovedRenderersCheck);
			TrafficManager.Instance.TrafficUpdateEvent.AddListener(RemovedRenderersCheck);
		}

		/// <summary>
		/// Function to remove renderers that do not exists anymore.
		/// </summary>
		void RemovedRenderersCheck()
		{
			foreach (Renderer key in _clickedObjects.Keys.ToList().Where(key => key == null))
			{
				_clickedObjects.Remove(key);
				if (InfoPanel != null)
					InfoPanel.SetActive(false);
			}
		}

		// Update is called once per frame
		void Update()
		{
			// Check if left mouse button is pressed and if there has not been clicked on an UI element
			if (!Input.GetMouseButtonDown(0) ||
			    UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;

			// Check if we have a hit on a prefab
			bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hitInfo,
				_maxDistance,
				_defaultLayer);

			// We reset the highlighting
			ResetHighlighting();
			_selectedObject = null;
			if (hit)
			{
				DestroyButton.SetActive(false);
				GameObject targetObject = hitInfo.transform.gameObject;
				// Check if the object we hit has a parent, since buildings, vehicle etc don't have parents but they do have children
				if (hitInfo.transform.parent != null)
				{
					targetObject = hitInfo.transform.parent.gameObject;
				}

				if ((targetObject.CompareTag("Building") || targetObject.CompareTag("DestroyedBuilding")) &&
				    targetObject.name.StartsWith("neighbourhood-"))
				{
					HighlightNeighbourhood(targetObject);
				}
				else if (targetObject.CompareTag("Tank") || targetObject.CompareTag("Vehicle"))
				{
					HighlightVehicle(targetObject);
				}
				else
				{
					InfoPanel.gameObject.SetActive(false);
				}
			}
			else
			{
				InfoPanel.gameObject.SetActive(false);
			}
		}

		/// <summary>
		/// Function to highlight a neighbourhood when clicked.
		/// </summary>
		/// <param name="clickedObject"></param>
		/// <param name="showInfoPanel"></param>
		public void HighlightNeighbourhood(GameObject clickedObject, bool showInfoPanel = true)
		{
			if (showInfoPanel)
			{
				string neighbourhoodName = clickedObject.name.Replace("neighbourhood-", "");
				string infoText = $"Neighbourhood: {neighbourhoodName}";
				try
				{
					if (clickedObject.CompareTag("Building"))
					{
						NeighbourhoodModel neighboorhoud =
							CityManager.Instance.GameModel.Neighbourhoods.SingleOrDefault(
								(x => x.Name == neighbourhoodName));
						if (neighboorhoud != null)
						{
							IVisualizedObject visualizedObject =
								neighboorhoud.VisualizedObjects.SingleOrDefault(x => x.GameObject == clickedObject);
							_selectedObject = visualizedObject;
							string days = neighboorhoud.Age == 1 ? "day" : "days";
							infoText =
								$"Neighbourhood: {neighbourhoodName}\r\nAge: {neighboorhoud.Age} {days}";
							VisualLayerModel selectedLayer = LayerManager.Instance.SelectedLayer;
							if (visualizedObject is IVisualizedBuilding visualizedBuilding)
							{
								DestroyButton.SetActive(true);
								if (selectedLayer != null)
								{
									// Only enable destroy button for buildings
									infoText =
										$"Neighbourhood: {neighbourhoodName}\r\n{selectedLayer.Name.Replace("Layer", "")}: {visualizedBuilding.LayerValues[selectedLayer.Name]} / {neighboorhoud.LayerValues.Single(x => x.LayerType == selectedLayer.Name).MaxValue} ";
								}
							}
						}
					}
				}
				catch (Exception e)
				{
					Debug.LogError($"[OnObjectClickManager] {e.Message}");
				}
				finally
				{
					InfoPanel.GetComponentInChildren<TMP_Text>().text = infoText;
					InfoPanel.gameObject.SetActive(true);
				}
			}

			Renderer[] renderComponents = clickedObject.GetComponentsInChildren<Renderer>();
			foreach (Renderer renderComponent in renderComponents)
			{
				_clickedObjects.Add(renderComponent, renderComponent.material.color);
				renderComponent.material.color = Color.green;
			}
		}


		/// <summary>
		/// Function to highlight vehicles when clicked.
		/// </summary>
		/// <param name="clickedObject"></param>
		public void HighlightVehicle(GameObject clickedObject)
		{
			InfoPanel.gameObject.SetActive(true);
			try
			{
				string infoText = string.Empty;
				if (clickedObject.CompareTag("Tank"))
				{
					TankNavigator tankNavigator = clickedObject.GetComponent<TankNavigator>();
					if (tankNavigator.IsStandby || tankNavigator.Target == null)
						infoText = "Tank is in stand-by mode";
					else
					{
						NeighbourhoodModel neighbourhoodModel = CityManager.Instance.GameModel.Neighbourhoods
							.Select(x => x).SingleOrDefault(x =>
								x.VisualizedObjects.Contains(tankNavigator.Target));
						if (neighbourhoodModel != null)
							infoText = $"Target: {neighbourhoodModel.Name}";
					}
				}
				else
				{
					TrafficManager.Vehicle v =
						TrafficManager.Instance.Vehicles.Single(x => x.VehicleGameObject == clickedObject);

					// Check if vehicle is visible
					if (clickedObject.GetComponentsInChildren<Renderer>().Any(x => !x.enabled)) return;

					infoText = v.NeighbourhoodModel == null
						? "Vehicle driving towards deleted neighbourhood"
						: $"Vehicle driving towards {v.NeighbourhoodModel.Name}";
				}

				InfoPanel.GetComponentInChildren<TMP_Text>().text = infoText;
				NavMeshAgent agent = clickedObject.GetComponent<NavMeshAgent>();
				if (agent != null)
				{
					CameraController.Instance.FollowTarget(clickedObject, agent.speed * 0.1f);
				}

				Renderer[] renderComponents = clickedObject.GetComponentsInChildren<Renderer>();
				foreach (Renderer renderComponent in renderComponents)
				{
					_clickedObjects.Add(renderComponent, renderComponent.material.color);
					renderComponent.material.color = Color.green;
				}
			}
			catch (Exception e)
			{
				Debug.LogError($"[OnObjectClickManager] {e.Message}");
			}
		}

		/// <summary>
		/// Function to reset all highlighting and stop following targets.
		/// </summary>
		public void ResetHighlighting()
		{
			// Reset all color's of all objects
			foreach (Renderer key in _clickedObjects.Keys.ToList().Where(key => key != null && key.gameObject != null))
			{
				key.material.color = _clickedObjects[key];
			}

			_clickedObjects.Clear();
			CameraController.Instance.StopFollowingTarget();
		}

		/// <summary>
		/// Function to destroy an object that is selected. Called by the destroy button.
		/// </summary>
		public void DestroySelectedObject()
		{
			if (_selectedObject != null)
				StartCoroutine(ApiManager.Instance.KillVisualizedObject(_selectedObject, true));
			InfoPanel.SetActive(false);
		}
	}
}
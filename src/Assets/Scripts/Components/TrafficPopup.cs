using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Managers;
using Assets.Scripts.Models;
using Assets.Scripts.Models.VisualizedObjects;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.Components
{
	internal class TrafficPopup : MonoBehaviour
	{
		public GameObject ViewContent;

		public Dictionary<string, GameObject> InfoGameObjects = new Dictionary<string, GameObject>();

		public GameObject TrafficOverviewRowPrefab;

		void OnDisable()
		{
			TrafficManager.Instance.TrafficUpdateEvent.RemoveListener(UpdatePopup);
			TeamManager.Instance.TeamSelectionChangedEvent.RemoveListener(UpdatePopup);
		}

		void OnEnable()
		{
			UpdatePopup();
			TrafficManager.Instance.TrafficUpdateEvent.AddListener(UpdatePopup);
			TeamManager.Instance.TeamSelectionChangedEvent.AddListener(UpdatePopup);
		}

		private void ResetRows()
		{
			foreach (KeyValuePair<string, GameObject> infoGameObject in InfoGameObjects)
			{
				Destroy(infoGameObject.Value);
			}

			InfoGameObjects.Clear();
		}

		/// <summary>
		/// Function to refresh the popup with new information.
		/// </summary>
		private void UpdatePopup()
		{
			IEnumerable<TrafficManager.Vehicle> vehicles = TrafficManager.Instance.Vehicles.OrderBy(x =>
				x.NeighbourhoodModel.Name).Where(x => x.VehicleModel.Size > 0);
			if (TeamManager.Instance.SelectedTeam != null)
			{
				ResetRows();
				vehicles = vehicles.Where(x => x.NeighbourhoodModel.Team == TeamManager.Instance.SelectedTeam);
			}

			foreach (TrafficManager.Vehicle vehicle in vehicles)
			{
				if (vehicle.VehicleGameObject != null)
				{
					AddRow(AssetsManager.Instance.GetVehicleSpritesByType(vehicle.VehicleName), vehicle.NeighbourhoodModel.Name, vehicle.VehicleModel,
						vehicle.VehicleGameObject);
				}
			}
		}

		/// <summary>
		/// Function to add a row to the table of traffic.
		/// </summary>
		/// <param name="sprite"></param>
		/// <param name="service"></param>
		/// <param name="vehicleModel"></param>
		/// <param name="vehicleGameObject"></param>
		private void AddRow(Sprite sprite, string service, VisualizedVehicleModel vehicleModel,
			GameObject vehicleGameObject)
		{
			GameObject trafficRow;
			// Check if we already have this row, so we only need to update the object?
			if (InfoGameObjects.ContainsKey(vehicleModel.Identifier))
			{
				trafficRow = InfoGameObjects[vehicleModel.Identifier];
			}
			else
			{
				// Row does not exists, create one
				trafficRow =
					Instantiate(TrafficOverviewRowPrefab);

				// Add an event trigger object so we can make the button clickable
				EventTrigger eventTrigger = trafficRow.AddComponent<EventTrigger>();
				EventTrigger.Entry entry = new EventTrigger.Entry {eventID = EventTriggerType.PointerClick};
				entry.callback.AddListener(eventData =>
				{
					OnObjectClickManager.Instance.HighlightVehicle(vehicleGameObject);
					gameObject.SetActive(false);
				});
				eventTrigger.triggers.Add(entry);
				trafficRow.transform.SetParent(ViewContent.transform, false);
				InfoGameObjects.Add(vehicleModel.Identifier, trafficRow);
			}

			TMP_Text[] texts = trafficRow.GetComponentsInChildren<TMP_Text>();
			Image image = trafficRow.GetComponentInChildren<Image>();

			// Set the correct text. Row 0 = name of the service, Row 1 = the size of the service
			texts[0].text = service;
			texts[1].text = vehicleModel.Size.ToString();

			// Set the correct sprite
			image.sprite = sprite;
		}
	}
}
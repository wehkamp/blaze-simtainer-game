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
			foreach (var vehiclePrefab in SettingsManager.Instance.Settings.AssetBundle.Vehicles)
			{
			}

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

		private void UpdatePopup()
		{
			IEnumerable<TrafficManager.Vehicle> vehicles = TrafficManager.Instance.Vehicles.OrderBy(x =>
				x.neighbourhoodModel.Name).Where(x => x.vehicleModel.Size > 0);
			if (TeamManager.Instance.SelectedTeam != null)
			{
				ResetRows();
				vehicles = vehicles.Where(x => x.neighbourhoodModel.Team == TeamManager.Instance.SelectedTeam);
			}

			foreach (TrafficManager.Vehicle vehicle in vehicles)
			{
				if (vehicle.gameObject != null)
				{
					AddRow(AssetsManager.Instance.GetVehicleSpritesByType(vehicle.defaultPrefabName), vehicle.neighbourhoodModel.Name, vehicle.vehicleModel,
						vehicle.gameObject);
				}
			}
		}


		private void AddRow(Sprite sprite, string service, VisualizedVehicleModel vehicleModel,
			GameObject vehicleGameObject)
		{
			GameObject trafficRow;
			TMP_Text[] texts;
			// We already have this row, so we only update the object?
			if (InfoGameObjects.ContainsKey(vehicleModel.Identifier))
			{
				trafficRow = InfoGameObjects[vehicleModel.Identifier];
			}
			else
			{
				trafficRow =
					Instantiate(TrafficOverviewRowPrefab);
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

			texts = trafficRow.GetComponentsInChildren<TMP_Text>();
			Image image = trafficRow.GetComponentInChildren<Image>();

			texts[0].text = service;
			texts[1].text = vehicleModel.Size.ToString();

			image.sprite = sprite;
		}
	}
}
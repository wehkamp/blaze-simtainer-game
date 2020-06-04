using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Managers;
using Assets.Scripts.Models.Settings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Components
{
	/// <summary>
	/// This class takes care of the counters on the bottom panel.
	/// It counts all the traffic and keeps the counters up-to-date through events.
	/// </summary>
	internal class TrafficPanel : MonoBehaviour
	{
		public GameObject BottomPanel;

		public GameObject Image;

		// String represents the name of the vehicle (e.g. Truck)
		private readonly Dictionary<string, TrafficSprite> _trafficCount =
			new Dictionary<string, TrafficSprite>();

		private float _widthDeltaX;

		// We do not use a struct here since we want to change the count on-the-fly
		private class TrafficSprite
		{
			public GameObject VehicleGameObject { get; set; }
			public int Count { get; set; }
		}

		// Start is called before the first frame update
		void Start()
		{
			ResetAllCounters();
			AssetsManager.Instance.AssetsLoaded.AddListener(AssetsLoaded);
		}

		/// <summary>
		/// This function will 
		/// </summary>
		private void AssetsLoaded()
		{
			foreach (VehiclePrefab vehicle in SettingsManager.Instance.Settings.AssetBundle.Vehicles)
			{
				// Create an image object and add it to the bottom bar
				GameObject g = Instantiate(Image, BottomPanel.transform);
				Image img = g.GetComponent<Image>();
				img.overrideSprite = AssetsManager.Instance.GetVehicleSpritesByType(vehicle.Name);

				RectTransform rt = g.GetComponent<RectTransform>();

				rt.localPosition = new Vector3(rt.localPosition.x + _widthDeltaX, rt.localPosition.y,
					rt.localPosition.z);

				_widthDeltaX += 80f;
				_trafficCount.Add(vehicle.Name, new TrafficSprite {VehicleGameObject = g, Count = 0});
			}

			TrafficManager.Instance.TrafficUpdateEvent.AddListener(UpdateCounters);
			TeamManager.Instance.TeamSelectionChangedEvent.AddListener(UpdateCounters);
		}

		/// <summary>
		/// Function to update all the counters in the bottom panel.
		/// Only is called when the team is changed or the traffic manager received an update.
		/// </summary>
		private void UpdateCounters()
		{
			IEnumerable<TrafficManager.Vehicle> vehicles = TrafficManager.Instance.Vehicles;

			// Reset all counters to 0
			ResetAllCounters();

			// Check if a team is selected, if so we want to filter all traffic for the selected team
			if (TeamManager.Instance.SelectedTeam != null)
			{
				vehicles = vehicles.Where(x => x.NeighbourhoodModel.Team == TeamManager.Instance.SelectedTeam);
			}

			// First count all vehicles by their name
			foreach (TrafficManager.Vehicle vehicle in vehicles)
			{
				_trafficCount[vehicle.VehicleName].Count++;
			}

			// Now set the right text
			foreach (KeyValuePair<string, TrafficSprite> kv in _trafficCount)
			{
				kv.Value.VehicleGameObject.GetComponentInChildren<TMP_Text>().text =
					kv.Value.Count.ToString();
			}
		}

		/// <summary>
		/// Function to reset all the counters to 0
		/// </summary>
		private void ResetAllCounters()
		{
			foreach (KeyValuePair<string, TrafficSprite> kv in _trafficCount)
			{
				ResetCounter(kv.Value);
			}
		}

		/// <summary>
		/// Function to reset the count to 0 for a traffic sprite
		/// </summary>
		/// <param name="trafficSprite"></param>
		private static void ResetCounter(TrafficSprite trafficSprite)
		{
			trafficSprite.Count = 0;
			trafficSprite.VehicleGameObject.GetComponentInChildren<TMP_Text>().text = trafficSprite.Count.ToString();
		}
	}
}
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

		// String represents the name of the vehicle's prefab
		private readonly Dictionary<string, TrafficSprite> _trafficCount =
			new Dictionary<string, TrafficSprite>();

		private float _widthDeltaX = 0f;

		private class TrafficSprite
		{
			public GameObject Obj { get; set; }
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
				GameObject g = Instantiate(Image, BottomPanel.transform);
				Image img = g.GetComponent<Image>();
				img.overrideSprite = AssetsManager.Instance.GetVehicleSpritesByType(vehicle.Name);

				RectTransform rt = g.GetComponent<RectTransform>();

				rt.localPosition = new Vector3(rt.localPosition.x + _widthDeltaX, rt.localPosition.y,
					rt.localPosition.z);

				_widthDeltaX += 80f;
				_trafficCount.Add(vehicle.Name, new TrafficSprite {Obj = g, Count = 0});
			}

			TrafficManager.Instance.TrafficUpdateEvent.AddListener(UpdateCounters);
			TeamManager.Instance.TeamSelectionChangedEvent.AddListener(UpdateCounters);
		}

		private void UpdateCounters()
		{
			IEnumerable<TrafficManager.Vehicle> vehicles = TrafficManager.Instance.Vehicles;
			ResetAllCounters();
			if (TeamManager.Instance.SelectedTeam != null)
			{
				vehicles = vehicles.Where(x => x.neighbourhoodModel.Team == TeamManager.Instance.SelectedTeam);
			}

			foreach (TrafficManager.Vehicle vehicle in vehicles)
			{
				IncrementText(_trafficCount[vehicle.defaultPrefabName]);
			}
		}

		private void ResetAllCounters()
		{
			foreach (KeyValuePair<string, TrafficSprite> kv in _trafficCount)
			{
				ResetCounter(kv.Value);
			}
		}

		private static void ResetCounter(TrafficSprite trafficSprite)
		{
			trafficSprite.Count = 0;
			trafficSprite.Obj.GetComponentInChildren<TMP_Text>().text = trafficSprite.Count.ToString();
		}

		private static void IncrementText(TrafficSprite trafficSprite)
		{
			trafficSprite.Count++;
			trafficSprite.Obj.GetComponentInChildren<TMP_Text>().text = trafficSprite.Count.ToString();
		}
	}
}
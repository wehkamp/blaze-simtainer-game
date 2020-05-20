using System;
using System.Linq;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Managers;
using Assets.Scripts.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Components.Navigators
{
	/// <summary>
	/// This class is used for the plane to navigate through the map
	/// </summary>
	internal class PlaneNavigator : MonoBehaviour
	{
		// Speed
		public float Speed = 30f;

		// Is firing enabled or not
		public bool IsFiringEnabled;

		// Height of the plane
		public float PlaneHeight = 80f;

		// Layer mask that is being used for the Raycast
		private int _layerMask;

		// This property must be set when the airplane fired something so it will not fire again
		private bool _fired;

		private GameObject _propeller;

		// Do a barrel roll
		private bool _barrelRoll;

		// Keep the euler angles in memory so we can reset the plane after a barrel roll
		private Vector3 _currentEulerAngles;

		// X border of the plane
		private int _maxX;

		// Z border of the plane
		private int _maxZ;

		/// <summary>
		/// Amount of buildings that must exists before we drop a bomb on a neighbourhood.
		/// </summary>
		public int MinimumBuildings = 2;

		// Start is called before the first frame update
		void Start()
		{
			// Get the grid boundaries
			(int maxX, int maxZ) = BoundariesUtil.CalculateMaxGridBoundaries(GridManager.Instance);
			_maxX = maxX;
			_maxZ = maxZ;

			// Set the correct LayerMask used in Raycasting
			_layerMask = LayerMask.GetMask("Default");

			// Select a random spawn position
			(Vector3 randomPos, Quaternion rotation) = SelectSpawnPoint();
			transform.position = randomPos;
			transform.rotation = rotation;

			// Loop through all children and check if a Proppeler exists
			for (int i = 0; i < transform.childCount; i++)
			{
				if (transform.GetChild(i).name == "Proppeler002")
				{
					_propeller = transform.GetChild(i).gameObject;
				}
			}

			// Set the minimum Buildings required for chaos
			MinimumBuildings = SettingsManager.Instance.Settings.Chaos.MinimumBuildings;
		}

		public void ToggleBarrelRoll()
		{
			_barrelRoll = !_barrelRoll;

			// Set plane back to normal angles
			if (!_barrelRoll)
				transform.eulerAngles = _currentEulerAngles;
		}

		/// <summary>
		/// Function to select a spawn point for the airplane.
		/// </summary>
		/// <returns></returns>
		Tuple<Vector3, Quaternion> SelectSpawnPoint()
		{
			// if Z is 500, we select something between the -500 & 500
			int[] randomZ = {_maxX * (-1), _maxX};
			// Pick a random value between the for example -500 & 500
			int randValue = Random.Range(0, randomZ.Length);
			Quaternion rotation;
			int x = Random.Range(0, 499);
			int z = randomZ[randValue];

			// Set the correct rotation for the plane.
			if (x > 10 && z < 10)
				rotation = Quaternion.Euler(0f, 180f, 0f);
			else if (x > 10 && z > 10)
				rotation = Quaternion.Euler(0f, 180f, 0f);
			else if (x < 10 && z > 10)
				rotation = Quaternion.Euler(0f, 180f, 0f);
			else
				rotation = Quaternion.Euler(0f, 180f, 0f);

			return new Tuple<Vector3, Quaternion>(new Vector3(x, PlaneHeight, z), rotation);
		}

		void LateUpdate()
		{
			// Check if the plane is flying out of the boundaries, if so reset the position of the plane
			if (transform.position.z > _maxZ + 100 || transform.position.x > _maxX + 100 ||
			    transform.position.z < -_maxX - 100 ||
			    transform.position.x < -_maxX - 100)
			{
				// We are crossing the border, set new a position
				transform.position = SelectSpawnPoint().Item1;
				transform.rotation = SelectSpawnPoint().Item2;

				_currentEulerAngles = transform.eulerAngles;

				// Reset fired for this path
				_fired = false;
			}
		}

		// Check if we have a hit on a building
		void Update()
		{
			// Move the plane forward
			if (transform.rotation.eulerAngles.y > 10)
				transform.Translate(transform.forward * -Speed * Time.deltaTime);
			else
				transform.Translate(transform.forward * Speed * Time.deltaTime);

			// Do a barrel roll :)
			if (_barrelRoll)
			{
				Vector3 newPlaneAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y,
					transform.eulerAngles.z + 1f);
				transform.eulerAngles = newPlaneAngles;
			}

			// Calculate rotation and rotate the propeller
			if (_propeller != null)
			{
				Vector3 newPropellerAngles = new Vector3(_propeller.transform.eulerAngles.x + 10f,
					_propeller.transform.eulerAngles.y,
					_propeller.transform.eulerAngles.z);
				_propeller.transform.eulerAngles = newPropellerAngles;
			}

			// If we already fired or attack mode isn't turned on, return
			if (_fired || !IsFiringEnabled)
				return;

			// Check if we are flying over a building
			if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 200f, _layerMask))
			{
				// Chance of 1 in 100 every frame so still quite a big chance
				bool randomFiringEnabled = Random.Range(0, 100) == 1;

				// Check if we hit a building with our raycast and check if the building has the correct requirements
				if (hit.collider.gameObject.CompareTag("Building") && randomFiringEnabled && CityManager
					    .Instance.GameModel
					    .Neighbourhoods
					    .Single(x => x.Name == hit.collider.gameObject.name.Replace("neighbourhood-", ""))
					    .VisualizedObjects.Count(x=>x is IVisualizedBuilding) >= MinimumBuildings)
				{
					// We met the requirements Drop a bomb on the building
					_fired = true;
					Instantiate(AssetsManager.Instance.GetPredefinedPrefab(AssetsManager.PrefabType.Bomb),
						new Vector3(hit.collider.gameObject.transform.position.x, transform.position.y,
							hit.collider.gameObject.transform.position.z), Quaternion.identity);
					Debug.Log($"HIT! {hit.collider.gameObject.name}");
				}
			}
		}
	}
}
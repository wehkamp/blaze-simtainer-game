using System;
using System.Linq;
using Assets.Scripts.Managers;
using Assets.Scripts.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Components.Navigators
{
	internal class PlaneNavigator : MonoBehaviour
	{
		// Speed
		public float Speed = 30f;

		// Is firing enabled or not
		public bool IsFiringEnabled;

		// Height of the plane
		public float PlaneHeight = 80f;

		private int _layerMask;

		private bool _fired;

		private GameObject _propeller;

		private bool _barrelRoll;

		private Vector3 _currentEulerAngles;

		private bool _randomFiringEnabled;

		private int _maxX;

		private int _maxZ;

		/// <summary>
		/// Amount of buildings that must exists before we drop a bomb on a neighbourhood.
		/// </summary>
		public int MinimumBuildings = 2;

		// Start is called before the first frame update
		void Start()
		{
			(int maxX, int maxZ) = BoundariesUtil.CalculateMaxGridBoundaries(GridManager.Instance);
			_maxX = maxX;
			_maxZ = maxZ;
			_layerMask = LayerMask.GetMask("Default");
			(Vector3 randomPos, Quaternion rotation) = SelectSpawnPoint();
			transform.position = randomPos;
			transform.rotation = rotation;

			for (int i = 0; i < transform.childCount; i++)
			{
				if (transform.GetChild(i).name == "Proppeler002")
				{
					_propeller = transform.GetChild(i).gameObject;
				}
			}

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
			// if Z is 500, we select -500 & 500
			int[] randomZ = {_maxX * (-1), _maxX};
			int randValue = Random.Range(0, randomZ.Length);
			Quaternion rotation;
			int x = Random.Range(0, 499);
			int z = randomZ[randValue];

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
				transform.position = SelectSpawnPoint().Item1;
				transform.rotation = SelectSpawnPoint().Item2;

				_currentEulerAngles = transform.eulerAngles;
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

			if (_fired || !IsFiringEnabled)
				return;

			if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 200f, _layerMask))
			{
				// Chance of 1 in 100 every frame so still quite a big chance
				_randomFiringEnabled = Random.Range(0, 100) == 1;
				if (hit.collider.gameObject.CompareTag("Building") && _randomFiringEnabled && CityManager
					.Instance.GameModel
					.Neighbourhoods
					.Single(x => x.Name == hit.collider.gameObject.name.Replace("neighbourhood-", ""))
					.VisualizedObjects.Count >= MinimumBuildings)
				{
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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Models.Settings;
using Assets.Scripts.Utils;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace Assets.Scripts.Managers
{
	/// <summary>
	/// This class is used to return prefab types.
	/// Because there are a lot of prefabs in use, it is handier to use this kind of class for loading resources from the right paths
	/// </summary>
	internal class AssetsManager : Singleton<AssetsManager>
	{
		private readonly Dictionary<string, GameObject> _prefabCacheDictionary = new Dictionary<string, GameObject>();
		private readonly Dictionary<string, Material> _materialCacheDictionary = new Dictionary<string, Material>();
		private readonly Dictionary<string, Sprite> _spriteCacheDictionary = new Dictionary<string, Sprite>();
		public UnityEvent AssetsLoaded;

		private AssetBundleSettings _assetBundleSettingsSettings;

		private AssetBundle _assetBundle;


		private IOrderedEnumerable<BuildingPrefab> _buildingPrefabs;
		private IOrderedEnumerable<VehiclePrefab> _vehiclePrefabs;
		private int _buildingDecayedThreshold;

		void Start()
		{
			SettingsManager.Instance.SettingsLoadedEvent.AddListener(SettingsLoaded);
		}

		private IEnumerator LoadAssetBundle(string url)
		{
			Debug.Log("Starting web request");
			using (UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(url))
			{
				uwr.timeout = 60;
				yield return uwr.SendWebRequest();

				if (uwr.isNetworkError || uwr.isHttpError)
				{
					Debug.Log(uwr.error);
				}
				else
				{
					// Get downloaded asset bundle
					_assetBundle = DownloadHandlerAssetBundle.GetContent(uwr);
					Debug.Log("Assets loaded");
					OnAssetsLoaded();
				}
			}
		}

		private void SettingsLoaded()
		{
			_assetBundleSettingsSettings = SettingsManager.Instance.Settings.AssetBundle;
			// Load the Asset bundles

			if (_assetBundleSettingsSettings.Name.StartsWith("http"))
			{
				StartCoroutine(LoadAssetBundle(_assetBundleSettingsSettings.Name));
			}
			else if (Application.platform == RuntimePlatform.WebGLPlayer)
			{
				StartCoroutine(LoadAssetBundle("AssetBundles/" + _assetBundleSettingsSettings.Name));
			}
			else
			{
#if UNITY_EDITOR
				_assetBundle = AssetBundle.LoadFromFile($"Assets/AssetBundles/{_assetBundleSettingsSettings.Name}");
#else
				_assetBundle = AssetBundle.LoadFromFile($"{Application.dataPath}/{_assetBundleSettingsSettings.Name}");
#endif
				OnAssetsLoaded();
			}


			// Invoke the event handler to let other managers know the assets bundle is loaded

			// Make lists of prefabs by a descending ordering
			_buildingPrefabs = _assetBundleSettingsSettings.Buildings.OrderByDescending(x => x.MinSize);
			_vehiclePrefabs = _assetBundleSettingsSettings.Vehicles.OrderByDescending(x => x.MinSize);
			_buildingDecayedThreshold = _assetBundleSettingsSettings.BuildingDecayAgeThreshold;
		}

		private void OnAssetsLoaded()
		{
			_assetBundle.LoadAllAssets();
			// Generate vehicle sprites for the UI
			GenerateVehicleSprites();
			// Fix shaders of all materials
			RefreshShaders();
			AssetsLoaded?.Invoke();
		}

		private void RefreshShaders()
		{
			// Bug due to materials: https://answers.unity.com/questions/238109/problem-with-materials-after-creating-asset-bundle.html
			// We need to refresh the shaders, otherwise the transparency does not work
			UnityEngine.Object[] materials = _assetBundle.LoadAllAssets(typeof(Material));
			foreach (UnityEngine.Object o in materials)
			{
				Material m = (Material) o;
				string shaderName = m.shader.name;
				Shader newShader = Shader.Find(shaderName);

				if (newShader != null)
				{
					m.shader = newShader;
				}
				else
				{
					Debug.LogWarning("unable to refresh shader: " + shaderName + " in material " + m.name);
				}

				_materialCacheDictionary.Add(m.name.Split(' ')[0], m);
			}
		}

		/// <summary>
		/// Function that return a building object together with the rotation of a building
		/// </summary>
		/// <param name="size"></param>
		/// <param name="age"></param>
		/// <returns></returns>
		public Tuple<GameObject, float> GetBuildingPrefab(int size, int age)
		{
			GameObject g;
			// return GetPrefab(_assetBundleSettingsSettings.Buildings.First().PrefabName);
			foreach (BuildingPrefab buildingPrefab in _buildingPrefabs)
			{
				if (size > buildingPrefab.MinSize)
				{
					Prefab prefab;
					if (age < _buildingDecayedThreshold)
					{
						prefab = buildingPrefab.Prefabs.PickRandom();
					}
					else
					{
						prefab = buildingPrefab.DecayedPrefabs.PickRandom();
					}

					g = GetPrefab(prefab.Name);
					return new Tuple<GameObject, float>(g, prefab.Rotation);
				}
			}

			return null;
		}

		public GameObject GetVehiclePrefab(string vehicleName)
		{
			VehiclePrefab v = _vehiclePrefabs.Single(x => x.Name == vehicleName);
			// Load random prefab
			string prefabName = v.PrefabNames.PickRandom();
			GameObject g = _prefabCacheDictionary.ContainsKey(prefabName)
				? _prefabCacheDictionary[prefabName]
				: GetPrefab(prefabName);

			if (g.GetComponent<NavMeshAgent>() == null)
				g.AddComponent<NavMeshAgent>();
			// Set the correct speed
			g.GetComponent<NavMeshAgent>().speed = v.Speed;
			return g;
		}

		public GameObject GetPrefab(string prefabName, bool cache = true)
		{
			if (_prefabCacheDictionary.ContainsKey(prefabName) && cache)
				return _prefabCacheDictionary[prefabName];
			GameObject g = _assetBundle.LoadAsset<GameObject>(prefabName);
			if (g == null)
				throw new ArgumentException($"Cant find prefab {prefabName}");

			if (cache)
				_prefabCacheDictionary.Add(prefabName, g);
			return g;
		}

		public Material GetMaterial(string materialName)
		{
			if (_materialCacheDictionary.ContainsKey(materialName))
				return _materialCacheDictionary[materialName];
			Material mat = _assetBundle.LoadAsset<Material>(materialName);
			_materialCacheDictionary.Add(materialName, mat);
			return mat;
		}

		public enum PrefabType
		{
			RoadStraight,
			RoadTSection,
			RoadIntersection,
			RoadCorner,
			DestroyedGridTile,
			Grass,
			Tank,
			Plane,
			Bomb,
			ExplosionFx,
			StagingBuilding,
			DestroyFx
		}

		public GameObject GetPredefinedPrefab(PrefabType prefabType)
		{
			switch (prefabType)
			{
				case PrefabType.RoadStraight:
					return GetPrefab(_assetBundleSettingsSettings.Roads.RoadStraight);
				case PrefabType.RoadTSection:
					return GetPrefab(_assetBundleSettingsSettings.Roads.RoadTSection);
				case PrefabType.RoadIntersection:
					return GetPrefab(_assetBundleSettingsSettings.Roads.RoadIntersection);
				case PrefabType.RoadCorner:
					return GetPrefab(_assetBundleSettingsSettings.Roads.RoadCorner);
				case PrefabType.Grass:
					return GetPrefab(_assetBundleSettingsSettings.Grass);
				case PrefabType.StagingBuilding:
					return GetPrefab(_assetBundleSettingsSettings.StagingBuildingPrefab);
				case PrefabType.Tank:
					// We do not need to cache these
					return _assetBundle.LoadAsset<GameObject>(_assetBundleSettingsSettings.Chaos.TankPrefab);
				case PrefabType.Plane:
					return _assetBundle.LoadAsset<GameObject>(_assetBundleSettingsSettings.Chaos.PlanePrefab);
				case PrefabType.Bomb:
					return GetPrefab(_assetBundleSettingsSettings.Chaos.BombPrefab);
				case PrefabType.ExplosionFx:
					return GetPrefab(_assetBundleSettingsSettings.Chaos.ExplosionPrefab);
				case PrefabType.DestroyFx:
					return GetPrefab(_assetBundleSettingsSettings.DestroyedTiles.Fx);
				case PrefabType.DestroyedGridTile:
					return GetPrefab(_assetBundleSettingsSettings.DestroyedTiles.RandomTiles.PickRandom());
				default:
					throw new ArgumentOutOfRangeException(nameof(prefabType), prefabType, null);
			}
		}


		/// <summary>
		/// Function to get a vehicle name based on the size.
		/// </summary>
		/// <param name="size"></param>
		/// <returns></returns>
		public string GetVehicleName(int size)
		{
			foreach (VehiclePrefab vehiclePrefab in _vehiclePrefabs)
			{
				if (size >= vehiclePrefab.MinSize)
				{
					return vehiclePrefab.Name;
				}
			}

			throw new ArgumentOutOfRangeException(nameof(size), size, "Vehicle can not be found");
		}

		/// <summary>
		/// Function where all the prefab's are defined.
		/// </summary>
		/// <param name="vehicleName"></param>
		/// <returns></returns>
		public Sprite GetVehicleSpritesByType(string vehicleName)
		{
			// Load from cache
			if (_spriteCacheDictionary.ContainsKey(vehicleName))
				return _spriteCacheDictionary[vehicleName];
			throw new ArgumentException($"Missing vehicle sprite! For vehicle: {vehicleName}");
		}

		/// <summary>
		/// Function to generate sprites for every vehicle in the game.
		/// </summary>
		private void GenerateVehicleSprites()
		{
			foreach (VehiclePrefab vehiclePrefab in _assetBundleSettingsSettings.Vehicles)
			{
				// Generate a texture of the prefab
				Texture2D texture =
					RuntimePreviewGenerator.GenerateModelPreview(GetPrefab(vehiclePrefab.PrefabNames.First())
						.transform);
				// Add the sprite to the caching dictionary
				_spriteCacheDictionary.Add(vehiclePrefab.Name, texture.GenerateSprite());
			}
		}

		void OnDestroy()
		{
			_assetBundle.Unload(true);
		}
	}
}
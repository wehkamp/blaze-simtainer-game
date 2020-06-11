using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Assets.Scripts.Models.Settings;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Assets.Editor
{
	/// <summary>
	/// This class is used to automatically apply all the properties and components a prefab needs to be working in this game.
	/// It reads the config.json and every prefab in there is being handled. 
	/// </summary>
	internal class AssetsPropertiesApplier
	{
		[MenuItem("Assets/ Fix Asset properties and components")]
		internal static void ApplyAssetPropertiesAndComponents()
		{
			// First read the config.json
			StreamReader reader = new StreamReader("config.json");
			SettingsModel settings = JsonUtility.FromJson<SettingsModel>(reader.ReadToEnd());

			// Vehicles
			foreach (VehiclePrefab vehiclePrefab in settings.AssetBundle.Vehicles)
			{
				foreach (string prefabName in vehiclePrefab.PrefabNames)
				{
					ApplyPropertiesToPrefab(prefabName, settings.AssetBundle.Name, "Vehicle",
						ComponentType.NavMeshAgent, true);
				}
			}

			// Buildings
			foreach (BuildingPrefab buildingPrefab in settings.AssetBundle.Buildings)
			{
				foreach (Prefab prefab in buildingPrefab.Prefabs)
				{
					ApplyPropertiesToPrefab(prefab.Name, settings.AssetBundle.Name, "Building",
						ComponentType.NotWalkable, true);
				}

				foreach (Prefab prefab in buildingPrefab.DecayedPrefabs)
				{
					ApplyPropertiesToPrefab(prefab.Name, settings.AssetBundle.Name, "Building",
						ComponentType.NotWalkable, true);
				}
			}

			// Chaos Elements
			ApplyPropertiesToPrefab(settings.AssetBundle.Chaos.PlanePrefab, settings.AssetBundle.Name, "Plane");
			ApplyPropertiesToPrefab(settings.AssetBundle.Chaos.TankPrefab, settings.AssetBundle.Name, "Tank",
				ComponentType.NavMeshAgent);
			ApplyPropertiesToPrefab(settings.AssetBundle.Chaos.ExplosionPrefab, settings.AssetBundle.Name, "Fx");
			ApplyPropertiesToPrefab(settings.AssetBundle.Chaos.BombPrefab, settings.AssetBundle.Name, "Fx");

			// Roads
			ApplyPropertiesToPrefab(settings.AssetBundle.Roads.RoadIntersection, settings.AssetBundle.Name, "Road");
			ApplyPropertiesToPrefab(settings.AssetBundle.Roads.RoadTSection, settings.AssetBundle.Name, "Road");
			ApplyPropertiesToPrefab(settings.AssetBundle.Roads.RoadStraight, settings.AssetBundle.Name, "Road");
			ApplyPropertiesToPrefab(settings.AssetBundle.Roads.RoadCorner, settings.AssetBundle.Name, "Road");

			// Staging building
			ApplyPropertiesToPrefab(settings.AssetBundle.StagingBuildingPrefab, settings.AssetBundle.Name, "Building",
				ComponentType.NotWalkable, true);

			// Grass tile
			ApplyPropertiesToPrefab(settings.AssetBundle.Grass, settings.AssetBundle.Name, "Grass",
				ComponentType.NotWalkable, true);

			// Random tiles that show a destroyed building
			foreach (string tile in settings.AssetBundle.DestroyedTiles.RandomTiles)
			{
				ApplyPropertiesToPrefab(tile, settings.AssetBundle.Name, "DestroyedBuilding", ComponentType.NotWalkable,
					true);
			}

			// Layer FX
			foreach (LayerEffect layerEffect in settings.AssetBundle.LayerEffects)
			{
				ApplyPropertiesToPrefab(layerEffect.PrefabName, settings.AssetBundle.Name, "Fx");
			}

			// Destroyed Tile FX
			ApplyPropertiesToPrefab(settings.AssetBundle.DestroyedTiles.Fx, settings.AssetBundle.Name, "Fx");

			// Finally refresh the asset database
			AssetDatabase.Refresh();
		}

		/// <summary>
		/// Types of possible components that needs 
		/// </summary>
		private enum ComponentType
		{
			NavMeshAgent,
			NotWalkable,
			Camera,
			None
		}

		/// <summary>
		/// Complex function to fix imported prefabs to work with the project
		/// </summary>
		/// <param name="prefabName"></param>
		/// <param name="assetBundle"></param>
		/// <param name="tag"></param>
		/// <param name="componentType"></param>
		/// <param name="generateTransparentMaterials"></param>
		private static void ApplyPropertiesToPrefab(string prefabName, string assetBundle, string tag,
			ComponentType componentType = ComponentType.None, bool generateTransparentMaterials = false)
		{
			string[] assetGuids = AssetDatabase.FindAssets(prefabName);

			foreach (string assetGuid in assetGuids)
			{
				string assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
				// Match exact name of prefab, because we could also get a model or a texture with the same name instead of the prefab.
				if (Path.GetFileName(assetPath) != prefabName + ".prefab") continue;
				Debug.Log($"Handling {prefabName}");

				// Load the target prefab
				GameObject targetGameObject =
					PrefabUtility.LoadPrefabContents(AssetDatabase.GUIDToAssetPath(assetGuid));

				// Check if it actually loads up
				if (targetGameObject == null)
				{
					Debug.LogError($"Can't find prefab {prefabName}");
					continue;
				}

				// Already done this one if tag is already been set
				if (targetGameObject.CompareTag(tag)) continue;

				// Mark the object as a dirty object so we can edit it
				EditorUtility.SetDirty(targetGameObject);
				PrefabUtility.RecordPrefabInstancePropertyModifications(targetGameObject);

				// Set the correct tag of the object
				targetGameObject.tag = tag;

				// Add a component if component type matches
				if (componentType != ComponentType.None)
				{
					AddComponent(componentType, targetGameObject);
				}

				// Get all renderers of the target object
				Renderer[] renderers = targetGameObject.GetComponents<Renderer>();
				Renderer[] childRenderers = targetGameObject.GetComponentsInChildren<Renderer>();

				// Fix the renders, set correct properties and create a transparent material if requested
				FixRenderers(renderers, assetBundle, generateTransparentMaterials);
				FixRenderers(childRenderers, assetBundle, generateTransparentMaterials);

				// Finally save the asset. Unload the scene, destroy the objects
				FinishObject(targetGameObject, assetBundle, assetPath);
			}
		}

		private static void FinishObject(GameObject targetGameObject, string assetBundle, string assetPath)
		{
			// Save the object as a prefab and overwrite the old prefab
			PrefabUtility.SaveAsPrefabAsset(targetGameObject, assetPath);

			// Set the correct asset bundle for the object
			SetAssetBundles(assetBundle, assetPath);

			// Unload the prefab from the memory
			PrefabUtility.UnloadPrefabContents(targetGameObject);

			// Unload scene for optimization. Only 64 scenes can be actively loaded in Unity
			AsyncOperation unloading = SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene(),
				UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);

			int ticks = 0;
			while (unloading != null && !unloading.isDone && ticks < 30)
			{
				Thread.Sleep(100);
				ticks++;
			}
		}

		/// <summary>
		/// Function to apply all material properties of the given renderers.
		/// </summary>
		/// <param name="renderers"></param>
		/// <param name="assetBundle"></param>
		/// <param name="generateTransparentMaterials"></param>
		private static void FixRenderers(IEnumerable<Renderer> renderers, string assetBundle,
			bool generateTransparentMaterials)
		{
			foreach (Renderer renderer in renderers)
			{
				if (renderer.sharedMaterial == null) continue;

				// Apply all material properties
				ApplyMaterialProperties(assetBundle, renderer.sharedMaterial);

				// Generate a transparent material if required
				if (generateTransparentMaterials)
				{
					CreateTransparentMaterial(assetBundle, renderer.sharedMaterial);
				}
			}
		}

		/// <summary>
		/// Function to apply default properties to a material.
		/// </summary>
		/// <param name="assetBundleName"></param>
		/// <param name="material"></param>
		private static void ApplyMaterialProperties(string assetBundleName, Material material)
		{
			if (material.enableInstancing) return;
			EditorUtility.SetDirty(material);
			material.enableInstancing = true;
			SetAssetBundles(assetBundleName, AssetDatabase.GetAssetPath(material.GetInstanceID()));
		}

		/// <summary>
		/// Function to create a transparent material based on an existing material
		/// </summary>
		/// <param name="assetBundleName"></param>
		/// <param name="originalMaterial"></param>
		private static void CreateTransparentMaterial(string assetBundleName, Material originalMaterial)
		{
			// Get the original path of the material
			string path = AssetDatabase.GetAssetPath(originalMaterial);

			string newFileName = path.Split('/').Last().Insert(0, "Transparent-");

			string newPath = Path.Combine(path.Substring(0, path.LastIndexOf('/')), newFileName);

			// Check if material already exist
			if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), newPath)))
			{
				return;
			}

			Material newMaterial = new Material(Shader.Find(originalMaterial.shader.name));

			// Copy the properties from the original material
			newMaterial.CopyPropertiesFromMaterial(originalMaterial);

			// Set new properties of material to enable transparency
			newMaterial.SetInt("_SrcBlend", (int) UnityEngine.Rendering.BlendMode.SrcAlpha);
			newMaterial.SetInt("_DstBlend", (int) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
			newMaterial.SetInt("_ZWrite", 0);
			newMaterial.DisableKeyword("_ALPHATEST_ON");
			newMaterial.EnableKeyword("_ALPHABLEND_ON");
			newMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
			newMaterial.color = new Color(newMaterial.color.r, newMaterial.color.g, newMaterial.color.b, 0.3f);
			newMaterial.renderQueue = 3000;
			newMaterial.enableInstancing = true;

			// Finally create the material as an asset
			AssetDatabase.CreateAsset(newMaterial, newPath);

			// And set the correct asset bundle of the material
			SetAssetBundles(assetBundleName, newPath);
		}

		/// <summary>
		/// Set all asset bundles to the correct one
		/// </summary>
		/// <param name="assetBundleName"></param>
		/// <param name="assetPath"></param>
		private static void SetAssetBundles(string assetBundleName, string assetPath)
		{
			if (!string.IsNullOrEmpty(assetPath) && !string.IsNullOrEmpty(assetBundleName))
				AssetImporter.GetAtPath(assetPath).SetAssetBundleNameAndVariant(assetBundleName, "");
		}

		/// <summary>
		/// Add a component to a game object based on the component type
		/// </summary>
		/// <param name="componentType"></param>
		/// <param name="targetGameObject"></param>
		private static void AddComponent(ComponentType componentType, GameObject targetGameObject)
		{
			switch (componentType)
			{
				case ComponentType.NavMeshAgent:
					if (targetGameObject.GetComponent<NavMeshAgent>() == null)
					{
						// Add a navmashagent which allows an object to move around as a AI
						NavMeshAgent n = targetGameObject.AddComponent<NavMeshAgent>();
						n.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
						n.avoidancePriority = 0;
						n.baseOffset = 0.1f;
						n.radius = 1.0f;
					}

					break;
				case ComponentType.NotWalkable:
					if (targetGameObject.GetComponent<NavMeshModifier>() == null)
					{
						// Add a navmash modifier so the navmesh will know that this object is not walkable
						NavMeshModifier navMeshModifier = targetGameObject.AddComponent<NavMeshModifier>();
						navMeshModifier.overrideArea = true;
						navMeshModifier.area = NavMesh.GetAreaFromName("Not Walkable");
					}

					break;
				case ComponentType.Camera:
					// These camera settings are based on the paid assets pack
					if (targetGameObject.GetComponent<Camera>() == null)
					{
						GameObject cameraObject = new GameObject();

						Camera camera = cameraObject.AddComponent<Camera>();
						camera.clearFlags = CameraClearFlags.Color;
						// Set default background color
						camera.backgroundColor = new Color32(88, 85, 74, 255);
						camera.orthographic = false;

						Vector3 defaultPositionForPaidAssetPack = Vector3.zero;
						Quaternion defaultRotationForPaidAssetPack = Quaternion.Euler(0, 0, 0);
						float defaultFieldOfViewForPaidAssetPack = 0f;
						switch (targetGameObject.tag)
						{
							case "Plane":
								cameraObject.tag = "PlaneCamera";

								defaultPositionForPaidAssetPack = new Vector3(-0.88f, 25.1f, -4.7f);
								defaultRotationForPaidAssetPack = Quaternion.Euler(70, 0, 0);
								defaultFieldOfViewForPaidAssetPack = 47.1f;
								break;
							case "Tank":
								cameraObject.tag = "TankCamera";

								defaultPositionForPaidAssetPack = new Vector3(-1.08f, 1f, 0.23f);
								defaultFieldOfViewForPaidAssetPack = 60f;
								break;
						}

						cameraObject.transform.position = defaultPositionForPaidAssetPack;
						cameraObject.transform.rotation = defaultRotationForPaidAssetPack;
						camera.fieldOfView = defaultFieldOfViewForPaidAssetPack;
						cameraObject.transform.SetParent(targetGameObject.transform);
					}

					break;
			}
		}
	}
}
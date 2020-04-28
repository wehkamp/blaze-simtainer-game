using System;
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
			StreamReader reader = new StreamReader("config.json");
			SettingsModel settings = JsonUtility.FromJson<SettingsModel>(reader.ReadToEnd());

			foreach (VehiclePrefab vehiclePrefab in settings.AssetBundle.Vehicles)
			{
				foreach (string prefabName in vehiclePrefab.PrefabNames)
				{
					ApplyPropertiesToPrefab(prefabName, settings.AssetBundle.Name, "Vehicle",
						ComponentType.NavMeshAgent, true);
				}
			}

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

			ApplyPropertiesToPrefab(settings.AssetBundle.Chaos.PlanePrefab, settings.AssetBundle.Name, "Plane");
			ApplyPropertiesToPrefab(settings.AssetBundle.Chaos.TankPrefab, settings.AssetBundle.Name, "Tank",
				ComponentType.NavMeshAgent);
			ApplyPropertiesToPrefab(settings.AssetBundle.Chaos.ExplosionPrefab, settings.AssetBundle.Name, "Fx");
			ApplyPropertiesToPrefab(settings.AssetBundle.Chaos.BombPrefab, settings.AssetBundle.Name, "Fx");
			ApplyPropertiesToPrefab(settings.AssetBundle.Roads.RoadIntersection, settings.AssetBundle.Name, "Road");
			ApplyPropertiesToPrefab(settings.AssetBundle.Roads.RoadTSection, settings.AssetBundle.Name, "Road");
			ApplyPropertiesToPrefab(settings.AssetBundle.Roads.RoadStraight, settings.AssetBundle.Name, "Road");
			ApplyPropertiesToPrefab(settings.AssetBundle.Roads.RoadCorner, settings.AssetBundle.Name, "Road");
			ApplyPropertiesToPrefab(settings.AssetBundle.StagingBuildingPrefab, settings.AssetBundle.Name, "Building",
				ComponentType.NotWalkable, true);
			ApplyPropertiesToPrefab(settings.AssetBundle.Grass, settings.AssetBundle.Name, "Grass",
				ComponentType.NotWalkable, true);

			foreach (string tile in settings.AssetBundle.DestroyedTiles.RandomTiles)
			{
				ApplyPropertiesToPrefab(tile, settings.AssetBundle.Name, "DestroyedBuilding", ComponentType.NotWalkable,
					true);
			}

			foreach (LayerEffect layerEffect in settings.AssetBundle.LayerEffects)
			{
				ApplyPropertiesToPrefab(layerEffect.PrefabName, settings.AssetBundle.Name, "Fx");
			}

			ApplyPropertiesToPrefab(settings.AssetBundle.DestroyedTiles.Fx, settings.AssetBundle.Name, "Fx");
			ApplyPropertiesToPrefab(settings.AssetBundle.DestroyedTiles.Fx, settings.AssetBundle.Name, "Fx");

			AssetDatabase.Refresh();
		}

		/// <summary>
		/// Types of possible components that needs 
		/// </summary>
		private enum ComponentType
		{
			NavMeshAgent,
			NotWalkable,
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
			string[] strings = AssetDatabase.FindAssets(prefabName);

			for (int i = 0; i < strings.Length; i++)
			{
				string assetPath = AssetDatabase.GUIDToAssetPath(strings[i]);
				// Match exact name of prefab
				if (Path.GetFileName(assetPath) != prefabName + ".prefab") continue;
				Debug.Log($"Handling {prefabName}");
				GameObject targetGameObject =
					PrefabUtility.LoadPrefabContents(AssetDatabase.GUIDToAssetPath(strings[i]));

				if (targetGameObject == null)
				{
					Debug.LogError($"Can't find prefab {prefabName}");
					continue;
				}

				// Already done this one
				if (targetGameObject.CompareTag(tag)) continue;

				EditorUtility.SetDirty(targetGameObject);
				PrefabUtility.RecordPrefabInstancePropertyModifications(targetGameObject);
				switch (componentType)
				{
					case ComponentType.NavMeshAgent:
						if (targetGameObject.GetComponent<NavMeshAgent>() == null)
						{
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
							NavMeshModifier navMeshModifier = targetGameObject.AddComponent<NavMeshModifier>();
							navMeshModifier.overrideArea = true;
							navMeshModifier.area = NavMesh.GetAreaFromName("Not Walkable");
						}

						break;
				}

				targetGameObject.tag = tag;


				Renderer[] renders = targetGameObject.GetComponents<Renderer>();
				foreach (Renderer t in renders)
				{
					if (t.sharedMaterial == null) continue;
					ApplyMaterialProperties(assetBundle, t.sharedMaterial);

					if (generateTransparentMaterials)
					{
						CreateTransparentMaterial(assetBundle, t.sharedMaterial);
					}
				}

				Renderer[] childRenderers = targetGameObject.GetComponentsInChildren<Renderer>();
				foreach (Renderer t in childRenderers)
				{
					if (t.sharedMaterial == null) continue;
					ApplyMaterialProperties(assetBundle, t.sharedMaterial);

					if (generateTransparentMaterials)
					{
						CreateTransparentMaterial(assetBundle, t.sharedMaterial);
					}
				}

				// Finally save the asset. Unload the scene, destroy the objects
				PrefabUtility.SaveAsPrefabAsset(targetGameObject, assetPath);
				SetAssetBundles(assetBundle, assetPath);
				PrefabUtility.UnloadPrefabContents(targetGameObject);
				AsyncOperation unloading = SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene(),
					UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);

				int ticks = 0;
				while (unloading != null && !unloading.isDone && ticks < 30)
				{
					Thread.Sleep(100);
					ticks++;
				}
			}
		}

		private static void ApplyMaterialProperties(string assetBundleName, Material m)
		{
			if (m.enableInstancing) return;
			EditorUtility.SetDirty(m);
			m.enableInstancing = true;
			SetAssetBundles(assetBundleName, AssetDatabase.GetAssetPath(m.GetInstanceID()));
		}

		/// <summary>
		/// Function to create a transparent material based on an existing material
		/// </summary>
		/// <param name="assetBundleName"></param>
		/// <param name="m"></param>
		private static void CreateTransparentMaterial(string assetBundleName, Material m)
		{
			string path = AssetDatabase.GetAssetPath(m);

			string newFileName = path.Split('/').Last().Insert(0, "Transparent-");

			string newPath = Path.Combine(path.Substring(0, path.LastIndexOf('/')), newFileName);

			// Check if material already exist
			if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), newPath)))
			{
				return;
			}

			Material newMaterial = new Material(Shader.Find(m.shader.name));

			newMaterial.CopyPropertiesFromMaterial(m);

			newMaterial.SetInt("_SrcBlend", (int) UnityEngine.Rendering.BlendMode.SrcAlpha);
			newMaterial.SetInt("_DstBlend", (int) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
			newMaterial.SetInt("_ZWrite", 0);
			newMaterial.DisableKeyword("_ALPHATEST_ON");
			newMaterial.EnableKeyword("_ALPHABLEND_ON");
			newMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
			newMaterial.color = new Color(newMaterial.color.r, newMaterial.color.g, newMaterial.color.b, 0.3f);
			newMaterial.renderQueue = 3000;
			newMaterial.enableInstancing = true;
			AssetDatabase.CreateAsset(newMaterial, newPath);
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
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Components;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Models;
using Assets.Scripts.Models.VisualizedObjects;
using Assets.Scripts.Utils;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace Assets.Scripts.Managers
{
	/// <summary>
	/// This class will handle the whole grid management. It is allowed to access Grid tiles for other classes to do some checks.
	/// TODO: This class should be a little more optimized, there is too much real-time calculation.
	/// </summary>
	internal class GridManager : Singleton<GridManager>
	{
		// Rows are dynamic and changed in the generate city function to match a little with the amount of instances
		public int Rows { get; private set; } = 50;

		// Cols is being set by the SettingsManager
		public int Cols { get; private set; } = 50;

		public const int TileSize = 10;

		// The actual grid which may be used through different managers
		public readonly Dictionary<Tile, TileObject> Grid = new Dictionary<Tile, TileObject>();

		private NavMeshSurface _navMeshSurface;

		public Dictionary<Vector3, Quaternion> VehicleSpawnPoints { get; } = new Dictionary<Vector3, Quaternion>();

		public UnityEvent GridInitializedEvent;

		// Tile struct for the grid we're creating
		public struct Tile
		{
			public int X { get; }
			public int Z { get; }

			public Tile(int x, int z)
			{
				X = x;
				Z = z;
			}
		}

		public class TileObject
		{
			public GameObject GameObject { get; set; }
			public ObjectType ObjectType { get; set; }
		}

		void Start()
		{
			_navMeshSurface = FindObjectOfType<NavMeshSurface>();
		}

		#region City Generation

		public void GenerateCity(GameModel gameModel)
		{
			int instances = gameModel.Neighbourhoods.Select(
				x => x.VisualizedObjects.Count
			).Sum();

			Cols = SettingsManager.Instance.Settings.Grid.TilesPerStreet;

			Rows = (instances + gameModel.Neighbourhoods.Count) / Cols * 3;
			// We always want an uneven number of rows so we can generate a street everywhere
			if (Rows % 2 == 0)
				Rows++;

			GenerateGrid();
			VehicleSpawnPoints.Add(new Vector3(0f, 0.2f, 0f), Quaternion.Euler(0, 0f, 0));
			VehicleSpawnPoints.Add(new Vector3(Cols * TileSize - TileSize, 0.2f, 0f), Quaternion.Euler(0, 0f, 0));
			VehicleSpawnPoints.Add(new Vector3(Cols * TileSize - TileSize, 0.2f, Rows * TileSize - TileSize),
				Quaternion.Euler(0, 180f, 0));
			VehicleSpawnPoints.Add(new Vector3(0f, 0.2f, Rows * TileSize - TileSize), Quaternion.Euler(0, 180f, 0));
			foreach (NeighbourhoodModel neighbourhoodModel in gameModel.Neighbourhoods)
			{
				SpawnNeighbourhood(neighbourhoodModel);
			}

			_navMeshSurface.BuildNavMesh();
			GridInitializedEvent?.Invoke();
		}

		private void GenerateGrid()
		{
			for (int row = 0; row < Rows; row++)
			{
				for (int col = 0; col < Cols; col++)
				{
					int posX = col * TileSize;
					int posZ = row * TileSize;
					Grid.Add(new Tile(posX, posZ), null);
				}
			}

			SpawnMainRoads();

			for (int i = 0; i < Rows - 2; i++)
			{
				if (i % 2 == 0)
					SpawnRandomRoad(i * TileSize);
			}
		}

		#endregion

		#region Roads

		private void SpawnRandomRoad(int startZ)
		{
			IEnumerable<KeyValuePair<Tile, TileObject>> roadsToReplace = Grid.Where(x =>
				x.Key.Z == startZ && (x.Key.X == 0 || x.Key.X == Cols * TileSize - TileSize));

			foreach (KeyValuePair<Tile, TileObject> tile in roadsToReplace.ToList())
			{
				// Check if the road is not a corner
				if (tile.Value.GameObject != null && tile.Value.GameObject.name.ToLower().Contains("corner")) continue;
				if (tile.Value.GameObject != null)
				{
					Destroy(tile.Value.GameObject);
				}

				float direction = 90f;
				if (tile.Key.X > 0)
				{
					direction = -90f;
				}

				GameObject newRoad = Instantiate(
					AssetsManager.Instance.GetPredefinedPrefab(AssetsManager.PrefabType.RoadTSection),
					new Vector3(tile.Key.X, 0.5f, tile.Key.Z), Quaternion.Euler(0, direction, 0));
				Grid[tile.Key] = new TileObject {GameObject = newRoad, ObjectType = ObjectType.Road};
			}


			foreach (Tile tile in Grid.Where(t => t.Key.Z == startZ && t.Value == null).Select(t => t.Key)
				.ToList())
			{
				GameObject road = Instantiate(
					AssetsManager.Instance.GetPredefinedPrefab(AssetsManager.PrefabType.RoadStraight),
					new Vector3(tile.X, 0.5f, tile.Z), Quaternion.Euler(0, 0f, 0));
				Grid[tile] = new TileObject {GameObject = road, ObjectType = ObjectType.Road};
			}
		}

		/// <summary>
		/// Function to spawn the main roads of the map.
		/// </summary>
		private void SpawnMainRoads()
		{
			// Loop through all empty tiles on the borders of the map
			// X = 0 | X = 500 for example. 0,0 0,10 0,20... 490,0 490,10 490,20...
			foreach (KeyValuePair<Tile, TileObject> tile in Grid
				.Where(tile => tile.Key.X == 0 || (tile.Key.X == Cols * TileSize - TileSize) && tile.Value == null)
				.ToList())
			{
				AssetsManager.PrefabType prefabType = AssetsManager.PrefabType.RoadStraight;
				Quaternion rotation = Quaternion.Euler(0, 90f, 0);

				// Calculate the rotation of the corner by checking the position in the map
				if (tile.Key.X == Cols * TileSize - TileSize && tile.Key.Z == Rows * TileSize - TileSize)
				{
					rotation = Quaternion.Euler(0, 270f, 0);
					prefabType = AssetsManager.PrefabType.RoadCorner;
				}
				else if (tile.Key.Z == Rows * TileSize - TileSize && tile.Key.X == 0)
				{
					rotation = Quaternion.Euler(0, 180f, 0);
					prefabType = AssetsManager.PrefabType.RoadCorner;
				}
				else if (tile.Key.X == Cols * TileSize - TileSize && tile.Key.Z == 0)
				{
					rotation = Quaternion.Euler(0, 0f, 0);
					prefabType = AssetsManager.PrefabType.RoadCorner;
				}
				else if (tile.Key.Z == 0 && tile.Key.X == 0)
				{
					rotation = Quaternion.Euler(0, 90f, 0);
					prefabType = AssetsManager.PrefabType.RoadCorner;
				}
				// Spawn the road
				GameObject gameObj = Instantiate(AssetsManager.Instance.GetPredefinedPrefab(prefabType),
					new Vector3(tile.Key.X, 0.5f, tile.Key.Z),
					rotation);

				// Override the tile
				Grid[tile.Key] = new TileObject {ObjectType = ObjectType.Road, GameObject = gameObj};
			}

			// Create the last road on the map
			foreach (KeyValuePair<Tile, TileObject> tile in Grid
				.Where(tile => (tile.Key.Z == 0 || tile.Key.Z == Rows * TileSize - TileSize) && tile.Value == null)
				.ToList())
			{
				GameObject gameObj = Instantiate(
					AssetsManager.Instance.GetPredefinedPrefab(AssetsManager.PrefabType.RoadStraight),
					new Vector3(tile.Key.X, 0.5f, tile.Key.Z),
					Quaternion.Euler(0, 0f, 0));
				Grid[tile.Key] = new TileObject {ObjectType = ObjectType.Road, GameObject = gameObj};
			}
		}

		#endregion

		#region Neighbourhoods spawning

		/// <summary>
		/// Function to build a neighbourhood
		/// </summary>
		/// <param name="neighbourhoodModel"></param>
		public void SpawnNeighbourhood(NeighbourhoodModel neighbourhoodModel)
		{
			// If there are no visualized objects, destroy the block
			if (neighbourhoodModel.VisualizedObjects.Count < 1)
			{
				DestroyBlock(neighbourhoodModel, false);
				return;
			}

			// Make a list of buildings
			List<IVisualizedBuilding> visualizedObjects =
				neighbourhoodModel.VisualizedObjects.OfType<IVisualizedBuilding>().ToList();

			// Calculate total tile count we need to spawn this building (if building is wider than the TileSize)
			int tileCount = GetTotalTilesRequired(visualizedObjects, neighbourhoodModel.Age) + 1;

			// Find the amount of tiles we need to spawn this neighbourhood
			List<Tile> tiles = FindTiles(tileCount);

			if (tiles.Count != tileCount)
			{
				// Can't spawn the neighbourhood. Not enough tiles per street.
				Debug.LogError(
					"Building block went wrong. Not enough tiles!\r\nIncrease the TilesPerStreet in the config.json");
				return;
			}

			// Set spawn offset and tile index to 0
			float spawnOffsetX = 0;
			int tileIndex = 0;
			// Loop through all the visualized objects
			for (int index = 0; index < visualizedObjects.Count; index++)
			{
				// Get the correct tile
				Tile tile = tiles[tileIndex];
				// If the tile is already filled, destroy it.
				if (Grid[tile] != null && Grid[tile].GameObject != null)
					Destroy(Grid[tile].GameObject);

				// Spawn a building at this street
				GameObject building = SpawnBuilding(tile, visualizedObjects[index].Size, neighbourhoodModel.Age,
					neighbourhoodModel, visualizedObjects[index] is VisualizedStagingBuildingModel);
				// Set the correct tile information
				Grid[tile] = new TileObject
					{GameObject = building, ObjectType = ObjectType.Building};
				tileIndex++;

				// Check how many tiles we need to spawn this building
				int tilesRequired = GetTilesRequiredForBuilding(building);

				// Check if we need more than 1 tile for this building
				if (tilesRequired > 1)
				{
					if (Physics.Raycast(building.transform.position, Vector3.left, out RaycastHit hit, 5))
					{
						// Check if there is grass or a road next to us and at the distance so we can align our building
						if (hit.transform.gameObject.CompareTag("Grass") || hit.transform.gameObject.CompareTag("Road"))
							spawnOffsetX += hit.distance;
					}
					// We might need to move the building to align perfectly like calculated in the raycast.
					building.transform.position = new Vector3(building.transform.position.x + spawnOffsetX,
						building.transform.position.y, building.transform.position.z);
					for (int i = 1; i < tilesRequired; i++)
					{
						// If we need more than 1 tile fill the next tiles as well
						// This will only happen if the building is wider than the TileSize
						Tile tile2 = tiles[tileIndex];
						// Destroy existing object if it's not the building we are trying to spawn
						if (Grid[tile2] != null && !Grid[tile2].GameObject.Equals(building))
							Destroy(Grid[tile2].GameObject);

						// Set the correct tile information
						Grid[tile2] = new TileObject
							{GameObject = building, ObjectType = ObjectType.Building};
						tileIndex++;
					}
				}
				// Set the correct game object in the visualized object of the neighbourhood so we can use it as reference
				neighbourhoodModel
					.VisualizedObjects[neighbourhoodModel.VisualizedObjects.IndexOf(visualizedObjects[index])]
					.GameObject = building;
			}

			// Spawn a grass patch as a divider between buildings
			GameObject grassPatch = Instantiate(
				AssetsManager.Instance.GetPredefinedPrefab(AssetsManager.PrefabType.Grass),
				new Vector3(tiles.Last().X, 0.5f, tiles.Last().Z), Quaternion.Euler(0, 90f, 0));
			// Add the grass patch to the list of visualized objects
			neighbourhoodModel.VisualizedObjects.Add(new VisualizedGrassTileModel {GameObject = grassPatch});

			Grid[tiles.Last()] = new TileObject
				{GameObject = grassPatch, ObjectType = ObjectType.Grass};
		}

		/// <summary>
		/// Function to spawn a new building
		/// </summary>
		/// <param name="tile"></param>
		/// <param name="size"></param>
		/// <param name="age"></param>
		/// <param name="neighbourhood"></param>
		/// <param name="staging"></param>
		/// <returns></returns>
		public GameObject SpawnBuilding(Tile tile, int size, int age, NeighbourhoodModel neighbourhood, bool staging)
		{
			// Default rotation
			Quaternion buildingRotation = Quaternion.Euler(0, 90f, 0);
			GameObject building;
			if (staging)
			{
				building = AssetsManager.Instance.GetPredefinedPrefab(AssetsManager.PrefabType.StagingBuilding);
			}
			else
			{
				// Extract the tuple with the building prefab and the rotation
				(GameObject buildingObject, float rotation) = AssetsManager.Instance.GetBuildingPrefab(size, age);
				buildingRotation = Quaternion.Euler(0, rotation, 0);
				building = buildingObject;
			}

			// Spawn the prefab with the correct location and rotation
			building = Instantiate(building, new Vector3(tile.X, 0.5f, tile.Z), buildingRotation);

			// Set the name of the object so we can use it later as a reference
			building.name = $"neighbourhood-{neighbourhood.Name}";
			return building;
		}


		/// <summary>
		/// Function to destroy a particular building.
		/// </summary>
		/// <param name="visualizedObject"></param>
		/// <param name="destroyEffect">Set this to true if you want the building to disappear with a nice effect</param>
		public void DestroyBuilding(IVisualizedBuilding visualizedObject, bool destroyEffect = true)
		{
			if (visualizedObject.GameObject == null) return;

			// Get the correct tile from the grid
			List<KeyValuePair<Tile, TileObject>> gridObjects = Grid.Where(t =>
				t.Value != null && t.Value.GameObject != null &&
				t.Value.GameObject.Equals(visualizedObject.GameObject)).ToList();

			foreach (KeyValuePair<Tile, TileObject> gridObject in gridObjects)
			{
				// Check if we want a destroy effect or not
				if (visualizedObject is VisualizedStagingBuildingModel || !destroyEffect)
					Destroy(visualizedObject.GameObject);
				else
					visualizedObject.GameObject.AddComponent<DestroyGridTile>().Tile = gridObject.Key;
				// Set the tile to null
				Grid[gridObject.Key] = null;
			}
		}

		/// <summary>
		/// Function to destroy a complete neighbourhood/block
		/// </summary>
		/// <param name="neighbourhood"></param>
		/// <param name="destroyEffect"></param>
		public void DestroyBlock(NeighbourhoodModel neighbourhood, bool destroyEffect)
		{
			foreach (IVisualizedObject visualizedObject in neighbourhood.VisualizedObjects
				.Where(x => !(x is VisualizedVehicleModel)).ToList())
			{
				if (visualizedObject.GameObject == null) continue;

				// Find all grid objects that equal to the visualized object
				List<KeyValuePair<Tile, TileObject>> gridObjects = Grid.Where(x =>
					x.Value != null && x.Value.GameObject != null &&
					x.Value.GameObject.Equals(visualizedObject.GameObject)).ToList();

				foreach (KeyValuePair<Tile, TileObject> gridObject in gridObjects)
				{
					if (destroyEffect)
						visualizedObject.GameObject.AddComponent<DestroyGridTile>().Tile = gridObject.Key;
					else
					{
						Destroy(visualizedObject.GameObject);
						visualizedObject.GameObject = null;
						Grid[gridObject.Key] = null;
					}
				}

				// Remove all grass tiles from a neighbourhood that is being destroyed
				if (visualizedObject is VisualizedGrassTileModel)
					neighbourhood.VisualizedObjects.Remove(visualizedObject);
			}


			Debug.Log($"[GridManager] Removed block of neighbourhood: {neighbourhood.Name}");
		}

		#endregion

		#region Tile Calculation

		/// <summary>
		/// Function to get free tiles that are behind each other, so you can create a block.
		/// </summary>
		/// <param name="tiles"></param>
		/// <returns></returns>
		private List<Tile> FindTiles(int tiles)
		{
			int currentZIndex = 0;
			List<Tile> foundTiles = new List<Tile>();
			while (currentZIndex <= Cols)
			{
				foreach (KeyValuePair<Tile, TileObject> tile in Grid.Where(tile =>
					tile.Key.Z == currentZIndex * TileSize))
				{
					// If found a tile, add it to the list else clear the list and re-continue searching for empty spots
					if (tile.Value == null || tile.Value.ObjectType == ObjectType.DestroyedBuilding)
						foundTiles.Add(tile.Key);
					else
						foundTiles.Clear();

					if (foundTiles.Count != tiles) continue;
					// We found enough tiles! Lets check if there are still objects (grass tiles) left and remove them
					RemoveTiles(foundTiles);
					return foundTiles;
				}

				// Nothing found, lets go to the next Z index
				currentZIndex++;
			}
#if UNITY_EDITOR
			Debug.LogError("Failed finding empty tiles");
#endif
			return foundTiles;
		}

		/// <summary>
		/// This function removes destroyed grass tiles from the game when calculating new tiles for a new building block
		/// </summary>
		/// <param name="tiles"></param>
		private void RemoveTiles(List<Tile> tiles)
		{
			foreach (Tile tile in tiles)
			{
				if (Grid[tile] != null && Grid[tile].GameObject != null)
				{
					DestroyImmediate(Grid[tile].GameObject);
					Grid[tile] = null;
				}
			}
		}

		/// <summary>
		/// Function to calculate how many tiles are needed for an amount of buildings.
		/// </summary>
		/// <param name="buildings"></param>
		/// <param name="age"></param>
		/// <returns></returns>
		public int GetTotalTilesRequired(IEnumerable<IVisualizedBuilding> buildings, int age)
		{
			int tiles = 0;
			foreach (IVisualizedBuilding building in buildings)
			{
				Tuple<GameObject, float> b = AssetsManager.Instance.GetBuildingPrefab(building.Size, age);
				tiles += GetTilesRequiredForBuilding(b.Item1);
			}

			return tiles;
		}

		/// <summary>
		/// Function to calculate the tiles that are required for placing a building.
		/// </summary>
		/// <param name="building"></param>
		/// <returns></returns>
		public int GetTilesRequiredForBuilding(GameObject building)
		{
			const int threshold = 1;
			int tiles = 1;

			Vector3 sizeBounds = building.GetComponent<MeshFilter>().sharedMesh.bounds.size;

			for (int i = 1; i < sizeBounds.x; i++)
			{
				if (i % (TileSize + threshold) == 0)
					tiles++;
			}

			return tiles;
		}

		#endregion
	}
}
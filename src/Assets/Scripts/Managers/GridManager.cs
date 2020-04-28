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

		// Cols is a solid 50
		public int Cols { get; private set; } = 50;

		public readonly int TileSize = 10;

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

			// Make rows based on amount of instances in total divided by 14 so we always have enough space.
			Rows = instances / 14;

			// We always want an uneven number of rows so we can generate a street everywhere
			if (Rows % 2 == 0)
				Rows++;

			Cols = SettingsManager.Instance.Settings.Grid.TilesPerStreet;
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
			foreach (KeyValuePair<Tile, TileObject> tile in Grid
				.Where(tile => tile.Key.X == 0 || (tile.Key.X == Cols * TileSize - TileSize) && tile.Value == null)
				.ToList())
			{
				AssetsManager.PrefabType prefabType = AssetsManager.PrefabType.RoadStraight;
				Quaternion rotation = Quaternion.Euler(0, 90f, 0);

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

				GameObject gameObj = Instantiate(AssetsManager.Instance.GetPredefinedPrefab(prefabType),
					new Vector3(tile.Key.X, 0.5f, tile.Key.Z),
					rotation);
				Grid[tile.Key] = new TileObject {ObjectType = ObjectType.Road, GameObject = gameObj};
			}

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
			if (neighbourhoodModel.VisualizedObjects.Count < 1)
			{
				DestroyBlock(neighbourhoodModel, false);
				return;
			}

			List<IVisualizedObject> visualizedObjects =
				neighbourhoodModel.VisualizedObjects
					.Where(x => x is VisualizedBuildingModel || x is VisualizedStagingBuildingModel).ToList();

			int tileCount = visualizedObjects.Count + 1;
			List<Tile> tiles = FindTiles(tileCount);
			if (tiles.Count != tileCount)
			{
				Debug.LogError("Building block went wrong. Not enough tiles!");
				return;
			}

			for (int index = 0; index < visualizedObjects.Count; index++)
			{
				Tile tile = tiles[index];
				if (Grid[tile] != null && Grid[tile].GameObject != null)
					Destroy(Grid[tile].GameObject);

				GameObject building = SpawnBuilding(tile, visualizedObjects[index].Size, neighbourhoodModel.Age,
					neighbourhoodModel, visualizedObjects[index] is VisualizedStagingBuildingModel);

				neighbourhoodModel
					.VisualizedObjects[neighbourhoodModel.VisualizedObjects.IndexOf(visualizedObjects[index])]
					.GameObject = building;
				Grid[tile] = new TileObject
					{GameObject = building, ObjectType = ObjectType.Building};
			}

			GameObject grassPatch = Instantiate(
				AssetsManager.Instance.GetPredefinedPrefab(AssetsManager.PrefabType.Grass),
				new Vector3(tiles.Last().X, 0.5f, tiles.Last().Z), Quaternion.Euler(0, 90f, 0));
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
			Quaternion buildingRotation = Quaternion.Euler(0, 90f, 0);
			GameObject building;
			if (staging)
			{
				building = AssetsManager.Instance.GetPredefinedPrefab(AssetsManager.PrefabType.StagingBuilding);
			}
			else
			{
				(GameObject buildingObject, float rotation) = AssetsManager.Instance.GetBuildingPrefab(size, age);
				buildingRotation = Quaternion.Euler(0, rotation, 0);
				building = buildingObject;
			}

			building = Instantiate(building, new Vector3(tile.X, 0.5f, tile.Z), buildingRotation);

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
			if (visualizedObject.GameObject == null || !(visualizedObject is IVisualizedBuilding)) return;

			KeyValuePair<Tile, TileObject> gridObj = Grid.SingleOrDefault(t =>
				t.Value != null && t.Value.GameObject != null &&
				t.Value.GameObject == visualizedObject.GameObject);
			if (visualizedObject is VisualizedStagingBuildingModel || !destroyEffect)
				Destroy(visualizedObject.GameObject);
			else
				visualizedObject.GameObject.AddComponent<DestroyGridTile>().Tile = gridObj.Key;

			Grid[gridObj.Key] = null;
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

				KeyValuePair<Tile, TileObject> grid = Grid.SingleOrDefault(x =>
					x.Value != null && x.Value.GameObject != null &&
					x.Value.GameObject == visualizedObject.GameObject);
				if (destroyEffect)
					visualizedObject.GameObject.AddComponent<DestroyGridTile>().Tile = grid.Key;
				else
				{
					Destroy(visualizedObject.GameObject);
					visualizedObject.GameObject = null;
					Grid[grid.Key] = null;
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
			int tries = 0;
			int currentZIndex = 0;
			List<Tile> foundTiles = new List<Tile>();
			while (currentZIndex <= Cols)
			{
				tries++;
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

			Debug.LogError("Failed tiles");

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
	}

	#endregion
}
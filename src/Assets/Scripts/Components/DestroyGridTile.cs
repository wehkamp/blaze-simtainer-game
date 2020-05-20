using System.Collections.Generic;
using Assets.Scripts.Managers;
using Assets.Scripts.Models;
using Assets.Scripts.Utils;
using UnityEngine;
using static Assets.Scripts.Managers.GridManager;

namespace Assets.Scripts.Components
{
	/// <summary>
	/// Component which destroys a grid tile and replaces it with a destroyed grass tile.
	/// You must set the tile property, otherwise the component will crash
	/// </summary>
	internal class DestroyGridTile : MonoBehaviour
	{
		private readonly List<GameObject> _destroyEffects = new List<GameObject>();

		private Vector3 _startPos;
		private Bounds _bounds;

		// This property MUST be set, otherwise the component will crash
		public Tile Tile;

		void Start()
		{
			// We need the boundaries to add some effects to the top of a building
			_bounds = BoundariesUtil.GetMaxBounds(gameObject);
		}

		// Start is called before the first frame update
		void OnEnable()
		{
			GameObject destroyFx =
				AssetsManager.Instance.GetPredefinedPrefab(AssetsManager.PrefabType.DestroyFx);

			_destroyEffects.Add(Instantiate(destroyFx,
				new Vector3(transform.position.x, _bounds.max.y, transform.position.z),
				transform.rotation));

			_destroyEffects.Add(Instantiate(destroyFx,
				new Vector3(transform.position.x + 1.5f, _bounds.max.y, transform.position.z),
				transform.rotation));
			_destroyEffects.Add(Instantiate(destroyFx,
				new Vector3(transform.position.x - 1.5f, _bounds.max.y, transform.position.z),
				transform.rotation));
			_startPos = transform.position;
		}

		void Update()
		{
			// Move the building in the ground
			float y = transform.position.y - 0.1f;
			float position = _bounds.max.y - Mathf.Abs(transform.position.y);

			// Check if the top of boundary is below the ground
			if (position <= 0)
			{
				// Create a destroyed grass tile
				GameObject destroyedGrassTile =
					Instantiate(
						AssetsManager.Instance.GetPredefinedPrefab(AssetsManager.PrefabType.DestroyedGridTile), _startPos,
						Quaternion.identity);
				GridManager.Instance.Grid[Tile] = new TileObject
					{GameObject = destroyedGrassTile, ObjectType = ObjectType.DestroyedBuilding};
				destroyedGrassTile.name = gameObject.name;
				Destroy(gameObject);
			}

			// Apply new position
			transform.position = new Vector3(transform.position.x, y, transform.position.z);
		}

		void OnDisable()
		{
			foreach (GameObject destroyEffect in _destroyEffects)
			{
				Destroy(destroyEffect);
			}
		}
	}
}
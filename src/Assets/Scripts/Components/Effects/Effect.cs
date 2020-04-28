using System.Collections.Generic;
using Assets.Scripts.Managers;
using Assets.Scripts.Models;
using Assets.Scripts.Utils;
using UnityEngine;

namespace Assets.Scripts.Components.Effects
{
	/// <summary>
	/// Component that adds fire effects on the top of a GameObject by using the collision boundaries.
	/// </summary>
	public class Effect : MonoBehaviour
	{
		private readonly List<GameObject> _effects = new List<GameObject>();


		public void SetFx(GameObject fxObject)
		{
			Bounds bounds = BoundariesUtil.GetMaxBounds(gameObject);

			_effects.Add(Instantiate(fxObject,
				new Vector3(transform.position.x, bounds.max.y, transform.position.z),
				transform.rotation));

			_effects.Add(Instantiate(fxObject,
				new Vector3(transform.position.x + 1.5f, bounds.max.y, transform.position.z),
				transform.rotation));
			_effects.Add(Instantiate(fxObject,
				new Vector3(transform.position.x - 1.5f, bounds.max.y, transform.position.z),
				transform.rotation));
		}

		void OnDestroy()
		{
			foreach (GameObject effect in _effects)
			{
				Destroy(effect);
			}
		}
	}
}
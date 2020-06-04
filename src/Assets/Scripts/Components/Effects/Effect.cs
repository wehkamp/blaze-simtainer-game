using System.Collections.Generic;
using Assets.Scripts.Managers;
using Assets.Scripts.Models;
using Assets.Scripts.Utils;
using UnityEngine;

namespace Assets.Scripts.Components.Effects
{
	/// <summary>
	/// Component that adds effects on the top of a GameObject by using the collision boundaries.
	/// Call the <see cref="SetFx"/> method to set the correct Fx prefab.
	/// </summary>
	public class Effect : MonoBehaviour
	{
		private readonly List<GameObject> _effects = new List<GameObject>();

		/// <summary>
		/// This function will set the correct prefab/effect that is being used to spawn on top of gameobjects.
		/// </summary>
		/// <param name="fxObject"></param>
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
			// Destroy all the effects that we have in our memory
			foreach (GameObject effect in _effects)
			{
				Destroy(effect);
			}
		}
	}
}
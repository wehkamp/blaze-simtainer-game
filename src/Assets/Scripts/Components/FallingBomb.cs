using System.Linq;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Managers;
using UnityEngine;

namespace Assets.Scripts.Components
{
	/// <summary>
	/// Class used for the bomb that is falling out of the plane.
	/// </summary>
	internal class FallingBomb : MonoBehaviour
	{
		void Update()
		{
			// This can only happen when the bomb dropped on something that is not above the world
			if (transform.position.y < 0)
			{
				Destroy(gameObject);
				return;
			}

			// Make the bomb fall down
			transform.Translate(transform.up * -10f * Time.deltaTime);

			// Check if we hit the building we are aiming at
			if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 5f))
			{
				// Check if hit collider is actually a building
				if (!hit.collider.gameObject.CompareTag("Building")) return;

				IVisualizedObject target = CityManager.Instance.GameModel.Neighbourhoods
					.SelectMany(x => x.VisualizedObjects)
					.SingleOrDefault(x => x.GameObject == hit.collider.gameObject);
				if (target != null)
					StartCoroutine(ApiManager.Instance.KillVisualizedObject(target));
				else
					Debug.LogWarning("Target can not be found");

				// Create an explosion
				GameObject explosion = Instantiate(
					AssetsManager.Instance.GetPrefab(SettingsManager.Instance.Settings.AssetBundle.Chaos.ExplosionPrefab),
					hit.collider.transform.position, Quaternion.identity);

				Debug.Log($"KILL! {hit.collider.gameObject.name}");

				// Destroy the explosion and the bomb
				Destroy(explosion, 10f);
				Destroy(gameObject);
			}
		}
	}
}
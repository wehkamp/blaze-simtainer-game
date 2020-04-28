using System.Linq;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Managers;
using UnityEngine;

namespace Assets.Scripts.Components
{
	internal class FallingBomb : MonoBehaviour
	{

		void Update()
		{
			if (transform.position.y < 0)
			{
				Destroy(gameObject);
				return;
			}

			transform.Translate(transform.up * -10f * Time.deltaTime);

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

				GameObject explosion = Instantiate(
					AssetsManager.Instance.GetPrefab(SettingsManager.Instance.Settings.AssetBundle.Chaos.ExplosionPrefab),
					hit.collider.transform.position, Quaternion.identity);

				Debug.Log($"KILL! {hit.collider.gameObject.name}");
				Destroy(explosion, 10f);
				Destroy(gameObject);
			}
		}
	}
}
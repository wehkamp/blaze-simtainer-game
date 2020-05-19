using Assets.Scripts.Managers;
using Assets.Scripts.Models.Settings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Components
{
	public class BuildingLegend : MonoBehaviour
	{
		public GameObject Image;

		public GameObject LegendPanel;
		private float _heightDeltaX = 5f;

		// Start is called before the first frame update
		void Start()
		{
			AssetsManager.Instance.AssetsLoaded.AddListener(AssetsLoaded);
		}

		private void AssetsLoaded()
		{
			float panelHeightDeltaY = 0f;
			// Loop through all building prefabs from the configuration
			foreach (BuildingPrefab buildingPrefab in SettingsManager.Instance.Settings.AssetBundle.Buildings)
			{
				// Create an image prefab
				GameObject imageGameObject = Instantiate(Image, transform);
				imageGameObject.transform.SetParent(LegendPanel.transform);
				imageGameObject.SetActive(true);

				// Override the sprite
				Image img = imageGameObject.GetComponent<Image>();
				img.overrideSprite = AssetsManager.Instance.GetVehicleSpritesByType(buildingPrefab.Name);

				// Set the position of the icon
				RectTransform rt = imageGameObject.GetComponent<RectTransform>();
				rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 10f, rt.rect.width);
				rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, _heightDeltaX, rt.rect.height);

				// Set the label of the icon
				imageGameObject.GetComponentInChildren<TMP_Text>().text = buildingPrefab.Label;

				// Add some height
				_heightDeltaX += 35f;
				panelHeightDeltaY += 35f;

			}

			RectTransform panelRt = LegendPanel.GetComponent<RectTransform>();
			// Set the height of the panel
			panelRt.sizeDelta = new Vector2(panelRt.sizeDelta.x, panelRt.sizeDelta.y + panelHeightDeltaY);
		}
	}
}
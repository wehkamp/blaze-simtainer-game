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
		private float _heightDeltaX = 10f;

		// Start is called before the first frame update
		void Start()
		{
			AssetsManager.Instance.AssetsLoaded.AddListener(AssetsLoaded);

		}

		private void AssetsLoaded()
		{
			// RectTransform imageRt = Image.GetComponent<RectTransform>();
			// _heightDeltaX = imageRt.transform.localPosition.y;

			float panelHeightDeltaY = 0f;
			foreach (BuildingPrefab buildingPrefab in SettingsManager.Instance.Settings.AssetBundle.Buildings)
			{
				GameObject g = Instantiate(Image, transform);
				g.transform.SetParent(LegendPanel.transform);
				g.SetActive(true);
				Image img = g.GetComponent<Image>();
				img.overrideSprite = AssetsManager.Instance.GetVehicleSpritesByType(buildingPrefab.Name);

				RectTransform rt = g.GetComponent<RectTransform>();
				// rt.localPosition = new Vector3(rt.localPosition.x, rt.localPosition.y + _heightDeltaX,
				// 	rt.localPosition.z);
				Debug.Log($"{_heightDeltaX}");
				rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 10f, rt.rect.width);
				rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, _heightDeltaX, rt.rect.height);

				g.GetComponentInChildren<TMP_Text>().text = buildingPrefab.Label;
				_heightDeltaX += 35f;
				panelHeightDeltaY += 35.1f;

			}

			// Apply some panel settings
			RectTransform panelRt = LegendPanel.GetComponent<RectTransform>();

			float heightDeltaY = panelRt.sizeDelta.y;

			heightDeltaY += panelHeightDeltaY;
			panelRt.sizeDelta = new Vector2(panelRt.sizeDelta.x, heightDeltaY);
			LegendPanel.SetActive(false);
		}

		// Update is called once per frame
		void Update()
		{
		}
	}
}
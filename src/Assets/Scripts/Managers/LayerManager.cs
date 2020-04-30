using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Components.Effects;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Models;
using Assets.Scripts.Models.Layers;
using Assets.Scripts.Utils;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using LayerEffect = Assets.Scripts.Models.Settings.LayerEffect;

namespace Assets.Scripts.Managers
{
	internal class LayerManager : Singleton<LayerManager>
	{
		public GameObject LayerPanel;
		public GameObject LayerSettingsPanel;
		public VisualLayerModel SelectedLayer;

		// List with objects that are used for a selected layer
		private readonly List<GameObject> _selectedLayerObjects =
			new List<GameObject>();

		private IOrderedEnumerable<LayerEffect> _layerEffects;
		// Start is called before the first frame update
		void Start()
		{
			ApiManager.Instance.ApiInitializedEvent.AddListener(Setup);
			CityManager.Instance.CityUpdatedEvent.AddListener(LoadLayer);
		}

		public void Setup(GameModel gameModel)
		{
			if (SettingsManager.Instance.Settings.Layers.Enabled)
			{
				float y = -20f;
				foreach (VisualLayerModel layer in gameModel.Layers)
				{
					StartCoroutine(GenerateButtonWithTexture(layer, 0f, y));

					y -= 50f;
				}

				RectTransform rt = LayerPanel.GetComponent<RectTransform>();

				float heightDeltaY = rt.sizeDelta.y;

				heightDeltaY += gameModel.Layers.Count * 50;
				rt.sizeDelta = new Vector2(rt.sizeDelta.x, heightDeltaY);

				_layerEffects = SettingsManager.Instance.Settings.AssetBundle.LayerEffects.OrderByDescending(x=>x.Threshold);
			}
			else
			{
				Destroy(LayerSettingsPanel);
			}

			ApiManager.Instance.ApiInitializedEvent.RemoveListener(Setup);
		}

		/// <summary>
		/// This function will generate a button on the screen together with the texture received from the API.
		/// </summary>
		/// <param name="visualLayer"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		private IEnumerator GenerateButtonWithTexture(VisualLayerModel visualLayer, float x, float y)
		{
			string layerUrl = $"{SettingsManager.Instance.Settings.Api.BaseUrl}/{visualLayer.Icon}";
			UnityWebRequest www = UnityWebRequestTexture.GetTexture(layerUrl);
			yield return www.SendWebRequest();

			if (www.isNetworkError || www.isHttpError)
			{
				Debug.Log(www.error);
			}
			else
			{
				GameObject buttonObject = new GameObject();

				Button button = buttonObject.AddComponent<Button>();

				Image image = buttonObject.AddComponent<Image>();


				Texture2D webTexture = ((DownloadHandlerTexture) www.downloadHandler).texture;


				Sprite webSprite = webTexture.GenerateSprite();

				image.overrideSprite = webSprite;

				buttonObject.transform.SetParent(LayerPanel.transform);
				RectTransform rt = buttonObject.GetComponent<RectTransform>();
				rt.SetParent(LayerPanel.transform);

				rt.localPosition = new Vector3(x, y, 0f);
				rt.localScale = Vector3.one;
				rt.sizeDelta = new Vector2(32, 32);

				button.onClick.AddListener(delegate { OnClick(visualLayer); });
			}
		}

		/// <summary>
		/// When a layer is clicked, set and load the selected layer
		/// </summary>
		/// <param name="visualLayer"></param>
		private void OnClick(VisualLayerModel visualLayer)
		{
			// We do not want to spam the layers since there is no point of it
			if (visualLayer.Equals(SelectedLayer))
				return;
			SelectedLayer = visualLayer;
			LoadLayer();
		}

		/// <summary>
		/// This function will load a layer if clicked. Adding all effects to buildings.
		/// </summary>
		private void LoadLayer()
		{
			// Layer must be set before we execute this function
			if (SelectedLayer == null)
			{
				return;
			}

			ClearEffects();
			foreach (NeighbourhoodModel neighbourhood in CityManager.Instance.GameModel.Neighbourhoods)
			{
				LayerValueModel layerValueModel =
					neighbourhood.LayerValues.SingleOrDefault(x => x.LayerType == SelectedLayer.Name);
				if (layerValueModel == null) continue;

				foreach (IVisualizedBuilding visualizedObject in neighbourhood.VisualizedObjects.OfType<IVisualizedBuilding>())
				{
					if (visualizedObject.LayerValues == null || visualizedObject.GameObject == null)
					{
						return;
					}

					if (!visualizedObject.LayerValues.ContainsKey(SelectedLayer.Name)) continue;

					double layerValue = visualizedObject.LayerValues[SelectedLayer.Name];

					double avg = layerValue / layerValueModel.MaxValue;

					// Check if we hit the threshold and apply the effect to a building
					foreach (LayerEffect layerEffect in _layerEffects)
					{
						if (avg > layerEffect.Threshold)
						{
							visualizedObject.GameObject.AddComponent<Effect>().SetFx(AssetsManager.Instance.GetPrefab(layerEffect.PrefabName));
							_selectedLayerObjects.Add(visualizedObject.GameObject);
							break;
						}
					}
				}
			}
		}

		public void ClearEffects(bool clearSelectedLayer = false)
		{
			foreach (GameObject selectedObjects in _selectedLayerObjects)
			{
				Destroy(selectedObjects.GetComponent<Effect>());
			}

			_selectedLayerObjects.Clear();

			if (clearSelectedLayer)
				SelectedLayer = null;
		}
	}
}
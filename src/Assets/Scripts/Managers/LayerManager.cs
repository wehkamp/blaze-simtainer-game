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

		private readonly List<SpriteImageObject> _spriteObjects = new List<SpriteImageObject>();

		/// <summary>
		/// Struct used to combine an outline component and a visual layer model
		/// </summary>
		private struct SpriteImageObject
		{
			public readonly Image Image;
			public readonly VisualLayerModel VisualLayerModel;

			public SpriteImageObject(Image image, VisualLayerModel visualLayerModel)
			{
				Image = image;
				VisualLayerModel = visualLayerModel;
			}
		}

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
				// Default Y = -20
				float y = -20f;

				// Loop through all layers and generate a button with the correct texture
				foreach (VisualLayerModel layer in gameModel.Layers)
				{
					StartCoroutine(GenerateButtonWithTexture(layer, 0f, y));

					y -= 50f;
				}

				RectTransform rt = LayerPanel.GetComponent<RectTransform>();

				float heightDeltaY = rt.sizeDelta.y;

				// Set the correct height for the layers panel
				heightDeltaY += gameModel.Layers.Count * 50;
				rt.sizeDelta = new Vector2(rt.sizeDelta.x, heightDeltaY);

				_layerEffects =
					SettingsManager.Instance.Settings.AssetBundle.LayerEffects.OrderByDescending(x => x.Threshold);
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

				// Create a button
				Button button = buttonObject.AddComponent<Button>();

				// Create an image
				Image image = buttonObject.AddComponent<Image>();
		
				// The downloaded texture (should always have a white color)
				Texture2D webTexture = ((DownloadHandlerTexture) www.downloadHandler).texture;

				// Make a sprite of the downloaded texture
				Sprite webSprite = webTexture.GenerateSprite();

				image.sprite = webSprite;

				image.color = Color.black;
				buttonObject.transform.SetParent(LayerPanel.transform);

				// Set the correct position for the button
				RectTransform rt = buttonObject.GetComponent<RectTransform>();
				rt.SetParent(LayerPanel.transform);
				rt.localPosition = new Vector3(x, y, 0f);
				rt.localScale = Vector3.one;
				rt.sizeDelta = new Vector2(32, 32);

				// Set the name of the button
				buttonObject.name = visualLayer.Name;

				SpriteImageObject spriteImageObject = new SpriteImageObject(image, visualLayer);
				_spriteObjects.Add(spriteImageObject);

				// Create the onclick function
				button.onClick.AddListener(delegate { OnClick(spriteImageObject); });
			}
		}

		/// <summary>
		/// When a layer is clicked, set and load the selected layer
		/// </summary>
		/// <param name="selectedSpriteImageObject"></param>
		private void OnClick(SpriteImageObject selectedSpriteImageObject)
		{
			// We do not want to spam the layers since there is no point of it
			if (selectedSpriteImageObject.VisualLayerModel.Equals(SelectedLayer))
				return;



			foreach (SpriteImageObject sprite in _spriteObjects)
			{
				sprite.Image.color = sprite.VisualLayerModel.Equals(selectedSpriteImageObject.VisualLayerModel) ? Color.white : Color.black;
			}

			SelectedLayer = selectedSpriteImageObject.VisualLayerModel;

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

				foreach (IVisualizedBuilding visualizedObject in neighbourhood.VisualizedObjects
					.OfType<IVisualizedBuilding>())
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
							visualizedObject.GameObject.AddComponent<Effect>()
								.SetFx(AssetsManager.Instance.GetPrefab(layerEffect.PrefabName));
							break;
						}
					}
				}
			}
		}

		/// <summary>
		/// Function to clear all layer effects. Destroys all effects.
		/// </summary>
		/// <param name="clearSelectedLayer">Set this to true if you want to clear the selected layer.
		/// Used in closing the layer button.</param>
		public void ClearEffects(bool clearSelectedLayer = false)
		{
			// Find all effects that exists in the game
			Effect[] effects = FindObjectsOfType<Effect>();
			foreach (Effect effect in effects)
			{
				// Destroy the effect
				Destroy(effect);	
			}

			if (!clearSelectedLayer) return;

			SelectedLayer = null;
			foreach (SpriteImageObject spriteOutlineObject in _spriteObjects)
			{
				spriteOutlineObject.Image.color = Color.black;
			}
		}
	}
}
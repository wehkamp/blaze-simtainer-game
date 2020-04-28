using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Managers;
using Assets.Scripts.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Components
{
	public class NeighbourhoodSearch : MonoBehaviour
	{
		public Button SearchButton1;

		public Button SearchButton2;

		public GameObject SearchPanel;

		public TMP_Text NothingFoundText;

		public void OnTypingEnd(string searchString)
		{
			if (string.IsNullOrEmpty(searchString))
			{
				SearchPanel.SetActive(false);
			}
		}

		public void SearchNeighbourhood(string searchString)
		{
			if (!SearchPanel.activeSelf)
			{
				SearchPanel.SetActive(true);
			}

			List<NeighbourhoodModel> neighbourhoodModels =
				CityManager.Instance.GameModel.Neighbourhoods.Where(x => x.Name.Contains(searchString))
					.Take(2).ToList();

			switch (neighbourhoodModels.Count)
			{
				case 2:
					NothingFoundText.gameObject.SetActive(false);
					SetButton(neighbourhoodModels[0], SearchButton1);
					SetButton(neighbourhoodModels[1], SearchButton2);
					break;
				case 1:
					NothingFoundText.gameObject.SetActive(false);
					SetButton(neighbourhoodModels[0], SearchButton1);
					DisableButton(SearchButton2);
					break;
				default:
					NothingFoundText.gameObject.SetActive(true);
					DisableButton(SearchButton1);
					DisableButton(SearchButton2);
					break;
			}
		}

		private static void SetButton(NeighbourhoodModel neighbourhoodModel, Button button)
		{
			TMP_Text text = button.GetComponentInChildren<TMP_Text>();

			text.text = neighbourhoodModel.Name;

			button.gameObject.SetActive(true);

			button.onClick.RemoveAllListeners();
			button.onClick.AddListener(() => { OnButtonClick(neighbourhoodModel); });
		}

		private static void OnButtonClick(NeighbourhoodModel neighbourhoodModel)
		{
			IVisualizedObject visualizedBuilding =
				neighbourhoodModel.VisualizedObjects.FirstOrDefault(x =>
					x is IVisualizedBuilding && x.GameObject != null);
			if (visualizedBuilding == null) return;

			CameraController.Instance.FocusOnTarget(visualizedBuilding.GameObject.transform.position);
			OnObjectClickManager.Instance.ResetHighlighting();
			OnObjectClickManager.Instance.HighlightNeighbourhood(visualizedBuilding.GameObject);
		}

		private void DisableButton(Button button)
		{
			button.onClick.RemoveAllListeners();
			button.gameObject.SetActive(false);
		}
	}
}
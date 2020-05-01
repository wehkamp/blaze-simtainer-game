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
	/// <summary>
	/// This class is used for the search bar in the bottom panel.
	/// </summary>
	public class NeighbourhoodSearch : MonoBehaviour
	{
		public Button SearchButton1;

		public Button SearchButton2;

		public GameObject SearchPanel;

		public TMP_Text NothingFoundText;

		/// <summary>
		/// When the player stops with typing, it will hide the panel if the search string is empty
		/// </summary>
		/// <param name="searchString"></param>
		public void OnTypingEnd(string searchString)
		{
			if (string.IsNullOrEmpty(searchString))
			{
				SearchPanel.SetActive(false);
			}
		}

		/// <summary>
		/// Function to search for a neighbourhood and add suggestions in the bottom panel.
		/// </summary>
		/// <param name="searchString"></param>
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

		/// <summary>
		/// Function to bind a neighbourhood to a button
		/// </summary>
		/// <param name="neighbourhoodModel"></param>
		/// <param name="button"></param>
		private static void SetButton(NeighbourhoodModel neighbourhoodModel, Button button)
		{
			TMP_Text text = button.GetComponentInChildren<TMP_Text>();

			text.text = neighbourhoodModel.Name;

			button.gameObject.SetActive(true);

			button.onClick.RemoveAllListeners();
			button.onClick.AddListener(() => { OnButtonClick(neighbourhoodModel); });
		}

		/// <summary>
		/// Function that is executed when a button is pressed.
		/// The player will focus with the camera on the first building of a neighbourhood
		/// </summary>
		/// <param name="neighbourhoodModel"></param>
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

		/// <summary>
		/// Function to disable a button, happens when 1 or no options are found
		/// </summary>
		/// <param name="button"></param>
		private static void DisableButton(Button button)
		{
			button.onClick.RemoveAllListeners();
			button.gameObject.SetActive(false);
		}
	}
}
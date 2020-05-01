using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Components
{
	/// <summary>
	/// This class contains all functions used for the main menu.
	/// </summary>
	public class MainMenuScript : MonoBehaviour
	{
		public TMP_Text VersionText;

		private bool _loadingScene;

		/// <summary>
		/// Set the correct version on the main menu
		/// </summary>
		void Start()
		{
#if DEVELOPMENT_BUILD || UNITY_EDITOR
			VersionText.text = $"v{Application.version} - DEVELOPMENT BUILD";
#else
		VersionText.text = $"v{Application.version}";
#endif
		}

		/// <summary>
		/// Function to load a scene
		/// </summary>
		/// <param name="sceneName"></param>
		public void LoadScene(string sceneName)
		{
			if (_loadingScene) return;
			SceneManager.LoadSceneAsync(sceneName);
			_loadingScene = true;
		}

		/// <summary>
		/// Function to quit the game
		/// </summary>
		public void ExitGame()
		{
#if !UNITY_WEBGL
			Application.Quit();
#endif
		}
	}
}
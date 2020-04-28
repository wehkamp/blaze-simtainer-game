using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Components
{
	public class MainMenuScript : MonoBehaviour
	{
		public TMP_Text VersionText;

		// Start is called before the first frame update
		void Start()
		{
#if DEVELOPMENT_BUILD || UNITY_EDITOR
			VersionText.text = $"v{Application.version} - DEVELOPMENT BUILD";
#else
		VersionText.text = $"v{Application.version}";
#endif
		}
		public void LoadScene(string sceneName)
		{
			SceneManager.LoadSceneAsync(sceneName);
		}

		public void ExitGame()
		{
			Application.Quit();
		}
	}
}
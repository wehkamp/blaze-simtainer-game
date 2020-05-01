using System.Collections;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Models;
using Assets.Scripts.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace Assets.Scripts.Managers
{
	[System.Serializable]
	internal class ApiInitializedEvent : UnityEvent<GameModel>
	{
	}

	[System.Serializable]
	internal class ApiUpdateEvent : UnityEvent<UpdateEventModel>
	{
	}

	/// <summary>
	/// API manager which handles calling the API every x seconds.
	/// As soon as the API request has been done, the data will be parsed through a custom parse function and the data will be fired through the ApiChangeUnityEventManager.
	/// </summary>
	internal class ApiManager : Singleton<ApiManager>
	{
		private string _gameEndpoint;

		private const string HubListenerName = "SendUpdateToClients";
		private SignalRLib _srLib;
		[SerializeField] public GameObject AlertCanvas;

		private TMP_Text _alertText;

		[Header("LOADING SCREEN")] public GameObject LoadingMenu;
		private string _hubEndpoint;

		public ApiInitializedEvent ApiInitializedEvent = new ApiInitializedEvent();
		public ApiUpdateEvent ApiUpdateEvent = new ApiUpdateEvent();

		// Start is called before the first frame update
		void Start()
		{
			// Wait until the settings and assets are loaded, otherwise we don't know our base url and endpoints
			AssetsManager.Instance.AssetsLoaded.AddListener(LoadApiData);
			LoadingMenu.SetActive(true);
			_alertText = AlertCanvas.GetComponentInChildren<TMP_Text>();
		}

		/// <summary>
		/// Start loading the API data as soon as the assets are loaded.
		/// </summary>
		private void LoadApiData()
		{
			Debug.Log("Loading API data");
			_gameEndpoint = SettingsManager.Instance.Settings.Api.BaseUrl +
			                SettingsManager.Instance.Settings.Api.GameEndpoint;
			_hubEndpoint = SettingsManager.Instance.Settings.Api.BaseUrl +
			               SettingsManager.Instance.Settings.Api.EventHubEndpoint;
			StartCoroutine(LoadDataFromApi());
			AlertCanvas.SetActive(false);
		}

		/// <summary>
		/// Try to start the SignalR connection with the server.
		/// </summary>
		private void StartSignalR()
		{
			if (string.IsNullOrEmpty(SettingsManager.Instance.Settings.Api.EventHubEndpoint))
			{
				Debug.LogWarning("SignalR is not enabled, no endpoint is set. Real-time data is unavailable.");

				return;
			}

			_srLib = new SignalRLib();
			_srLib.Init(_hubEndpoint, HubListenerName);

			_srLib.Error += (sender, e) =>
			{
				_alertText.text =
					$"Error retrieving live data from server! Server seems to be down.";
				AlertCanvas.SetActive(true);
#if UNITY_EDITOR
				Debug.LogError($"SignalR gives an error: {e.Message}");
#endif
			};
			_srLib.ConnectionStarted += (sender, e) => { Debug.Log("Connection with server established."); };


			_srLib.MessageReceived += (sender, e) =>
			{
				// Disable alerting text if server went back up
				if (_alertText.gameObject.activeSelf)
					_alertText.gameObject.SetActive(false);

				UpdateEventModel updateEventModel = JsonParser.ParseUpdateEvent(e.Message);
				if (Application.isEditor && updateEventModel.UpdatedLayerValues == null)
					Debug.Log(e.Message);
				ApiUpdateEvent?.Invoke(updateEventModel);
			};
		}

		/// <summary>
		/// Function to kill a object through the API.
		/// </summary>
		/// <returns></returns>
		public IEnumerator KillVisualizedObject(IVisualizedObject building, bool force = false)
		{
			using (UnityWebRequest webRequest =
				UnityWebRequest.Delete($"{_gameEndpoint}?identifier={building.Identifier}&force={force}"))
			{
				// Request and wait for the desired page.
				webRequest.timeout = 10;
				yield return webRequest.SendWebRequest();
				if (webRequest.isNetworkError || webRequest.isHttpError)
				{
					Debug.LogError($"Webrequest failed! Error: {webRequest.error}");
				}

				yield return null;
			}

			yield return null;
		}

		/// <summary>
		/// Function to load the first data from the API.
		/// </summary>
		/// <returns></returns>
		private IEnumerator LoadDataFromApi()
		{
			using (UnityWebRequest webRequest = UnityWebRequest.Get(_gameEndpoint))
			{
				// Request and wait for the desired page.
				webRequest.timeout = 10;

				yield return webRequest.SendWebRequest();
				LoadingMenu.SetActive(false);
				if (webRequest.isNetworkError || webRequest.isHttpError)
				{
					Debug.LogError($"Webrequest failed! Error: {webRequest.error}");
					_alertText.text =
						$"Error retrieving data from API. Please connect to the VPN\r\nPress {KeyCode.Escape} to return to the main menu";
					AlertCanvas.SetActive(true);
				}
				else
				{
					AlertCanvas.SetActive(false);
					GameModel gameModel = JsonParser.ParseGameModel(webRequest.downloadHandler.text);
					ApiInitializedEvent?.Invoke(gameModel);
					LoadingMenu.SetActive(false);
					StartSignalR();
				}

				yield return null;
			}

			yield return null;
		}

		/// <summary>
		/// Kill the connection with SignalR when the API manager is being destroyed.
		/// </summary>
		void OnDestroy()
		{
			if (_srLib == null) return;
			Debug.Log("Disconnecting from server.");
			_srLib.Exit();
		}
	}
}
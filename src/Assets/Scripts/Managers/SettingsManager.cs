﻿using System;
using System.Collections;
using System.IO;
using Assets.Scripts.Models.Settings;
using Assets.Scripts.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace Assets.Scripts.Managers
{
	/// <summary>
	/// This class will serve the <see cref="SettingsModel"/> across the whole application.
	/// Default it is set with the default options in the models.
	/// Because we focus on WebGL it is not possible to load a file directly, we actually need to load through a web request.
	/// This means the config.json MUST be in the root of the web server, which is not the nicest way.
	/// </summary>
	internal class SettingsManager : Singleton<SettingsManager>
	{
		public UnityEvent SettingsLoadedEvent = new UnityEvent();
		public SettingsModel Settings { get; private set; } = new SettingsModel();

		public GameObject AlertCanvas;

		private TMP_Text _alertText;

		/// <summary>
		/// Read the settings on awake state.
		/// </summary>
		void Start()
		{
			_alertText = AlertCanvas.GetComponentInChildren<TMP_Text>();
			const string configFileName = "config.json";

			//Read our config file. If WebGL we actually need to do a web request
			if (Application.platform == RuntimePlatform.WebGLPlayer)
			{
				StartCoroutine(GetConfigJson());
			}
			else
			{
				try
				{
					StreamReader reader = new StreamReader(configFileName);
					Settings = JsonUtility.FromJson<SettingsModel>(reader.ReadToEnd());
					Debug.Log("Settings loaded");
					SettingsLoadedEvent?.Invoke();
				}
				catch (ArgumentException e)
				{
					Debug.LogError(e.Message);
					_alertText.text = $"Failed loading the configuration.\r\nInvalid configuration file.";
					AlertCanvas.SetActive(true);
				}
				catch (FileNotFoundException e)
				{
					Debug.LogError(e.Message);
					_alertText.text = $"Failed loading the configuration.\r\nMissing configuration file.";
					AlertCanvas.SetActive(true);
				}
			}
		}

		/// <summary>
		/// Load the config through a web request.
		/// </summary>
		/// <returns></returns>
		private IEnumerator GetConfigJson()
		{
			using (UnityWebRequest webRequest = UnityWebRequest.Get("/config.json"))
			{
				// Request and wait for the desired page.
				webRequest.timeout = 10;
				yield return webRequest.SendWebRequest();
				if (webRequest.isNetworkError || webRequest.isHttpError)
				{
					Debug.LogError($"Retrieving config failed");
					_alertText.text = "Failed loading the configuration.\r\nMissing configuration file";
					AlertCanvas.SetActive(true);
				}
				else
				{
					Settings = JsonUtility.FromJson<SettingsModel>(webRequest.downloadHandler.text);
					SettingsLoadedEvent?.Invoke();
					Debug.Log("Settings loaded");
				}

				yield return null;
			}
		}
	}
}
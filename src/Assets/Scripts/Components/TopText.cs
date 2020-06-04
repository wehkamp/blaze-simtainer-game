using System;
using Assets.Scripts.Components.Navigators;
using Assets.Scripts.Managers;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.Components
{
	public class TopText : MonoBehaviour
	{
		private TMP_Text _text;

		private string _planeText;

		private string _tankText;

		private string _defaultText = "Searching for a target";

		// Start is called before the first frame update
		void Start()
		{
			_tankText = _defaultText;
			_planeText = _defaultText;

			CameraManager.Instance.CameraChanged.AddListener(UpdateTextByActiveCamera);
			ChaosManager.Instance.TankTargetChanged.AddListener(OnTankTargetChanged);
			ChaosManager.Instance.PlaneTargetChangedEvent.AddListener(OnPlaneTargetChanged);
			_text = GetComponent<TMP_Text>();
			FindObjectOfType<PlaneNavigator>();
		}

		private void OnPlaneTargetChanged(string targetName)
		{
			_planeText = string.IsNullOrEmpty(targetName) ? _defaultText : $"Target: {targetName}";
			UpdateTextByActiveCamera();
		}

		private void OnTankTargetChanged(string targetName)
		{
			_tankText = string.IsNullOrEmpty(targetName) ? _defaultText : $"Target: {targetName}";
			UpdateTextByActiveCamera();
		}

		private void UpdateTextByActiveCamera()
		{
			switch (CameraManager.Instance.ActiveCamera)
			{
				case CameraManager.CameraType.MainCamera:
					_text.text = "";
					break;
				case CameraManager.CameraType.PlaneCamera:
					_text.text = _planeText;
					break;
				case CameraManager.CameraType.TankCamera:
					_text.text = _tankText;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}
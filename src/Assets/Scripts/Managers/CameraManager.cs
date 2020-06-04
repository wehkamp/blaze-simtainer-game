using System;
using Assets.Scripts.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.Managers
{
	internal class CameraManager : Singleton<CameraManager>
	{
		public Camera MainCamera;

		public Camera PlaneCamera { get; set; }

		public Camera TankCamera { get; set; }

		public UnityEvent CameraChanged;

		public CameraType ActiveCamera;

		public void SwitchCamera(CameraType cameraType)
		{
			switch (cameraType)
			{
				case CameraType.MainCamera:
					if (PlaneCamera != null)
						PlaneCamera.gameObject.SetActive(false);
					if (TankCamera != null)
						TankCamera.gameObject.SetActive(false);
					MainCamera.gameObject.SetActive(true);
					break;
				case CameraType.PlaneCamera:
					MainCamera.gameObject.SetActive(false);
					if (TankCamera != null)
						TankCamera.gameObject.SetActive(false);
					PlaneCamera.gameObject.SetActive(true);
					break;
				case CameraType.TankCamera:
					MainCamera.gameObject.SetActive(false);
					if (PlaneCamera != null)
						PlaneCamera.gameObject.SetActive(false);
					TankCamera.gameObject.SetActive(true);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(cameraType), cameraType, null);
			}
			ActiveCamera = cameraType;
			CameraChanged?.Invoke();
		}

		public enum CameraType
		{
			MainCamera,
			PlaneCamera,
			TankCamera
		}
	}
}
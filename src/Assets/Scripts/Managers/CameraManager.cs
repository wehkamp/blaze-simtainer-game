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

		public CameraType ActiveCameraType;
		public Camera ActiveCamera;

		void Start()
		{
			ActiveCamera = MainCamera;
			ActiveCameraType = CameraType.MainCamera;
		}

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
					ActiveCamera = MainCamera;
					break;
				case CameraType.PlaneCamera:
					MainCamera.gameObject.SetActive(false);
					if (TankCamera != null)
						TankCamera.gameObject.SetActive(false);
					PlaneCamera.gameObject.SetActive(true);
					ActiveCamera = PlaneCamera;
					break;
				case CameraType.TankCamera:
					MainCamera.gameObject.SetActive(false);
					if (PlaneCamera != null)
						PlaneCamera.gameObject.SetActive(false);
					TankCamera.gameObject.SetActive(true);
					ActiveCamera = TankCamera;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(cameraType), cameraType, null);
			}

			ActiveCameraType = cameraType;
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
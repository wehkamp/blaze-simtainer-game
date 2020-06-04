using System;
using System.Collections;
using Assets.Scripts.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

// Original script from https://boolean.games/dev_zone/rts_camera_controller
namespace Assets.Scripts.Managers
{
	internal class CameraController : Singleton<CameraController>

	{
		// Public Variables

		// How quickly the camera moves

		public float PanSpeed = 150f;

		// How quickly the camera rotates

		public float RotSpeed = 15f;

		// How quickly the camera zooms

		public float ZoomSpeed = 15f;

		// The minimum distance of the mouse cursor from the screen edge required to pan the camera

		public float BorderWidth = 10f;

		// Boolean to control if moving the mouse within the borderWidth distance will pan the camera

		public bool EdgeScrolling = true;

		// A placeholder for a reference to the camera in the scene

		public Camera Cam;

		//Private Variables

		// Minimum distance from the camera to the camera target

		private const float ZoomMin = 1.8f;

		// Maximum distance from the camera to the camera target

		private const float ZoomMax = 15.0f;

		// Floats to hold reference to the mouse position, no values to be assigned yet

		private float _mouseX, _mouseY;

		private float _maxX = 100;

		private float _maxZ = 100;

		private Vector3 _focusTarget;

		/// <summary>
		/// If this object is not null it means that the camera controller is following a target.
		/// IF you want to stop the following call the function <see cref="StopFollowingTarget()"/>
		/// </summary>
		public GameObject FollowTargetObject { get; private set; }

		private bool _focusOnTarget;
		private float _followSpeed = .1f;
		private bool _lockControls;

		void Start()
		{
			// On start, get a reference to the Main Camera
			Cam = Camera.main;

			// As soon as the grid is changed we calculate the boundaries
			GridManager.Instance.GridInitializedEvent.AddListener(CalculateMaxCameraBoundaries);
			_lockControls = false;
		}

		void LateUpdate()
		{
			if (_focusOnTarget)
			{
				StartCoroutine(LerpFromTo(transform.position, _focusTarget, 1f));
			}

			if (FollowTargetObject != null)
			{
				transform.position =
					Vector3.MoveTowards(transform.position, FollowTargetObject.transform.position, _followSpeed);
			}
		}


		void Update()

		{
			// Controls are locked, do nothing


			// Check if user is focused on an UI-element
			if (EventSystem.current.IsPointerOverGameObject() || !Cam.gameObject.activeInHierarchy) return;

			// Zoom and rotation is allowed of controls are locked
			Rotation();
			Zoom();

			if (_lockControls) return;
			Movement();
		}


		private void Movement()
		{
			// Local variable to hold the camera target's position during each frame

			Vector3 pos = transform.position;

			// Local variable to reference the direction the camera is facing (Which is driven by the Camera target's rotation)

			Vector3 forward = transform.forward;

			// Ensure the camera target doesn't move up and down

			forward.y = 0;

			// Normalize the X, Y & Z properties of the forward vector to ensure they are between 0 & 1

			forward.Normalize();


			// Local variable to reference the direction the camera is facing + 90 clockwise degrees (Which is driven by the Camera target's rotation)

			Vector3 right = transform.right;

			// Ensure the camera target doesn't move up and down

			right.y = 0;

			// Normalize the X, Y & Z properties of the right vector to ensure they are between 0 & 1

			right.Normalize();


			// Move the camera (camera_target) Forward relative to current rotation if "W" is pressed or if the mouse moves within the borderWidth distance from the top edge of the screen

			if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) || EdgeScrolling &&
				Input.mousePosition.y >= Screen.height - BorderWidth)
			{
				pos += forward * PanSpeed * Time.deltaTime;
			}


			// Move the camera (camera_target) Backward relative to current rotation if "S" is pressed or if the mouse moves within the borderWidth distance from the bottom edge of the screen

			if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) || EdgeScrolling &&
				Input.mousePosition.y <= BorderWidth)
			{
				pos -= forward * PanSpeed * Time.deltaTime;
			}


			// Move the camera (camera_target) Right relative to current rotation if "D" is pressed or if the mouse moves within the borderWidth distance from the right edge of the screen

			if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) || EdgeScrolling &&
				Input.mousePosition.x >= Screen.width - BorderWidth)
			{
				pos += right * PanSpeed * Time.deltaTime;
			}


			// Move the camera (camera_target) Left relative to current rotation if "A" is pressed or if the mouse moves within the borderWidth distance from the left edge of the screen

			if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) || EdgeScrolling &&
				Input.mousePosition.x <= BorderWidth)
			{
				pos -= right * PanSpeed * Time.deltaTime;
			}

			// Check if we are not crossing the map border
			if (pos.x <= -100f || pos.x >= _maxX || pos.z < -100f || pos.z >= _maxZ)
			{
				return;
			}
			// Setting the camera target's position to the modified pos variable

			transform.position = pos;
		}

		/// <summary>
		/// Function to check if the player rotates the mouse with right mouse button.
		/// </summary>
		private void Rotation()
		{
			// If Mouse Button 1 is pressed, (the secondary (usually right) mouse button)

			if (Input.GetMouseButton(1))

			{
				// Our mouseX variable gets set to the X position of the mouse multiplied by the rotation speed added to it.

				_mouseX += Input.GetAxis("Mouse X") * RotSpeed;

				// Our mouseX variable gets set to the Y position of the mouse multiplied by the rotation speed added to it.

				_mouseY -= Input.GetAxis("Mouse Y") * RotSpeed;

				// Clamp the minimum and maximum angle of how far the camera can look up and down.

				_mouseY = Mathf.Clamp(_mouseY, 30, 45);

				// Set the rotation of the camera target along the X axis (pitch) to mouseY (up & down) & Y axis (yaw) to mouseX (left & right), the Z axis (roll) is always set to 0 as we do not want the camera to roll.

				transform.rotation = Quaternion.Euler(_mouseY, _mouseX + transform.rotation.y, transform.rotation.z);
			}
		}

		/// <summary>
		/// Function to check if the player zooms with their scroll wheel.
		/// </summary>
		private void Zoom()
		{
			// Local variable to temporarily store our camera's position

			float size = Cam.fieldOfView;

			// Local variable to store the distance of the camera from the camera_target

			// When we scroll our mouse wheel up, zoom in if the camera is not within the minimum distance (set by our zoomMin variable)

			if ((Input.GetAxis("Mouse ScrollWheel") > 0f || Input.GetKey(KeyCode.KeypadPlus)) && size > ZoomMin)
			{
				size -= ZoomSpeed * Time.deltaTime;
			}


			// When we scroll our mouse wheel down, zoom out if the camera is not outside of the maximum distance (set by our zoomMax variable)

			if ((Input.GetAxis("Mouse ScrollWheel") < 0f || Input.GetKey(KeyCode.KeypadMinus)) && size < ZoomMax)
			{
				size += ZoomSpeed * Time.deltaTime;
			}


			// Set the camera's position to the position of the temporary variable
			if(size < ZoomMax && size > ZoomMin)
				Cam.fieldOfView = size;
		}

		/// <summary>
		/// Function for focusing on a target for a smooth transition between camera position and target position
		/// </summary>
		/// <param name="pos1"></param>
		/// <param name="pos2"></param>
		/// <param name="duration"></param>
		/// <returns></returns>
		IEnumerator LerpFromTo(Vector3 pos1, Vector3 pos2, float duration)
		{
			_lockControls = true;
			for (float t = 0f; t < duration; t += Time.deltaTime)
			{
				transform.position = Vector3.Lerp(pos1, pos2, t / duration);
				yield return 0;
			}

			transform.position = pos2;
			_lockControls = false;
			_focusOnTarget = false;
		}

		/// <summary>
		/// Function to calculate the maximum boundaries for the camera.
		/// </summary>
		private void CalculateMaxCameraBoundaries()
		{
			(int maxX, int maxZ) = BoundariesUtil.CalculateMaxGridBoundaries(GridManager.Instance);
			_maxX = maxX;
			_maxZ = maxZ;
		}

		/// <summary>
		/// This function will let you focus on a position of a target
		/// </summary>
		/// <param name="targetPos"></param>
		public void FocusOnTarget(Vector3 targetPos)
		{
			_focusTarget = targetPos;
			_focusOnTarget = true;
		}

		/// <summary>
		/// This function will let you follow a target until you call the function <see cref="StopFollowingTarget()"/>
		/// </summary>
		/// <param name="targetObj">The game object that you want to be followed</param>
		/// <param name="speed">The following speed, default .1f</param>
		public void FollowTarget(GameObject targetObj, float speed = .1f)
		{
			if (!Cam.gameObject.activeInHierarchy) return;
			_followSpeed = speed;
			FollowTargetObject = targetObj;
			_lockControls = true;
		}

		/// <summary>
		/// This function will set the follow target object to null and this causes to stop following the target.
		/// </summary>
		public void StopFollowingTarget()
		{
			FollowTargetObject = null;
			_lockControls = false;
		}
	}
}
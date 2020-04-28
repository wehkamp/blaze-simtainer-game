using Assets.Scripts.Utils;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.Components.Navigators
{
	/// <summary>
	/// This class is the navigator for NavMesh agents (vehicles).
	/// </summary>
	[RequireComponent(typeof(NavMeshAgent))]
	internal class VehicleNavigator : MonoBehaviour
	{
		/// <summary>
		/// Target of the object.
		/// </summary>
		public Transform Target;

		private NavMeshAgent _agent;

		private bool _startedPathFinding;

		private Vector3 _startPos;

		private bool _drivingBack;

		void Start()
		{
			_agent = GetComponent<NavMeshAgent>();
			_startPos = transform.position;
		}

		/// <summary>
		/// Set the target for the NavMeshAgent.
		/// </summary>
		/// <param name="target"></param>
		public void SetTarget(Transform target)
		{
			Target = target;
		}

		/// <summary>
		/// Function to start the path finding and enabling all renderers to make the object visible in the world.
		/// </summary>
		public void StartPathFinding(bool enableRenderers = true)
		{
			if (Target == null)
			{
				Destroy(gameObject);
			}
			else
			{
				if (_agent.isActiveAndEnabled)
				{
					_agent.SetDestination(_drivingBack ? _startPos : Target.transform.position);
					if (enableRenderers)
					{
						foreach (Renderer childRenderer in GetComponentsInChildren<Renderer>())
						{
							childRenderer.enabled = true;
						}
					}

					_startedPathFinding = true;
				}
				else
				{
					Destroy(gameObject);
				}
			}
		}

		// Update is called once per frame
		void LateUpdate()
		{
			// TODO: Haven't found a solution for it, but this can maybe be more optimized
			// Check if vehicle is still on-route
			if (!_startedPathFinding || !NavigationUtil.PathComplete(_agent)) return;

			// We are not marked to destroy, so we're driving back to were we came from
			_startedPathFinding = false;
			_drivingBack = !_drivingBack;
			StartPathFinding(false);
		}
	}
}
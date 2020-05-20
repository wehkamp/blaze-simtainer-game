using System;
using System.Collections;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Managers;
using Assets.Scripts.Utils;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.Components.Navigators
{
	[RequireComponent(typeof(NavMeshAgent))]
	internal class TankNavigator : MonoBehaviour
	{
		public IVisualizedObject Target { get; private set; }

		private NavMeshAgent _agent;

		public bool HasTarget { get; private set; }

		private GameObject _turret;

		private bool _rotating = false;

		/// <summary>
		/// Property to check if the tank is in standby mode.
		/// You can change the standby behavior in <see cref="SetStandby(bool)"/>
		/// </summary>
		public bool IsStandby { get; private set; }


		/// <summary>
		/// Property to see if the tank is ready to fire (will happen after rotating).
		/// </summary>
		public bool IsReadyToFire { get; private set; }
		
		/// <summary>
		/// Property to enable or disable firing after the tank is at target.
		/// </summary>
		public bool IsFiringEnabled { get; set; } = false;

		void Start()
		{
			_agent = GetComponent<NavMeshAgent>();
			_turret = GameObject.FindGameObjectWithTag("TankTurret");
		}

		/// <summary>
		/// Function to set the target of the tank
		/// </summary>
		/// <param name="target">The GameObject of the target should not be null</param>
		public void SetTarget(IVisualizedObject target)
		{
			IsStandby = false;
			Target = target;
			_agent.enabled = true;
			if (_agent.isActiveAndEnabled && target.GameObject != null)
			{
				_agent.isStopped = false;
				_agent.SetDestination(target.GameObject.transform.position);
				HasTarget = true;
			}
		}

		/// <summary>
		/// Function to reset the target of the tank.
		/// </summary>
		public void RemoveTarget()
		{
			Target = null;
			HasTarget = false;
		}

		// Update is called once per frame
		void Update()
		{
			// Check if we have a target and if we are actually at the target
			if (HasTarget && NavigationUtil.PathComplete(_agent))
			{
				if (!_rotating && Target != null)
				{
					_agent.isStopped = true;
					// We stopped rotating so we are ready to fire
					if (IsFiringEnabled)
						Fire();
					else
						RemoveTarget();
				}
			}
		}

		/// <summary>
		/// Function to destroy a building
		/// </summary>
		private void Fire()
		{
			if (!_rotating)
				StartCoroutine("Rotate", Quaternion.LookRotation(Target.GameObject.transform.position).eulerAngles.y);

			if (IsReadyToFire)
			{
				StartCoroutine(ApiManager.Instance.KillVisualizedObject(Target));
				if (Target.GameObject != null)
				{
					// Create an explosion
					GameObject explosion = Instantiate(
						AssetsManager.Instance.GetPredefinedPrefab(AssetsManager.PrefabType.ExplosionFx),
						Target.GameObject.transform.position, Quaternion.identity);

					// Destroy the explosion after 10 seconds since we don't want this object to stay in the world
					Destroy(explosion, 10f);
				}

				// Set target back to null
				Target = null;
				HasTarget = false;

				// Rotate the turret back to normal
				StartCoroutine(Rotate(transform.eulerAngles.y));
			}
		}

		/// <summary>
		/// Function to rotate the tank turret to the angle the target is
		/// </summary>
		/// <param name="targetRotationY"></param>
		/// <returns></returns>
		private IEnumerator Rotate(float targetRotationY)
		{
			_rotating = true;
			float startRotation = _turret.transform.eulerAngles.y;
			float t = 0.0f;
			while (Math.Abs(_turret.gameObject.transform.rotation.eulerAngles.y - targetRotationY) > 0.1f)
			{
				t += Time.deltaTime;
				float yRotation = Mathf.Lerp(startRotation, targetRotationY, t / 1.5f) % 360.0f;
				_turret.transform.eulerAngles = new Vector3(_turret.transform.eulerAngles.x, yRotation,
					_turret.transform.eulerAngles.z);
				yield return null;
			}

			IsReadyToFire = !IsReadyToFire;
			_rotating = false;
		}

		/// <summary>
		/// Function to set the tank in stand-by mode. Should be used when no valid target can be found.
		/// </summary>
		/// <param name="standby">Set this only to true if you want the tank to be in stand-by mode</param>
		public void SetStandby(bool standby)
		{
			Target = null;
			HasTarget = false;
			_agent.ResetPath();
			IsStandby = standby;
		}
	}
}
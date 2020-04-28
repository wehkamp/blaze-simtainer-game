using System;
using System.Collections;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Managers;
using Assets.Scripts.Models;
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
		public bool IsReadyToFire { get; private set; }
		public bool IsFiringEnabled { get; set; } = false;

		void Start()
		{
			_agent = GetComponent<NavMeshAgent>();
			_turret = GameObject.FindGameObjectWithTag("TankTurret");
		}

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

		public void RemoveTarget()
		{
			Target = null;
			HasTarget = false;
		}

		// Update is called once per frame
		void Update()
		{
			if (HasTarget && NavigationUtil.PathComplete(_agent))
			{
				if (!_rotating && Target != null)
				{
					_agent.isStopped = true;
					if (IsFiringEnabled)
						Fire();
					else
						RemoveTarget();
				}
			}
		}

		private void Fire()
		{
			if (!_rotating)
				StartCoroutine("Rotate", Quaternion.LookRotation(Target.GameObject.transform.position).eulerAngles.y);

			if (IsReadyToFire)
			{
				StartCoroutine(ApiManager.Instance.KillVisualizedObject(Target));
				if (Target.GameObject != null)
				{
					GameObject explosion = Instantiate(
						AssetsManager.Instance.GetPredefinedPrefab(AssetsManager.PrefabType.ExplosionFx),
						Target.GameObject.transform.position, Quaternion.identity);
					Destroy(explosion, 10f);
				}

				Target = null;
				HasTarget = false;
				StartCoroutine(Rotate(transform.eulerAngles.y));
			}
		}

		IEnumerator Rotate(float targetRotationY)
		{
			_rotating = true;
			float startRotation = _turret.transform.eulerAngles.y;
			float endRotation = startRotation + 360.0f;
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

		public void SetStandby(bool standby)
		{
			Target = null;
			HasTarget = false;
			_agent.ResetPath();
			IsStandby = standby;
		}
	}
}
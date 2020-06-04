using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.Utils
{
	internal static class NavigationUtil
	{
		/// <summary>
		/// Function to calculate if the path is complete to the target of a NavMeshAgent.
		/// </summary>
		/// <param name="agent"></param>
		/// <param name="maxDistance"></param>
		/// <returns></returns>
		public static bool PathComplete(NavMeshAgent agent, float maxDistance = 2)
		{
			float distance = Vector3.Distance(agent.destination, agent.transform.position);
			if (distance <= maxDistance)
			{
				if (!agent.hasPath || agent.velocity.sqrMagnitude < 5)
				{
					return true;
				}
			}

			return false;
		}
	}
}

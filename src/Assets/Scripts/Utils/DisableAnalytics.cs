using UnityEngine;
using UnityEngine.Analytics;

namespace Assets.Scripts.Utils
{
	internal class DisableAnalytics
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
		internal static void DisableAnalyticsOne()
		{
			Analytics.enabled = false;
			Analytics.deviceStatsEnabled = false;
			Analytics.initializeOnStartup = false;
			Analytics.limitUserTracking = false;
			PerformanceReporting.enabled = false;
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		internal static void DisableAnalyticsTwo()
		{
			Analytics.enabled = false;
			Analytics.deviceStatsEnabled = false;
			Analytics.initializeOnStartup = false;
			Analytics.limitUserTracking = false;
			PerformanceReporting.enabled = false;
		}
	}
}
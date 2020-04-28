using System;

namespace Assets.Scripts.Models.Settings
{
	[Serializable]
	public class Chaos
	{
		/// <summary>
		/// Bool to enable or disable chaos.
		/// </summary>
		public bool Enabled = true;

		/// <summary>
		/// Bool to enable or disable the air plane.
		/// </summary>
		public bool PlaneEnabled = true;

		/// <summary>
		/// Bool to enable or disable the wehtank.
		/// </summary>
		public bool TankEnabled = true;

		/// <summary>
		/// Amount of buildings that need to exists before we destroy it.
		/// </summary>
		public int MinimumBuildings = 2;
	}
}
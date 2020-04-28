using System;

namespace Assets.Scripts.Models.Settings
{
	[Serializable]
	public class Grid
	{
		/// <summary>
		/// This property will determine the tiles per street that is generated on start-up.
		/// </summary>
		public int TilesPerStreet = 50;
	}
}

using System;

#pragma warning disable 0649

namespace Assets.Scripts.Models.Settings
{
	/// <summary>
	/// The model that contains all settings for the game. This is loaded on start-up in the <see cref="Managers.SettingsManager"/>.
	/// </summary>
	[Serializable]
	public class SettingsModel
	{
		/// <summary>
		/// This property contains all available API settings.
		/// </summary>
		public Api Api;

		/// <summary>
		/// This property contains all available Grid settings.
		/// </summary>
		public Grid Grid;

		/// <summary>
		/// This property determines the threshold of when a building should be decayed.
		/// </summary>
		public int BuildingDecayAgeThreshold = 100;

		/// <summary>
		/// This property contains all available layer settings.
		/// </summary>
		public Layers Layers;

		/// <summary>
		/// This property contains all available chaos engineering settings.
		/// </summary>
		public Chaos Chaos;

		/// <summary>
		/// This property contains all available team settings.
		/// </summary>
		public Teams Teams;

		/// <summary>
		/// This property determines the assets bundle that is being loaded into the game.
		/// </summary>
		public AssetBundleSettings AssetBundle;
	}
}
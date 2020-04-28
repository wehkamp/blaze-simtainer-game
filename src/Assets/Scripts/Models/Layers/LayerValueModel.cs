namespace Assets.Scripts.Models.Layers
{
	internal enum LayerEffect
	{
		Fire,
		Shaking,
		Smoke
	}

	internal class LayerValueModel
	{
		/// <summary>
		/// The type of a layer
		/// </summary>
		public string LayerType { get; set; }

		/// <summary>
		/// Maximum value of a layer, so we can calculate a threshold.
		/// </summary>
		public double MaxValue { get; set; }

		/// <summary>
		/// Minimum value of a layer, so we can calculate a threshold.
		/// </summary>
		public double MinValue { get; set; }
	}
}
namespace Assets.Scripts.Models.Layers
{
	internal class VisualLayerModel
	{
		/// <summary>
		/// Name of a layer, for example cpuLayer. The name Layer is stripped away everywhere in the game.
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		/// Icon of a layer, must exists on the back-end API in the /images folder.
		/// </summary>
		public string Icon { get; set; }

		protected bool Equals(VisualLayerModel other)
		{
			return Name == other.Name && Icon == other.Icon;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((VisualLayerModel) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((Name != null ? Name.GetHashCode() : 0) * 397) ^ (Icon != null ? Icon.GetHashCode() : 0);
			}
		}
	}
}

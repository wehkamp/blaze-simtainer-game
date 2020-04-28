using Assets.Scripts.Interfaces;
using UnityEngine;

namespace Assets.Scripts.Models.VisualizedObjects
{
	internal class VisualizedGrassTileModel : IVisualizedObject
	{
		public string Type { get; } = "grass";
		public int Size { get; } = 0;
		public string Identifier { get; } = string.Empty;
		public GameObject GameObject { get; set; }
	}
}

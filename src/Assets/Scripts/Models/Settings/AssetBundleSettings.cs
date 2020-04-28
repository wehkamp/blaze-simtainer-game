using System;
using System.Collections.Generic;

#pragma warning disable 0649

namespace Assets.Scripts.Models.Settings
{
	[Serializable]
	public class AssetBundleSettings
	{
		public string Name = "free";
		public int BuildingDecayAgeThreshold = 100;
		public string StagingBuildingPrefab;
		public List<BuildingPrefab> Buildings;
		public List<VehiclePrefab> Vehicles;
		public string Grass;
		public DestroyTiles DestroyedTiles;
		public List<LayerEffect> LayerEffects;
		public ChaosPrefabs Chaos;
		public RoadPrefabs Roads;
	}

	[Serializable]
	public class ChaosPrefabs
	{
		public string TankPrefab;
		public string PlanePrefab;
		public string BombPrefab;
		public string ExplosionPrefab;
	}

	[Serializable]
	public class DestroyTiles
	{
		public List<string> RandomTiles;
		public string Fx;
	}

	[Serializable]
	public class RoadPrefabs
	{
		public string RoadStraight;
		public string RoadTSection;
		public string RoadIntersection;
		public string RoadCorner;
	}
	[Serializable]
	public class BuildingPrefab
	{
		public List<Prefab> Prefabs;
		public List<Prefab> DecayedPrefabs;
		public int MinSize;
	}

	[Serializable]
	public class VehiclePrefab
	{
		public List<string> PrefabNames;
		public int MinSize;
		public float Speed;
		public string Name;
	}

	[Serializable]
	public class LayerEffect
	{
		public string PrefabName;
		public float Threshold;
	}

	[Serializable]
	public class Prefab
	{
		public string Name;
		public float Rotation;
	}
}
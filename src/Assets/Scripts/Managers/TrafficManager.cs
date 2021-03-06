﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Components.Navigators;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Models;
using Assets.Scripts.Models.VisualizedObjects;
using Assets.Scripts.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.Managers
{
	[Serializable]
	internal class VehicleSpawnEvent : UnityEvent<TrafficManager.Vehicle>
	{
	}

	internal class TrafficManager : Singleton<TrafficManager>
	{
		private readonly Queue<Vehicle> _vehicleQueue = new Queue<Vehicle>();

		public UnityEvent TrafficUpdateEvent = new UnityEvent();
		public VehicleSpawnEvent VehicleSpawned = new VehicleSpawnEvent();

		/// <summary>
		/// Vehicle struct. This is used so we can keep track of the vehicles.
		/// </summary>
		public struct Vehicle
		{
			public GameObject VehicleGameObject { get; }
			public VisualizedVehicleModel VehicleModel { get; }
			public NeighbourhoodModel NeighbourhoodModel { get; }

			public string VehicleName { get; }

			public Vehicle(GameObject vehicleGameObject, VisualizedVehicleModel vehicleModel,
				NeighbourhoodModel neighbourhoodModel, string vehicleName)
			{
				VehicleGameObject = vehicleGameObject;
				VehicleModel = vehicleModel;
				NeighbourhoodModel = neighbourhoodModel;
				VehicleName = vehicleName;
			}
		}

		public List<Vehicle> Vehicles { get; set; }

		public void Start()
		{
			Vehicles = new List<Vehicle>();
			ApiManager.Instance.ApiUpdateEvent.AddListener(UpdateEvent);
			ApiManager.Instance.ApiInitializedEvent.AddListener(StartEvent);
			GridManager.Instance.GridInitializedEvent.AddListener(GridInitialized);
		}

		private void GridInitialized()
		{
			StartCoroutine(ProcessQueue());
		}

		/// <summary>
		/// Function to process vehicle queue. Every vehicle has a navigator <see cref="VehicleNavigator"/>.
		/// The path has already been set in the <see cref="SpawnVehicle(VisualizedVehicleModel)"/> function
		/// We call the <see cref="VehicleNavigator.StartPathFinding"/> function.
		/// This function will enable all the renderers, making the object visible and the vehicle starts to navigate
		/// As soon as a vehicle reached their destination it will drive back to the start position of the vehicle
		/// </summary>
		/// <returns></returns>
		private IEnumerator ProcessQueue()
		{
			while (true)
			{
				while (_vehicleQueue.Count > 0)
				{
					// Grab the vehicle from the queue
					Vehicle v = _vehicleQueue.Dequeue();
					// Check if in the main time the neighbourhood is not de-spawned together with the vehicle
					if (v.VehicleGameObject != null && v.NeighbourhoodModel != null)
					{
						// Start the path finding of the vehicle
						v.VehicleGameObject.GetComponent<VehicleNavigator>().StartPathFinding();
						// Fire the event that we spawned a vehicle
						VehicleSpawned?.Invoke(v);

						// Check if the vehicle is already in the list of the neighbourhood
						IVisualizedObject vehicle = v.NeighbourhoodModel.VisualizedObjects.SingleOrDefault(x =>
							x is VisualizedVehicleModel && x.Identifier == v.VehicleModel.Identifier);
						// If vehicle is not in the list of visualized objects of a neighbourhood, add it
						if (vehicle == null)
						{
							v.VehicleModel.GameObject = v.VehicleGameObject;
							v.NeighbourhoodModel.VisualizedObjects.Add(v.VehicleModel);
						}
						else
							vehicle.GameObject = v.VehicleGameObject;
					}

					yield return new WaitForSeconds(0.3f);
				}

				yield return new WaitForSeconds(0.1f);
			}
		}

		/// <summary>
		/// As soon as the API is initialized we start to spawn vehicles.
		/// </summary>
		/// <param name="gameModel"></param>
		private void StartEvent(GameModel gameModel)
		{
			foreach (VisualizedVehicleModel updatedVehicle in gameModel.Neighbourhoods
				.SelectMany(x => x.VisualizedObjects)
				.OfType<VisualizedVehicleModel>().Shuffle())
			{
				if (Vehicles.All(x => x.VehicleModel.Identifier != updatedVehicle.Identifier))
				{
					if (updatedVehicle.Size > 0)
						SpawnVehicle(updatedVehicle);
				}
			}

			TrafficUpdateEvent.Invoke();
		}

		/// <summary>
		/// This function will check if vehicles needs to be marked as destroyed or if we need to spawn new vehicles.
		/// </summary>
		/// <param name="updateEventModel"></param>
		private void UpdateEvent(UpdateEventModel updateEventModel)
		{
			if (updateEventModel.UpdatedVisualizedObjects == null) return;

			// Select all vehicles that we currently have.
			List<VisualizedVehicleModel> oldVehicles = Vehicles.Select(x => x.VehicleModel).ToList();

			// Select all the new vehicles we got from the API.
			List<VisualizedVehicleModel> newVehicles =
				updateEventModel.UpdatedVisualizedObjects.OfType<VisualizedVehicleModel>().ToList();

			// Make a list of all the vehicles that are not in the new batch of vehicles.
			List<VisualizedVehicleModel> removedVehicles = oldVehicles.Except(newVehicles).ToList();

			// Make a list of newly added vehicles.
			List<VisualizedVehicleModel>
				addedVehicles = newVehicles.Except(oldVehicles).Where(v => v.Size > 0).ToList();

			// Remove all the old vehicles and mark them to destroy.
			foreach (VisualizedVehicleModel removedVehicle in removedVehicles)
			{
				Vehicle v = Vehicles.Single(x => x.VehicleModel.Equals(removedVehicle));

				if (v.NeighbourhoodModel.VisualizedObjects.Contains(v.VehicleModel))
					v.NeighbourhoodModel.VisualizedObjects.Remove(v.VehicleModel);
				if (CameraController.Instance.FollowTargetObject == v.VehicleGameObject)
					CameraController.Instance.StopFollowingTarget();
				Destroy(v.VehicleGameObject);
				Vehicles.Remove(v);
			}

			// Spawn all the new vehicles in a random order.
			foreach (VisualizedVehicleModel addedVehicle in addedVehicles)
			{
				SpawnVehicle(addedVehicle);
			}

			TrafficUpdateEvent.Invoke();
		}

		/// <summary>
		/// Function to spawn a vehicle. The vehicle will spawn invisible with their target already set.
		/// </summary>
		/// <param name="visualizedVehicleModel"></param>
		private void SpawnVehicle(VisualizedVehicleModel visualizedVehicleModel)
		{
			// Select a target. The identifier of a vehicle is the same as the identifier for a building!
			var target = CityManager.Instance.GameModel.Neighbourhoods
				.SelectMany(p => p.VisualizedObjects,
					(neighbourhood, visualizedObject) => new
						{Neighbourhood = neighbourhood, VisualizedObject = visualizedObject})
				.Where(x => x.VisualizedObject is IVisualizedBuilding)
				.FirstOrDefault(x => x.VisualizedObject.Identifier == visualizedVehicleModel.Identifier);

			// If target is null, means target is already removed. So we don't want to spawn a vehicle.
			if (target == null) return;

			// Vehicles do not have an age (yet)

			string vehicleName = AssetsManager.Instance.GetVehicleName(visualizedVehicleModel.Size);
			KeyValuePair<Vector3, Quaternion> spawnPoint =
				GridManager.Instance.VehicleSpawnPoints.PickRandom();
			GameObject vehicleGameObject = Instantiate(AssetsManager.Instance.GetVehiclePrefab(vehicleName),
				spawnPoint.Key, spawnPoint.Value);

			// Disable all renderers. We make the vehicle invisible. 
			// If we use the SetActive the game starts to freeze with every vehicle we spawn.
			// So we just keep the spawned vehicle but we make it invisible.
			// TODO: Implement a better way to do this
			foreach (Renderer childRenderer in vehicleGameObject.GetComponentsInChildren<Renderer>())
			{
				childRenderer.enabled = false;
			}

			// Add the vehicleNavigator component to the vehicle
			VehicleNavigator vehicleNavigator = vehicleGameObject.AddComponent<VehicleNavigator>();

			// Set vehicle name to identifier, so we know when it's clicked, to which visualized object the vehicle belongs.
			vehicleGameObject.name = $"vehicle-{visualizedVehicleModel.Identifier}";

			// Check if the game object is not null and set the target so we can already calculate the path.
			if (target.VisualizedObject.GameObject != null)
				vehicleNavigator.SetTarget(target.VisualizedObject.GameObject.transform);

			// Make a new vehicle object
			Vehicle v = new Vehicle(vehicleGameObject, visualizedVehicleModel, target.Neighbourhood, vehicleName);

			// And put the vehicle object in the queue so it's ready to "spawn".
			_vehicleQueue.Enqueue(v);

			// And add the vehicle to a list so we can keep track of it.
			Vehicles.Add(v);
		}
	}
}
# SimTainer

A game to visualize the cloud stack of wehkamp real-time with elements of resource management and chaos engineering! Open sourced, so you can try to set it up for your own environment. 

Every building in the image below represents a container and every vehicle driving around network traffic.

The information is provider by a back-end which is set-up as generic as possible, so you can visualize other things as well.
For the back-end in the example: Marathon, Mesos and Prometheus are used.

Back-end project can be found here: <https://github.com/wehkamp/blaze-simtainer-service>

![alt text](img/simtainer.gif "Example of the game")

## 1. Getting started

Before you proceed: **This game is still a work in progress. If you find any bug, please let us know and create an issue**

The open-source version is without the assets you see in the picture above. This version has own created prefabs which are created within an hour to strip the assets (I'm not a 3D artist unfortunately), which are not quite nice. You can of course download a free asset pack, but it's not allowed to include these in this repo.

These assets are not complete! So there is no scooter, car, van etc. Only a car, plane, tank and a bus.
The code base is the same as the one with the paid assets version.

If you download the compiled release, the nice looking assets are included.

### 1.1 Installation

To build/compile this application you must install Unity 2019.3.10f1. And to decorate the game like we did, you can purchase the assets as listed below.

The application was mainly focused on WebGL and therefore fully compatible with WebGL.

The WebGL performance is not that great with lots of data (only when revealing the whole map), Windows/Mac builds perform way better.

If you only want to use the game, [download](https://github.com/wehkamp/blaze-simtainer-game/releases) the game from the releases tab.

The game is available as Windows, Mac or WebGL download.

### 1.2 Used assets

Open-source assets:

* In-game Debug Console for Unity 3D - <https://github.com/yasirkula/UnityIngameDebugConsole> by Süleyman Yasir Kula

Paid assets that are not included:

* City Adventure - <https://assetstore.unity.com/packages/3d/environments/city-adventure-65307> by beffio
* Simple Apocalypse - Cartoon Assets - <https://assetstore.unity.com/packages/3d/environments/simple-apocalypse-cartoon-assets-44678>

Free assets that are not included:

* Skybox add-on - <https://assetstore.unity.com/packages/2d/textures-materials/sky/skybox-add-on-136594>

You can replace all the assets you want in the config file that is included.
If you want to use these assets, read chapter 5.1

### 1.3 Used libraries

* SignalR support for WebGL (included) - <https://github.com/evanlindsey/Unity-WebGL-SignalR>
* SimpleJSON (included) - <https://wiki.unity3d.com/index.php/SimpleJSON>
* UnityRuntimePreviewGenerator (included) - <https://github.com/yasirkula/UnityRuntimePreviewGenerator>
* NavMeshComponents (included) - <https://github.com/Unity-Technologies/NavMeshComponents>

### 1.4 Controls

| Key                    | Action                                                          |
|------------------------|-----------------------------------------------------------------|
| W / Up Arrow           | Move the camera forward                                         |
| A / Left Arrow         | Move the camera to the left                                     |
| S / Down Arrow         | Move the camera backwards                                       |
| D / Right Arrow        | Move the camera to the right                                    |
| Escape                 | Go back to the main menu or escape different camera view        |
| F4                     | Open the debug console                                          |
| Left mouse button      | When clicked on a building or vehicle it displays information   |
| Hold right mouse button| Rotate the camera around                                        |
| Scroll / KP + or -     | Zoom in or out                                                  |

## 2. Configuration

The game is fully configurable from a JSON file.
To change the configuration of the game, open the [config.json](src/config.json).

If you build the project yourself, always include your config.json in your game directory otherwise default settings will be used.

You can change the prefabs of buildings, vehicles, road etc. This has been done to make it extremely easy to switch between asset packs.

The game makes use of so called asset bundles. This is an Unity feature where you can easily deploy asset bundles.

This will mean you only need to make a pack of assets, edit the game config file and you can include them within the game for easy deployment.

No need to modify/compile the game yourself.

### Assets

To build the assets bundle, read chapter 5.1.
When you do not make use of WebGL, place the AssetBundle in the Data folder <https://docs.unity3d.com/ScriptReference/Application-dataPath.html> of the game.

If you're using the back end we provided, rename the bundle ending with .unityweb  and edit the `config.json` and include the extension in the name of the asset bundle.

Assets can also be placed on a webserver. If you change the name of the AssetBundle starting with http, the game will automatically detect it needs to get the assets from a webserver.

### Notes

#### **1: If you use WebGL and the base url is the same as the server your hosting on, leave it blank**

#### **2: If you use WebGL the config.json must in the root of the webserver, otherwise the game can not find it or you must edit the [SettingsManager.cs](src/Assets/Scripts/Managers/SettingsManager.cs) and change the location. I have not found another solution for it yet.**

#### **3: If you do not want to use real-time updates/SignalR, leave the EventHubEndpoint blank**

Example configuration:

```jsonc
{
  "Api": {
    "BaseUrl": "", // Base URL of the API. Leave it blank if you use WebGL on the same server as the game is hosted on
    "EventHubEndpoint": "/hubs/cloudstack/game", // Endpoint to the SignalR hub of the game
    "GameEndpoint": "/v1/cloudstack/game" // Endpoint of the game information
  },
  "Chaos": {
    "Enabled": true, // Enable or disable chaos engineering
    "PlaneEnabled": true, // Plane enabled to drop bombs on buildings (destroy containers)
    "TankEnabled": true, // Tank enabled to shoot at buildings (destroy containers)
    "MinimumBuildings": 2 // Minimum amount of buildings that are required before a hit will happen (in before production problems)
  },
  "Grid": {
    "TilesPerStreet": 50 // Amount of tiles that are spawned per street
  },
  "Layers": {
    "Enabled": true // Enable or disable layers
  },
  "Teams": {
    "Enabled": true // Enable or disable teams
  },
  "AssetBundle": {
    "Name": "free", // Name of how you want the asset bundle to be called
    "BuildingDecayAgeThreshold": 100, // Threshold of when buildings should be decayed (age of a building)
    "StagingBuildingPrefab": "BuildingConstruction", // Prefab when buildings are being built-up
    "Buildings": [
      {
        "Prefabs": [
          {
            "Name": "ExtraSmallBuilding", // Name of the prefab you want to use as building
            "Rotation": 0 // Rotation of the prefab
          }
        ],
        "DecayedPrefabs": [
          {
            "Name": "ExtraSmallBuildingDecayed", // Name of the prefab you want to use as a decayed building
            "Rotation": 0 // Rotation of the decayed prefab
          }
        ],
        "MinSize": 0 // Minimal size of a building to be used
      },
      {
        "Prefabs": [
          {
            "Name": "SmallBuilding",
            "Rotation": 0
          }
        ],
        "DecayedPrefabs": [
          {
            "Name": "SmallBuildingDecayed",
            "Rotation": 0
          }
        ],
        "Label": "$",
        "Name": "BuildingS",
        "MinSize": 15
      },
      {
        "Prefabs": [
          {
            "Name": "MediumBuilding",
            "Rotation": 0
          }
        ],
        "DecayedPrefabs": [
          {
            "Name": "MediumBuildingDecayed",
            "Rotation": 0
          }
        ],
        "Label": "$$",
        "Name": "BuildingM",
        "MinSize": 20
      },
      {
        "Prefabs": [
          {
            "Name": "LargeBuilding",
            "Rotation": 0
          }
        ],
        "DecayedPrefabs": [
          {
            "Name": "LargeBuildingDecayed",
            "Rotation": 0
          }
        ],
        "Label": "$$$",
        "Name": "BuildingL",
        "MinSize": 30
      }
    ],
    "Vehicles": [
      {
        "Name": "Car", // Name of the category of a vehicle
        "PrefabNames": [
          "Car" // Name of prefabs used a vehicle. The first vehicle from this list is picked as a sprite for the UI
        ],
        "MinSize": 0, // Minimal size of the vehicle
        "Speed": 15 // Speed of the vehicle
      },
      {
        "Name": "Truck",
        "PrefabNames": [
          "Truck"
        ],
        "MinSize": 500,
        "Speed": 10
      }
    ],
    "Grass": "GrassTile",
    "DestroyedTiles": { // Destroyed tiles are used when a building is destroyed and are randomly picked
      "RandomTiles": [
        "Tile_Grass_Destroyed1",
        "Tile_Grass_Destroyed2",
        "Tile_Grass_Destroyed3"
      ],
      "Fx": "Destroy_FX" // FX when a building is being destroyed
    },
    "LayerEffects": [
      {
        "PrefabName": "Fire_FX", // FX when the threshold of a layer is >= 0.9
        "Threshold": 0.9
      },
      {
        "PrefabName": "Smoke_FX", // FX when the threshold of a layer is >= 0.8
        "Threshold": 0.8
      }
    ],
    "Chaos": {
      "TankPrefab": "Tank", // Prefab for the tank
      "PlanePrefab": "Plane", // Prefab for the plane
      "BombPrefab": "Bomb", // Prefab for the bomb dropping out of the plane
      "ExplosionPrefab": "Explosion_FX" // Prefab effect when a building is being hit by a bomb or a tank
    },
    "Roads": { // Prefabs for roads
      "RoadStraight": "Road",
      "RoadTSection": "Road",
      "RoadIntersection": "Road",
      "RoadCorner": "Road"
    }
  }
}
```

All of those settings are parsed in the SettingsManager.cs class which you can find at [SettingsManager.cs](src/Assets/Scripts/Managers/SettingsManager.cs).

## 3. API Set-up

### 3.1 Communicate with the API

Communication with the API happens in 2 forms. Initialization of the data and real-time updating the data.

Every API call for data is handled in the [ApiManager.cs](src/Assets/Scripts/Managers/ApiManager.cs). There is 1 more place where a call happens, and that is in the [LayerManager.cs](src/Assets/Scripts/Managers/LayerManager.cs). It downloads an image from the back-end to make the layers dynamic.

#### 3.2 Initialization of the data

Data is being initialized by doing an API call to the endpoint /v1/cloudstack/game. It receives a model

The game model looks like the following:

```jsonc
{
  "neighbourhoods": [
    {
      "name": "authorization-service", // Name of a neighbourhood
      "visualizedObjects": [
        {
          "type": "building", // Type of the object, can be staging-building, building or vehicle
          "size": 7, // Size of the object
          "layerValues": { 
            "memoryLayer": 434, // Current layer value
            "cpuLayer": 6.8 // Current layer value
          },
          "identifier": "598af9be-7f40-46eb-8e3e-5059183396ac" // Unique identifier of the building
        },
        {
          "type": "building",
          "size": 7,
          "layerValues": {
            "memoryLayer": 434,
            "cpuLayer": 5
          },
          "identifier": "94f0fdb1-6dfe-4e92-afce-6a99fb774870"
        }
      ],
      "layerValues": [
        {
          "layerType": "cpuLayer",
          "maxValue": 100,
          "minValue": 0
        },
        {
          "layerType": "memoryLayer",
          "maxValue": 500,
          "minValue": 0
        }
      ],
      "daysOld": 95,
      "team": "authorization-developers"
    },
    {
      "name": "search-service",
      "visualizedObjects": [
        {
          "type": "building",
          "size": 7,
          "layerValues": {
            "memoryLayer": 499,
            "cpuLayer": 3.1
          },
          "identifier": "9f9180d9-5452-44cb-be86-019798213cf4"
        },
        {
          "type": "building",
          "size": 7,
          "layerValues": {
            "memoryLayer": 499,
            "cpuLayer": 3.1
          },
          "identifier": "011774df-0af3-4ff4-9672-f3bd16de5fc2"
        }
      ],
      "layerValues": [
        {
          "layerType": "cpuLayer",
          "maxValue": 100,
          "minValue": 0
        },
        {
          "layerType": "memoryLayer",
          "maxValue": 500,
          "minValue": 0
        }
      ],
      "daysOld": 150,
      "team": "search-developers"
    }
  ],
  "layers": [ // Types of layers
    {
      "icon": "images/cpu_icon.png", // Location to image on the back-end for example https://simtainer.yourorganization.local/images/cpu_icon.png
      "layerType": "cpuLayer" // Name of the layer
    },
    {
      "icon": "images/ram_icon.png",
      "layerType": "memoryLayer"
    }
  ],
  "teams": [
    "authorization-developers", // Teams of your organization
    "search-developers"
  ]
}
```

#### 3.3 Real-time data updates

Data is real-time updated by the SignalR library.
The following plug-in is used: <https://github.com/evanlindsey/Unity-WebGL-SignalR>.

If you want to use WebGL, you need to adjust your index.html and add a reference to the SignalR library in the header.

```html
<script src="https://cdn.jsdelivr.net/npm/@microsoft/signalr@3.1.2/dist/browser/signalr.min.js"></script>
```

A JSON string must be sent to the client, not a model. The JSON is being parsed by the game.

You don't need to fill every key. Only the values you want to update.

The model for event updates is as followed:

```jsonc
{
  "neighbourhoodName": "authorization-service",
  "addedNeighbourhood": {
    "name": "authorization-service-v2",
    "visualizedObjects": [],
    "layerValues": [
      {
        "layerType": "cpuLayer",
        "maxValue": 100,
        "minValue": 0
      },
      {
        "layerType": "memoryLayer",
        "maxValue": 500,
        "minValue": 0
      }
    ],
    "daysOld": 95,
    "team": "authorization-developers"
  },
  "removedNeighbourhood": "IDENTIFIER-OF-NEIGHBOURHOOD", // for example authorization-service
  "addedVisualizedObject": {
          "type": "staging-building",
          "size": 7,
          "layerValues": null,
          "identifier": "011774df-0af3-4ff4-9672-f3bd16de5fc2"
  },
  "removedVisualizedObject": "IDENTIFIER-OF-A-BUILDING",
  "updatedLayerValues": {
    "IDENTIFIER-OF-A-BUILDING": {
      "cpuLayer": 0.1,
      "memoryLayer": 0.5,
    }
  },
  "updatedNeighbourhood": {
      "name": "search-service",
      "visualizedObjects": [],
      "layerValues": [],
      "daysOld": 150,
      "team": "search-developers"
  },
  "updatedVisualizedObjects": [{
          "type": "staging-building",
          "size": 7,
          "layerValues": null,
          "identifier": "011774df-0af3-4ff4-9672-f3bd16de5fc2"
        },
        {
          "type": "staging-building",
          "size": 7,
          "layerValues": null,
          "identifier": "12345678-0af3-4ff4-9672-f3bd16de5fc2"
        }
  ]
}
```

**Note:** It is important to set the neighbourhoodName. This is because if a new visualized object is added, the game doesn't know to which neighbourhood this belongs.

### Object types

For now there are 4 types of Visualized Objects that can exists in the game. Of course you can expand these but the following are used in this project:

1. building
2. staging-building
3. vehicle
4. grass (only used in the game, not from the API)

For buildings & vehicles size is relevant.

A simple formula I used for the back-end is `RAM / 10 * Cpu` For example: `1200 / 10 * 0.3 = 36`

Because a better CPU is more expensive but more RAM is more expensive too, so a building looks "better/bigger".

You can edit the configuration to change sizes.

**For now there can only be 1 vehicle per building. It is a TODO for the future to expand this.**

The vehicle should have the same identifier as the building and it automatically drive towards it.

## 4. Features

### 4.1 Chaos Engineering

To use chaos engineering there has been decided to let a tank drive around together with an airplane flying over dropping bombs on buildings.

If a hit happens, a building will collapse and a docker container will real-time be destroyed.

If you click on the tank or the plane in the UI the camera will switch to the vehicle. You can switch back to the normal view by pressing the button again.

There are settings to disable the airplane or tank. There is also a setting where you can set the minimum required buildings.

It is more safe to set this to 2. If you use this in a production environment it could happen that a service with only 1 instance is getting killed.

**When a team is selected, the chaos feature only applies to the selected team! The tank will change route, if it does not meet the requirements, it will go to a stand-by mode.**

To disable chaos engineering, edit the `config.json`.

### 4.2 Traffic

Traffic in the game represents in our case network traffic. Every building can have 1 vehicle at the moment. If a vehicle has reached it's destination, it will drive back to their spawn point to always keep traffic on the move.

You can set the sizes in the configuration

Depending on the size of the input the game will choose a vehicle to use. You can edit this in the configuration.

### 4.3 Layers

Layers are fully dynamic depending on the API. Images should be placed at `BASEURL/images/example_icon.png`.
These images are loaded in the game. You can find the example JSON at paragraph 3.2.

**It is important to use a white icon, since Unity can only colorize white sprites.**

We used the layers for metrics about CPU, Memory and 500 error's.

Layers are managed in the LayerManager class.

You can see the layers in action in the gif at the beginning of this readme.

To disable Layers, edit the `config.json`.

### 4.4 Team selection

Teams can be selected in the dropdown in the top-left corner.

When a team is selected transparency is enabled for every building that does not belong to the selected team.

To disable Team Selection edit the `config.json`.

## 5. Adding your own prefabs

### 5.1 Automatic

If you purchased the assets packs that are listed in chapter 1.2, you can load the config: `config-paid.json` from the Preset config files folder by simply copying it to the root of the project and rename it to `config.json`.
After that, you only need to click on the assets menu and execute the following actions:

1. Make all meshes readable
2. Fix Asset properties and components
3. Build AssetsBundles (read about assets in chapter 2)

And when you look into the Assets/AssetBundles folder, you should see an asset bundle called paid.

Of course you can also do this with your own prefab packs. You only need to edit the config.json to set the correct prefab names, and then execute the actions as listed above.

**NOTE:** If you use the City Adventure pack, the creator forgot to set the right mesh in the mesh collider for all vehicles. The only thing you need to do is go by all vehicles you want to use and set the right mesh on the mesh collider. Otherwise clicking on a vehicle will not working (because of ray casting). 
If you use the simple apocalypse pack, please edit the `SA_Veh_Tank` prefab and tag the turret of the tank as a TankTurret.

### 5.2 Manual adding prefabs

You can add your own prefabs to this game. Like said in 1.2, the prefabs must be 10x10 if you want to use the default tile size.

Steps to take when adding your own prefabs:

1. Import the prefab

2. Place the prefab in the Resources/Prefabs/`TYPE` folder.

3. Give the correct tag to the prefab, if you're adding a tank, don't forget to tag the TankTurret as it rotates when it fires.

### Buildings & Roads

4. Add a NavMeshModifier to the prefab if it's not a vehicle and set the right target.
Check override area and for a road area with walkable selected, for a building select not walkable

### Vehicles

4. Add a NavMeshAgent to the prefab.

For example, for the scooter the following settings were used:

| Setting                 | Value          |
|-------------------------|----------------|
| Agent Type              | Humanoid       |
| Base Offset             | 0.1            |
| Speed                   | 20             |
| Angular speed           | 120            |
| Acceleration            | 8              |
| Stopping distance       | 0              |
| Auto Braking            | Enabled        |
| Radius                  | 0.5            |
| Height                  | 1              |
| Quality                 | None           |
| Priority                | 50             |
| Auto Traverse Off Mesh  | Enabled        |
| Auto Re-path            | Enabled        |
| Area Mask               | Walkable       |

5. If the prefab is a tank or a plane, add a camera to the object, align it correctly and give it the correct Tag `TankCamera` or `PlaneCamera`.

## Important! Buildings & Vehicles

These steps are necessary only if you intend to use the team selection.

**The material must not contain any white spaces.**

6. Place the material you used in the Resources/Materials folder.

7. Duplicate the material and rename the duplicated material to **Transparent-** *Material name*. (I know this is not a really nice thing, but it's because of WebGL limitations)

8. Set the Rendering mode to **Fade** of the transparent material.

9. Edit the `config.json` and set all the prefabs to the correct names.

For performance reasons it is advised to **enable** GPU instancing for the transparent material and the original prefab material.

## 6. Documentation

Most of the code is documented.
This means that if you have knowledge of Unity and C# it should be easy to make your own adjustments.

**Every** manager in the game is a Singleton, all managers can reach to each others publics to retrieve information.

Because the game is dynamic as possible, most of the scripting in the game is event based.
The order of how the game is being loaded:

1. Settings Manager - fires SettingsLoaded event
2. Assets Manager - fires AssetsLoaded event
3. API Manager - fires ApiInitialized event and after that keeps firing ApiUpdate events
4. City Manager - fires CityUpdated event as soon as the grid is changed
5. Grid Manager - fires GridInitialized event as soon as the grid is generated

Any manager can hook into any of these events.
There are other managers that have events as well, but the order does not matter for them.

## 7. Developer note

This project is a graduation project. If you have any questions, feedback or suggestions, feel free to contact us (Harm Weites, Sebastiaan Bekker or Leroy van Dijk)!

## 8. Icons

Icons used in this project are from iconfinder.com created by DesignerzBase.

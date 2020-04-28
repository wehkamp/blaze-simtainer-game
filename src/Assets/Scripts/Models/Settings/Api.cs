using System;

namespace Assets.Scripts.Models.Settings
{
	[Serializable]
	public class Api
	{
		/// <summary>
		/// Base url of the API that is being used. Default empty for WebGL
		/// </summary>
		public string BaseUrl = "";

		/// <summary>
		/// Endpoint for the event hub, this can be changed if you want to implement multiple entities that make the game different.
		/// Leave this blank if you want to disable SignalR.
		/// </summary>
		public string EventHubEndpoint = "/hubs/cloudstack/game";

		/// <summary>
		/// Default endpoint for the game. This endpoint retrieves the current state of the game.
		/// </summary>
		public string GameEndpoint = "/v1/cloudstack/game";
	}
}
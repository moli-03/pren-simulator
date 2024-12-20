using Assets.Src.Vehicle.Graph;
using UnityEngine;

namespace Assets.Src.Vehicle.States {

	public class EndpointReached : VehicleState
	{
		public override string Name => "EndpointReached";

		public EndpointReached(VehicleController vehicle) : base(vehicle) { }

		public override void Start()
		{
			// Start spinning very fast
			this.Vehicle.Drive.TurnRightOnSpot(1f);
		}
	}

}
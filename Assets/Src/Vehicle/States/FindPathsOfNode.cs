using UnityEngine;

namespace Assets.Src.Vehicle.States {

	public class FindPathsOfNode : VehicleState
	{

		public FindPathsOfNode(VehicleController vehicle) : base(vehicle) {
			this.Vehicle.Drive.TurnLeftOnSpot(0.8f);
		}

		public override void Update()
		{
			
		}
	}

}
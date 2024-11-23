using Assets.Src.Vehicle.Graph;
using UnityEngine;

namespace Assets.Src.Vehicle.States {

	public class WaitingOnStartingPosition : VehicleState
	{
		public override string Name => "WaitingOnStartingPosition";

		public WaitingOnStartingPosition(VehicleController vehicle) : base(vehicle) { }

		public override void Update()
		{

			if (Input.GetKeyDown(KeyCode.Return)) {

				// Store the first node
				this.Vehicle.StoreNode(this.Vehicle.Position);

				// New state is searching for paths to find the entrance to the graph
				this.Vehicle.SetState(new FindPathsOfNode(this.Vehicle));
			}
			
		}
	}

}
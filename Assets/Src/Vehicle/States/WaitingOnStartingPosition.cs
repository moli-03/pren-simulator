using Assets.Src.Vehicle.Graph;
using UnityEngine;

namespace Assets.Src.Vehicle.States {

	public class WaitingOnStartingPosition : VehicleState
	{

		public WaitingOnStartingPosition(VehicleController vehicle) : base(vehicle) { }

		public override void Update()
		{

			if (Input.GetKeyDown(KeyCode.Return)) {

				// Set the relative origin of the map
				this.Vehicle.Map.SetOrigin(this.Vehicle.transform.position);

				// Store the node
				this.Vehicle.StoreNode(this.Vehicle.transform.position);

				// New state is searching for paths to find the entrance to the graph
				this.Vehicle.SetState(new FindPathsOfNode(this.Vehicle));
			}
			
		}
	}

}
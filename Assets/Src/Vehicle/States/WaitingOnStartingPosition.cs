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

				UIController.Instance.StartTimer();

				this.Vehicle.SetState(new NodeReached(this.Vehicle));
			}
			
		}
	}

}
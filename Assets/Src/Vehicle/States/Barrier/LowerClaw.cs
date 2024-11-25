using UnityEngine;

namespace Assets.Src.Vehicle.States.Barrier {

	public class LowerClaw : VehicleState
	{
		public override string Name => "LowerClaw";

		public LowerClaw(VehicleController vehicle) : base(vehicle) { }


		public override void Start()
		{
			this.Vehicle.Claw.LowerArm(() => {
				this.Vehicle.SetState(new GrabBarrier(this.Vehicle));
			});
		}

	}

}
using UnityEngine;

namespace Assets.Src.Vehicle.States.Barrier {

	public class LiftBarrier : VehicleState
	{
		public override string Name => "LiftBarrier";

		public LiftBarrier(VehicleController vehicle) : base(vehicle) { }


		public override void Start()
		{
			this.Vehicle.Claw.LiftArm(() => {
				this.Vehicle.SetState(new RepositionBarrier(this.Vehicle));
			});
		}

	}

}
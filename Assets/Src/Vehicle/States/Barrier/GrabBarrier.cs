using UnityEngine;

namespace Assets.Src.Vehicle.States.Barrier {

	public class GrabBarrier : VehicleState
	{
		public override string Name => "GrabBarrier";

		public GrabBarrier(VehicleController vehicle) : base(vehicle) { }


		public override void Start()
		{
			this.Vehicle.Claw.GrabBarrier(this.Vehicle.BottomDistanceSensor.GetHitGameObject(), () => {

				this.Vehicle.SetState(new LiftBarrier(this.Vehicle));
			});
		}

	}

}
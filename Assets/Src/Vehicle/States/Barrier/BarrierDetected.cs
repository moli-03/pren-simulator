using Assets.Src.Util;
using UnityEngine;

namespace Assets.Src.Vehicle.States.Barrier {

	public class BarrierDetected : VehicleState
	{
		public override string Name => "BarrierDetected";

		private float bottomSensorDistance;
		private Vector2 barrierPosition;

		public BarrierDetected(VehicleController vehicle, float bottomSensorDistance) : base(vehicle) {
			this.bottomSensorDistance = bottomSensorDistance;
			this.barrierPosition = this.Vehicle.Position + this.Vehicle.Forward * (this.Vehicle.BottomDistanceSensor.transform.localPosition.z + bottomSensorDistance);
		}


		public override void Start()
		{
			// Slow down
			this.Vehicle.Drive.DriveForwardPercent(0.1f);
		}

		public override void Update()
		{
			float distance = this.Vehicle.BottomDistanceSensor.GetDistance();

			if (distance <= Constants.DISTANCE_TO_BARRIER_FOR_PICKUP) {
				this.Vehicle.Drive.Stop();

				this.Vehicle.SetState(new LowerClaw(this.Vehicle));
			}
		}

	}

}
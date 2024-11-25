using Assets.Src.Util;
using UnityEngine;

namespace Assets.Src.Vehicle.States.Barrier {

	public class RepositionBarrier : VehicleState
	{
		public override string Name => "RepositionBarrier";

		private readonly Vector2 VehicleStartingPosition;

		private readonly float MoveDistance;

		private bool Rotating = false;

		public RepositionBarrier(VehicleController vehicle) : base(vehicle) {
			this.VehicleStartingPosition = this.Vehicle.Position;
			this.MoveDistance = ((Constants.VEHICLE_LENGTH / 2) + Constants.DISTANCE_TO_BARRIER_FOR_PICKUP) * 2;

			// Slowly drive forward
			this.Vehicle.LineFollower.Enable();
			this.Vehicle.LineFollower.SetSpeed(0.2f);
		}

		public override void Update()
		{

			if (this.Rotating) {
				return;
			}

			Vector2 moved = this.Vehicle.Position - this.VehicleStartingPosition;

			// Check if we have reached our destination
			if (moved.magnitude >= this.MoveDistance) {

				// Stop
				this.Vehicle.LineFollower.Disable();
				this.Vehicle.Drive.Stop();

				// Turn around
				this.Rotating = true;
				this.Vehicle.Drive.TurnDeg(180f, () => {

					// Put barrier down
					this.Vehicle.SetState(new ReleaseBarrier(this.Vehicle));
				});

			}
		}

	}

}
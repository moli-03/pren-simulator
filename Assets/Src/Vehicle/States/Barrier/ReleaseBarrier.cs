using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Src.Vehicle.States.Barrier {

	public class ReleaseBarrier : VehicleState
	{
		public override string Name => "ReleaseBarrier";

		public ReleaseBarrier(VehicleController vehicle) : base(vehicle) { }


		public override void Start()
		{
			// Lower the arm
			this.Vehicle.Claw.LowerArm(() => {

				// Release the barrier
				this.Vehicle.Claw.ReleaseBarrier(() => {

					// Move the arm up again
					this.Vehicle.Claw.LiftArm(null);

					// Spin 180 deg and go on
					this.Vehicle.Drive.TurnDeg(180f, () => {

						// Follow line again
						this.Vehicle.SetState(new FollowLine(this.Vehicle));
					});

				});
			});
		}

	}

}
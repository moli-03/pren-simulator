using System.Linq;
using Assets.Src.Util;
using UnityEngine;

namespace Assets.Src.Vehicle.States {

	public class FollowLine : VehicleState
	{

		private float MinDistance = 0.3f;
		private Vector2 StartingPosition;
		private Vector2? CircleDetectedAt = null;

		public FollowLine(VehicleController vehicle) : base(vehicle)
		{
			this.StartingPosition = this.Vehicle.Position;
			this.Vehicle.Drive.DriveForwardPercent(0.8f);
		}

		public override void Update()
		{
			// Drive the min distance
			if ((this.StartingPosition - this.Vehicle.Position).magnitude < this.MinDistance) {
				return;
			}

			if (this.CircleDetectedAt.HasValue) {
				// Go on until we reach the center of the node
				if ((this.CircleDetectedAt.Value - this.Vehicle.Position).magnitude < Constants.NODE_RADIUS) {
					return;
				}

				// Stop the car
				this.Vehicle.Drive.Stop();

				// Add to map
				this.Vehicle.Map.AddNodeAt(this.Vehicle.Position);

				// Circle reached (stupid stuff here)
				this.Vehicle.SetState(new FindPathsOfNode(this.Vehicle));
			}

			// Check if all horizontal sensors hit something
			if (!this.Vehicle.SensorBoard.HorizontalSensors.Any(sensor => !Pathing.IsOnLine(sensor))) {
				this.CircleDetectedAt = this.Vehicle.Position;
			}
		}
	}

}
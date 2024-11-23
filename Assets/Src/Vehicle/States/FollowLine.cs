using System.Linq;
using Assets.Src.Util;
using Assets.Src.Vehicle.Graph;
using UnityEngine;

namespace Assets.Src.Vehicle.States {

	public class FollowLine : VehicleState
	{
		public override string Name => "FollowLine";

		private float MinDistance = 0.3f;
		private Vector2 StartingPosition;
		private MapNode StartingNode;
		private MapPath FollowPath;
		private Vector2? CircleDetectedAt = null;
		private float DefaultRpm;
		private bool Adjusting = false;

		public FollowLine(VehicleController vehicle, MapNode startNode, MapPath followPath) : base(vehicle)
		{
			this.StartingPosition = this.Vehicle.Position;
			this.Vehicle.Drive.DriveForwardPercent(0.4f);
			this.DefaultRpm = this.Vehicle.Drive.LeftWheelRpm;
			this.StartingNode = startNode;
			this.FollowPath = followPath;
		}

		public override void Update()
		{

			// Stop adjusting
			if (this.Adjusting) {

				// Check if only the middle sensor is on the line
				if (!Pathing.IsOnLine(this.Vehicle.SensorBoard.LineFollowSensors[0]) && Pathing.IsOnLine(this.Vehicle.SensorBoard.LineFollowSensors[1]) && !Pathing.IsOnLine(this.Vehicle.SensorBoard.LineFollowSensors[2])) {
					this.Vehicle.Drive.SetLeftWheelRpm(this.DefaultRpm);
					this.Vehicle.Drive.SetRightWheelRpm(this.DefaultRpm);
					this.Adjusting = false;
				}

			}

			// Check if the right line follow sensor is on the line
			if (!this.Adjusting && Pathing.IsOnLine(this.Vehicle.SensorBoard.LineFollowSensors[2])) {

				this.Adjusting = true;

				// If the middle sensor is not on the line we have to drastically adjust
				if (!Pathing.IsOnLine(this.Vehicle.SensorBoard.LineFollowSensors[1])) {

					// Only need to adjust drastically
					this.Vehicle.Drive.SetLeftWheelRpm(this.DefaultRpm + 150);
					this.Vehicle.Drive.SetRightWheelRpm(this.DefaultRpm - 50);
				}
				else {
					// Only need to adjust slightly
					this.Vehicle.Drive.SetLeftWheelRpm(this.DefaultRpm + 50);
					this.Vehicle.Drive.SetRightWheelRpm(this.DefaultRpm);
				}

			}

			// Check if the left line follow sensor is on the line
			if (!this.Adjusting && Pathing.IsOnLine(this.Vehicle.SensorBoard.LineFollowSensors[0])) {
				
				this.Adjusting = true;

				// If the middle sensor is not on the line we have to drastically adjust
				if (!Pathing.IsOnLine(this.Vehicle.SensorBoard.LineFollowSensors[1])) {
					this.Vehicle.Drive.Stop();

					// Only need to adjust drastically
					this.Vehicle.Drive.SetRightWheelRpm(this.DefaultRpm + 150);
					this.Vehicle.Drive.SetLeftWheelRpm(this.DefaultRpm - 50);
				}
				else {

					// Only need to adjust slightly
					this.Vehicle.Drive.SetRightWheelRpm(this.DefaultRpm + 50);
					this.Vehicle.Drive.SetLeftWheelRpm(this.DefaultRpm);
				}

			}


			// Check if we hit a circle
			if (this.CircleDetectedAt.HasValue) {

				// Go on until we roughly reach the center of the node
				if ((this.CircleDetectedAt.Value - this.Vehicle.Position).magnitude < Constants.NODE_RADIUS / 5) {
					return;
				}

				// Stop the car
				this.Vehicle.Drive.Stop();

				// Node reached -> try to go to the center
				this.Vehicle.SetState(new MoveToCenter(this.Vehicle));
			}

			// Check if all horizontal sensors hit something
			if (!this.Vehicle.SensorBoard.HorizontalSensors.Any(sensor => !Pathing.IsOnLine(sensor))) {

				// Drive for at least the min distance
				if ((this.StartingPosition - this.Vehicle.Position).magnitude < this.MinDistance) {
					return;
				}

				this.CircleDetectedAt = this.Vehicle.Position;
				return;
			}
		
		}
	}

}
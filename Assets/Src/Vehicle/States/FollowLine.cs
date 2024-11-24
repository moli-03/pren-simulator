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
		private float DefaultRpm;
		private bool Adjusting = false;
		private Vector2? FrontSensorsFirstHitAt = null;
		private Vector2? TargetPosition = null;

		public FollowLine(VehicleController vehicle, MapNode startNode) : base(vehicle)
		{
			this.StartingPosition = this.Vehicle.Position;
			this.Vehicle.Drive.DriveForwardPercent(0.6f);
			this.DefaultRpm = this.Vehicle.Drive.LeftWheelRpm;
			this.StartingNode = startNode;
		}

		private void HandleLineFollowing() {

			// Handle stopping of adjusting
			if (this.Adjusting) {

				// Check if only the middle sensor is on the line
				if (
					!Pathing.IsOnLine(this.Vehicle.SensorBoard.FrontLineFollowSensors[1]) // Left not
					&& 
					Pathing.IsOnLine(this.Vehicle.SensorBoard.FrontLineFollowSensors[2]) // Middle yes
					&&
					!Pathing.IsOnLine(this.Vehicle.SensorBoard.FrontLineFollowSensors[3]) // Right not
				) {

					// Stop adjusting
					this.Vehicle.Drive.SetLeftWheelRpm(this.DefaultRpm);
					this.Vehicle.Drive.SetRightWheelRpm(this.DefaultRpm);
					this.Adjusting = false;
				}

			}

			// Check if the right line follow sensor is on the line
			if (!this.Adjusting && Pathing.IsOnLine(this.Vehicle.SensorBoard.FrontLineFollowSensors[3])) {

				// If the outer right sensor is on the line its because we hit another line
				if (Pathing.IsOnLine(this.Vehicle.SensorBoard.FrontLineFollowSensors[4])) {
					return;
				}

				this.Adjusting = true;

				// If the middle sensor is not on the line we have to drastically adjust
				if (!Pathing.IsOnLine(this.Vehicle.SensorBoard.FrontLineFollowSensors[2])) {

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
			if (!this.Adjusting && Pathing.IsOnLine(this.Vehicle.SensorBoard.FrontLineFollowSensors[1])) {

				// If the outer left sensor is on the line its because we hit another line
				if (Pathing.IsOnLine(this.Vehicle.SensorBoard.FrontLineFollowSensors[0])) {
					return;
				}

				
				this.Adjusting = true;

				// If the middle sensor is not on the line we have to drastically adjust
				if (!Pathing.IsOnLine(this.Vehicle.SensorBoard.FrontLineFollowSensors[2])) {

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
		}


		public override void Update()
		{

			// Drive for at least the min distance
			if ((this.StartingPosition - this.Vehicle.Position).magnitude >= this.MinDistance) {

				// Check for a first hit of the front sensors
				if (!this.FrontSensorsFirstHitAt.HasValue) {

					if (this.Vehicle.SensorBoard.FrontLineFollowSensors.All(sensor => Pathing.IsOnLine(sensor))) {
						this.FrontSensorsFirstHitAt = this.Vehicle.Position;
					}
				}
				else if (this.FrontSensorsFirstHitAt.HasValue && !this.TargetPosition.HasValue) {

					// Now any of the front sensors has to leave the circle
					if (this.Vehicle.SensorBoard.FrontLineFollowSensors.Any(sensor => !Pathing.IsOnLine(sensor))) {

						float distanceTraveled = (this.Vehicle.Position - this.FrontSensorsFirstHitAt.Value).magnitude;
						this.TargetPosition = this.Vehicle.Position + this.Vehicle.Forward * distanceTraveled / 2f;

						// Slowly drive forward
						this.Vehicle.Drive.DriveForwardPercent(0.1f);
					}
				}
				else if (this.TargetPosition.HasValue) {

					float distanceToTarget = (this.TargetPosition.Value - this.Vehicle.Position).magnitude;

					if (distanceToTarget <= 0.007f) { // 7mm tolerance
					
						// Check if we have already found a node on that position
						MapNode node = this.Vehicle.Map.GetNodeAt(this.Vehicle.Position);

						if (node != null) {

							// Add the already visited node to the history
							this.Vehicle.NodeHistory.Add(node);
						}
						else {

							// Add the new node to the map
							this.Vehicle.StoreNode(this.Vehicle.Position);
						}

						this.Vehicle.SetState(new FindPathsOfNode(this.Vehicle));
						return;
					}
				}
			}


			this.HandleLineFollowing();
		}
	}

}
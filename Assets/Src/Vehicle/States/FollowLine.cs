using System.Linq;
using Assets.Src.Util;
using Assets.Src.Vehicle.Graph;
using UnityEngine;

namespace Assets.Src.Vehicle.States {

	public class FollowLine : VehicleState
	{

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
				if (!Pathing.IsOnLine(this.Vehicle.SensorBoard.FrontSensors[0]) && Pathing.IsOnLine(this.Vehicle.SensorBoard.FrontSensors[1]) && !Pathing.IsOnLine(this.Vehicle.SensorBoard.FrontSensors[2])) {
					this.Vehicle.Drive.SetLeftWheelRpm(this.DefaultRpm);
					this.Vehicle.Drive.SetRightWheelRpm(this.DefaultRpm);
					this.Adjusting = false;
				}

			}

			// Adjust to left
			if (!this.Adjusting && Pathing.IsOnLine(this.Vehicle.SensorBoard.FrontSensors[0]) && Pathing.IsOnLine(this.Vehicle.SensorBoard.FrontSensors[1])) {
				this.Vehicle.Drive.SetRightWheelRpm(this.Vehicle.Drive.RightWheelRpm + 50);
				this.Vehicle.Drive.SetLeftWheelRpm(this.DefaultRpm);
				this.Adjusting = true;
			}

			// Adjust to right
			if (!this.Adjusting && Pathing.IsOnLine(this.Vehicle.SensorBoard.FrontSensors[1]) && Pathing.IsOnLine(this.Vehicle.SensorBoard.FrontSensors[2])) {
				this.Vehicle.Drive.SetLeftWheelRpm(this.Vehicle.Drive.LeftWheelRpm + 50);
				this.Vehicle.Drive.SetRightWheelRpm(this.DefaultRpm);
				this.Adjusting = true;
			}

			// Drive the min distance
			if ((this.StartingPosition - this.Vehicle.Position).magnitude < this.MinDistance) {
				return;
			}


			// Check if we hit a circle
			if (this.CircleDetectedAt.HasValue) {
				// Go on until we reach the center of the node
				if ((this.CircleDetectedAt.Value - this.Vehicle.Position).magnitude < Constants.NODE_RADIUS / 16) {
					return;
				}

				// Stop the car
				this.Vehicle.Drive.Stop();

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


				// Circle reached (stupid stuff here)
				this.Vehicle.SetState(new FindPathsOfNode(this.Vehicle));
			}

			// Check if all front sensors hit something
			if (!this.Vehicle.SensorBoard.HorizontalSensors.Any(sensor => !Pathing.IsOnLine(sensor))) {
				this.CircleDetectedAt = this.Vehicle.Position;
				return;
			}
		
		}
	}

}
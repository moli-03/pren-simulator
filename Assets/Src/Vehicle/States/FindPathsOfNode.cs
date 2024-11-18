using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Src.Util;
using Assets.Src.Vehicle.Graph;
using UnityEngine;

namespace Assets.Src.Vehicle.States {

	public class FindPathsOfNode : VehicleState
	{
		private bool Turned = false;
		private bool StartedOnLine = false;
		private bool Turning = false;
		private bool LinePreviouslyFound = false;
		private MapNode CurrentNode;
		private List<LineEdgePoints> EdgePoints = new List<LineEdgePoints>();

		struct LineEdgePoints {
			public Vector2 Left;
			public Vector2 Right;
		}


		public FindPathsOfNode(VehicleController vehicle) : base(vehicle) {

			this.CurrentNode = this.Vehicle.NodeHistory.Last();

			if (Pathing.IsOnLine(this.Vehicle.SensorBoard.FrontSensor)) {
				this.StartedOnLine = true;
				// Start slowly rotating left
				this.Vehicle.Drive.TurnLeftOnSpot(0.1f);
			}
			else {
				this.StartScanningForPaths();
			}
		}

		private void AddPaths() {

			// Add outgoing paths
			this.EdgePoints.ForEach(edgePoint => {

				if (edgePoint.Left == null || edgePoint.Right == null) {
					return;
				}

				// Figure out the middle of the line
				Vector2 middle = edgePoint.Left + 0.5f * (edgePoint.Right - edgePoint.Left);
				Vector2 direction = middle - this.CurrentNode.Position;

				this.CurrentNode.AddOutgoingPath(direction);
			});

		}


		private void StartScanningForPaths() {

			this.Turning = true;
			this.Turned = false;
			this.LinePreviouslyFound = false;

			this.Vehicle.Drive.TurnDeg(360, () => {

				this.Turned = true;

				// Add the found paths to the node
				this.AddPaths();

				// TODO: Dont chose randomly
				int index = (new System.Random()).Next(0, this.CurrentNode.OutgoingPaths.Count - 1);
				MapPath chosenPath = this.CurrentNode.OutgoingPaths[index];

				// Turn to that line
				this.Vehicle.Drive.RotateFacing(chosenPath.Direction, () => {
				
					// Start to follow that line
					this.Vehicle.SetState(new FollowLine(this.Vehicle));
				});
				return;
			});
		}

		public override void Update()
		{
			if (this.Turned) {
				return;
			}

			// Check if the front sensor is on the line
			bool isFrontSensorOnLine = Pathing.IsOnLine(this.Vehicle.SensorBoard.FrontSensor);

			// Keep spinning until its no longer on the line
			if (this.StartedOnLine && !this.Turning) {
				if (isFrontSensorOnLine) {
					return;
				}

				// Behind line now
				this.StartScanningForPaths();
				return;
			}

			// Check if we are hovering a line
			if (isFrontSensorOnLine) {

				// Yes -> were we previously on the line?
				if (LinePreviouslyFound) {
					return;
				}

				LinePreviouslyFound = true;
				this.EdgePoints.Add(new LineEdgePoints() {
					Left = this.Vehicle.Position + this.Vehicle.Forward * this.Vehicle.SensorBoard.FrontSensorDistance
				});
			}
			else {

				// No longer on a line?
				if (this.LinePreviouslyFound) {
					var currentEdgePoint = this.EdgePoints.Last();
					currentEdgePoint.Right = this.Vehicle.Position + this.Vehicle.Forward * this.Vehicle.SensorBoard.FrontSensorDistance;
					this.EdgePoints[this.EdgePoints.Count - 1] = currentEdgePoint;
					this.LinePreviouslyFound = false;
				}

			}

		}
	}

}
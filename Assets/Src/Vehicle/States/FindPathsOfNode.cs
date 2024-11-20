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
		private bool StartedOnLineFoundLine = false;
		private bool LinePreviouslyHovered = false;
		private MapNode CurrentNode;
		private List<LineEdgePoints> EdgePoints = new List<LineEdgePoints>();

		struct LineEdgePoints {
			public Vector2 Left;
			public Vector2 Right;
		}


		public FindPathsOfNode(VehicleController vehicle) : base(vehicle) {

			this.CurrentNode = this.Vehicle.NodeHistory.Last();

			// Check if we have already scanned the outgoing paths for that node
			if (this.CurrentNode.OutgoingPathsScanned) {

				// If so, directly choose the next path
				this.ChooseNextPath();
				return;
			}

			// Check if the front middle sensor is on the line
			if (Pathing.IsOnLine(this.Vehicle.SensorBoard.FrontSensors[1])) {

				this.StartedOnLine = true;
				// Start slowly rotating left
				this.Vehicle.Drive.TurnLeftOnSpot(0.2f);
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

			this.CurrentNode.OutgoingPathsScanned = true;
		}


		private void StartScanningForPaths() {

			this.Turned = false;
			this.LinePreviouslyHovered = false;

			this.Vehicle.Drive.TurnDeg(360, () => {

				this.Turned = true;

				// Add the found paths to the node
				this.AddPaths();

				this.ChooseNextPath();
			});
		}


		private void ChooseNextPath() {

			// TODO: Dont chose randomly
			int index = (new System.Random()).Next(0, this.CurrentNode.OutgoingPaths.Count - 1);
			MapPath chosenPath = this.CurrentNode.OutgoingPaths[index];

			// Turn to that line
			this.Vehicle.Drive.RotateFacing(chosenPath.Direction, () => {
				
				// Start to follow that line
				this.Vehicle.SetState(new FollowLine(this.Vehicle, this.CurrentNode, chosenPath));
			});
			return;
		}

		public new void FixedUpdate()
		{
			if (this.Turned) {
				return;
			}

			// Check if the front sensor is on the line
			bool isFrontSensorOnLine = Pathing.IsOnLine(this.Vehicle.SensorBoard.FrontFrontSensorTimmyStuff);

			Vector3 position = Pathing.ToWorldPosition(this.Vehicle.Position + this.Vehicle.Forward * 0.14f);
			position.y = isFrontSensorOnLine ? 0.03f : 0.01f;
			if (isFrontSensorOnLine) {
				Draw.DrawCircle(position, isFrontSensorOnLine ? Color.cyan : Color.magenta);
			}

			// Keep spinning until its no longer on the line
			if (this.StartedOnLine && !this.StartedOnLineFoundLine) {
				if (isFrontSensorOnLine) {
					return;
				}

				this.StartedOnLineFoundLine = true;

				// Behind line now
				this.StartScanningForPaths();
				return;
			}

			// Check if we are hovering a line
			if (isFrontSensorOnLine) {

				// Were we previously on a line?
				if (this.LinePreviouslyHovered) {
					return;
				}

				this.LinePreviouslyHovered = true;

				// Initially store the position
				this.EdgePoints.Add(new LineEdgePoints() {
					Left = this.Vehicle.Position + this.Vehicle.Forward * this.Vehicle.SensorBoard.FrontSensorDistance
				});
			}
			else {

				// No longer on a line?
				if (this.LinePreviouslyHovered) {

					// Get the last edge point duo we hovered
					var currentEdgePoint = this.EdgePoints.Last();

					// Set the right
					currentEdgePoint.Right = this.Vehicle.Position + this.Vehicle.Forward * this.Vehicle.SensorBoard.FrontSensorDistance;
					this.EdgePoints[this.EdgePoints.Count - 1] = currentEdgePoint;
					this.LinePreviouslyHovered = false;
				}

			}

		}
	}

}
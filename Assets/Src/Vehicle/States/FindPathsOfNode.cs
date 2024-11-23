using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Assets.Src.Util;
using Assets.Src.Vehicle.Graph;
using UnityEngine;

namespace Assets.Src.Vehicle.States {

	public class FindPathsOfNode : VehicleState
	{
		public override string Name => "FindPathsOfNode";

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
			if (Pathing.IsOnLine(this.Vehicle.SensorBoard.LineFollowSensors[1])) {

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

			this.LinePreviouslyHovered = false;

			this.Vehicle.Drive.TurnDeg(360, () => {

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


		public override void Update()
		{
			bool isFrontSensorOnLine = Pathing.IsOnLine(this.Vehicle.SensorBoard.PathDetectionSensor);

			// Handle rotating until no longer on a line
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

				Vector3 position = Pathing.ToWorldPosition(this.Vehicle.Position + this.Vehicle.Forward * this.Vehicle.SensorBoard.PathDetectionSensorDistanceFromCenter);
				position.y = 0.01f;
				Draw.DrawCircle(position, Color.green);

				// Initially store the position
				this.EdgePoints.Add(new LineEdgePoints() {
					Left = this.Vehicle.Position + this.Vehicle.Forward * this.Vehicle.SensorBoard.PathDetectionSensorDistanceFromCenter
				});
			}
			else {

				// No longer on a line?
				if (this.LinePreviouslyHovered) {

					// Get the last edge point duo we hovered
					var currentEdgePoint = this.EdgePoints.Last();

					// Set the right
					currentEdgePoint.Right = this.Vehicle.Position + this.Vehicle.Forward * this.Vehicle.SensorBoard.PathDetectionSensorDistanceFromCenter;
					this.EdgePoints[this.EdgePoints.Count - 1] = currentEdgePoint;
					this.LinePreviouslyHovered = false;

					Vector3 position = Pathing.ToWorldPosition(this.Vehicle.Position + this.Vehicle.Forward * this.Vehicle.SensorBoard.PathDetectionSensorDistanceFromCenter);
					position.y = 0.01f;
					Draw.DrawCircle(position, Color.magenta);
				}
			}
		}

	}
}